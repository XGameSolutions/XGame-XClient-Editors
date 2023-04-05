using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ShaderTool
{
    public class YamlFileSecondInfo
    {
        public int index;
        public int passType;
        public List<string> keywords = new List<string>();
        public bool valid { get; set; }
        private string m_StringInfo = null;

        public YamlFileSecondInfo()
        {
        }
        override public string ToString()
        {
            if (m_StringInfo == null)
                m_StringInfo = string.Format("passtype={0},keywords={1}", passType, string.Join("", keywords));
            return m_StringInfo;
        }
    }
    public class YamlFileFirstInfo
    {
        public int index;
        public long fileId;
        public string guid;
        public string path;
        public int type;
        public Shader shader;
        public List<YamlFileSecondInfo> seconds = new List<YamlFileSecondInfo>();
        public List<ShaderVariantCollection.ShaderVariant> variants = new List<ShaderVariantCollection.ShaderVariant>();

        public List<string> selectedAvailableKeywords = new List<string>();
        public List<string> excludedKeywords = new List<string>();

        public string uuid { get { return guid + "_" + fileId; } }

        public YamlFileFirstInfo()
        {
        }

        public string shaderName
        {
            get { return shader == null ? "<MISSING SHADER>" : shader.name; }
        }

        public virtual bool AddSecond(YamlFileSecondInfo second)
        {
            seconds.Add(second);
            return true;
        }

        public bool ContainsSecond(YamlFileSecondInfo info)
        {
            foreach (var second in seconds)
            {
                if (second.ToString().Equals(info.ToString())) return true;
            }
            return false;
        }

        public bool ContainsVariant(ShaderVariantCollection.ShaderVariant variant)
        {
            return variants.Contains(variant);
        }

        public bool RemoveSecond(int index)
        {
            if (index >= 0 && index < seconds.Count)
            {
                seconds.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsContainsAnyExculedKeywords(List<string> keywords)
        {
            foreach (var keyword in excludedKeywords)
            {
                if (keywords.Contains(keyword)) return true;
            }
            return false;
        }
    }
    public class ShaderVariantsYamlFileInfo<T, S>
        where T : YamlFileFirstInfo, new()
        where S : YamlFileSecondInfo, new()
    {
        private string m_Name;
        private string m_YamlFilePath;
        private string m_YamlFileInfoPath;
        private string m_YamlFileHeader;
        private List<T> m_Firsts = new List<T>();

        public List<T> firsts { get { return m_Firsts; } }

        public ShaderVariantsYamlFileInfo(string yamlFilePath)
        {
            Parse(yamlFilePath);
        }

        public bool ContainsShader(Shader shader)
        {
            foreach (var first in m_Firsts)
            {
                if (first.shaderName.Equals(shader.name)) return true;
            }
            return false;
        }

        public bool AddFirst(Shader shader, string guid, long fileId)
        {
            if (!ContainsShader(shader))
            {
                T first = new T();
                first.shader = shader;
                first.guid = guid;
                first.fileId = fileId;
                first.type = fileId == 4800000 ? 3 : 0;
                m_Firsts.Add(first);
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetAllVariantsCount()
        {
            var count = 0;
            foreach (var first in m_Firsts)
                foreach (var second in first.seconds)
                    count++;
            return count;
        }

        private void Parse(string yamlFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(yamlFilePath);
            m_YamlFilePath = yamlFilePath;
            m_YamlFileInfoPath = Path.GetDirectoryName(yamlFilePath) + "/" + fileName + ".info";
            if (!File.Exists(m_YamlFilePath))
            {
                Debug.LogError("can't find:" + m_YamlFilePath);
            }
            var infoDic = new Dictionary<string, List<List<string>>>();
            if (File.Exists(m_YamlFileInfoPath))
            {
                var lineList = File.ReadAllLines(m_YamlFileInfoPath);
                var shaderName = "";
                for (int i = 0; i < lineList.Length; i++)
                {
                    var line = lineList[i];
                    if (line.StartsWith("shader name:"))
                    {
                        shaderName = line.Split(':')[1];
                        infoDic[shaderName] = new List<List<string>>(){
                            new List<string>(),
                            new List<string>(),
                        };
                    }
                    else if (line.StartsWith("selected keywords:"))
                    {
                        var list = infoDic[shaderName][0];
                        list.AddRange(line.Split(':')[1].Split(' '));
                    }
                    else if (line.StartsWith("excluded keywords:"))
                    {
                        var list = infoDic[shaderName][1];
                        list.AddRange(line.Split(':')[1].Split(' '));
                    }
                }
            }
            else
            {
                Debug.LogError("can't find info:" + m_YamlFileInfoPath);
            }
            var textList = File.ReadAllLines(yamlFilePath);
            T first = null;
            S second = null;
            bool keywordStart = false;
            var isHeadString = true;
            var builtinAssets = AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");
            var fieldIdDic = new Dictionary<long, Object>();
            foreach (var obj in builtinAssets)
            {
                string guid = null;
                long fileId = 0;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out fileId))
                {
                    fieldIdDic[fileId] = obj;
                }
            }
            for (int i = 0; i < textList.Length; i++)
            {

                if (isHeadString)
                {
                    m_YamlFileHeader += textList[i] + "\n";
                }
                var text = textList[i].Trim();
                if (text.StartsWith("m_Shaders:"))
                {
                    isHeadString = false;
                }
                if (text.StartsWith("m_Name:"))
                {
                    m_Name = text.Split(':')[1].Trim();
                }
                else if (text.StartsWith("- first:"))
                {
                    first = null;
                    var temp = text.Split(':');
                    if (temp.Length < 5) continue;
                    first = new T();
                    first.fileId = long.Parse(temp[2].Trim().Split(',')[0]);
                    first.guid = temp[3].Trim().Split(',')[0];
                    first.type = int.Parse(temp[4].Trim().Split('}')[0]);
                    first.path = AssetDatabase.GUIDToAssetPath(first.guid);
                    if (first.type == 0)
                    {
                        var obj = fieldIdDic[first.fileId];
                        first.shader = obj as Shader;
                    }
                    else
                    {
                        first.shader = AssetDatabase.LoadAssetAtPath<Shader>(first.path);
                    }
                    if (first.shader == null)
                    {
                        Debug.LogError("shader is null:" + first.fileId + "," + first.guid);
                    }
                    else
                    {
                        //ShaderDatabase.GetShaderKeywords(first.shader, ref first.remainingKeywords);
                    }
                    if (infoDic.ContainsKey(first.shaderName))
                    {
                        first.selectedAvailableKeywords.AddRange(infoDic[first.shaderName][0]);
                        first.excludedKeywords.AddRange(infoDic[first.shaderName][1]);
                    }
                    m_Firsts.Add(first);
                }
                else if (text.StartsWith("passType:"))
                {
                    if (first != null)
                    {
                        second.passType = int.Parse(text.Split(':')[1].Trim());
                        var shaderName = first.shaderName;
                        if (first.ContainsSecond(second))
                        {
                            Debug.LogError(string.Format("second already exist:shader={0},second={1}",
                            shaderName, second.ToString()));
                        }
                        else
                        {
                            first.AddSecond(second);
                            try
                            {
                                var variantInfo = new ShaderVariantCollection.ShaderVariant(
                                first.shader,
                                (UnityEngine.Rendering.PassType)second.passType,
                                second.keywords.ToArray()
                                );
                                if (!first.variants.Contains(variantInfo))
                                {
                                    first.variants.Add(variantInfo);
                                }
                                else
                                {
                                    Debug.LogError(string.Format("variants already exist:shader={0},pass={1},keywords={2}",
                                    shaderName, second.passType, string.Join(" ", second.keywords)));
                                }
                            }
                            catch (System.Exception e)
                            {
                                second.valid = false;
                                Debug.LogError(string.Format("convert to variant ERROR:shader={0},pass={1},errormsg={2}",
                                shaderName, second.passType, e.Message));
                                Debug.LogError(string.Format("convert to variant ERROR:shader={0},pass={1},keywords={2}",
                                shaderName, second.passType, string.Join(" ", second.keywords)));
                            }
                        }

                    }
                }
                else if (text.StartsWith("- keywords:"))
                {
                    keywordStart = true;
                    Debug.LogError("keyword start:" + text);
                    if (first != null)
                    {
                        second = new S();
                        var keys = text.Split(':')[1].Trim().Split(' ');
                        foreach (var key in keys)
                        {
                            var keyword = key.Trim();
                            if (!string.IsNullOrEmpty(keyword))
                            {
                                //if (first.remainingKeywords.Contains(keyword))
                                second.keywords.Add(keyword);
                            }
                        }
                    }
                }
                else
                {
                    if (keywordStart && text.IndexOf(":") < 0)
                    {
                        if (first != null)
                        {
                            var keys = text.Trim().Split(' ');
                            foreach (var key in keys)
                            {
                                var keyword = key.Trim();
                                if (!string.IsNullOrEmpty(keyword))
                                {
                                    //if (first.remainingKeywords.Contains(keyword))
                                    second.keywords.Add(keyword);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SaveToFile()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m_YamlFileHeader);
            foreach (var first in m_Firsts)
            {
                sb.AppendFormat("  - first: {0}fileID: {1}, guid: {2}, type: {3}{4}\n", "{", first.fileId, first.guid, first.type, "}");
                sb.AppendFormat("    second:\n");
                if (first.seconds.Count == 0) sb.AppendFormat("      variants: []\n");
                else sb.AppendFormat("      variants:\n");
                foreach (var second in first.seconds)
                {
                    second.keywords.Sort();
                    sb.AppendFormat("      - keywords:");
                    int count = 0;
                    for (int i = 0; i < Mathf.Min(4, second.keywords.Count); i++)
                    {
                        sb.AppendFormat(" {0}", second.keywords[i]);
                        count++;
                    }
                    while (count < second.keywords.Count)
                    {
                        int min = count;
                        sb.Append("\n          ");
                        for (int i = min; i < Mathf.Min(min + 5, second.keywords.Count); i++)
                        {
                            sb.AppendFormat(" {0}", second.keywords[i]);
                            count++;
                        }
                    }
                    // for (int i = 0; i < second.keywords.Count; i++)
                    // {
                    //     sb.AppendFormat(" {0}", second.keywords[i]);
                    // }
                    if (second.keywords.Count == 0) sb.Append(" ");
                    sb.Append("\n");
                    sb.AppendFormat("        passType: {0}\n", second.passType);
                }
            }
            File.WriteAllText(m_YamlFilePath, sb.ToString());
            sb.Length = 0;
            foreach (var first in m_Firsts)
            {
                if (first.shader != null && (first.selectedAvailableKeywords.Count > 0 || first.excludedKeywords.Count > 0))
                {
                    sb.AppendFormat("shader name:{0}\n", first.shaderName);
                    sb.AppendFormat("selected keywords:{0}\n", string.Join(" ", first.selectedAvailableKeywords));
                    sb.AppendFormat("excluded keywords:{0}\n", string.Join(" ", first.excludedKeywords));
                }
            }
            File.WriteAllText(m_YamlFileInfoPath, sb.ToString());
        }
    }
}
