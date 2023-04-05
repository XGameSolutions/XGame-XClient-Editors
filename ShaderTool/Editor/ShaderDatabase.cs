using System.Collections.Generic;
using UnityEngine;

namespace ShaderTool
{
    internal class ShaderDatabase
    {
        public static string VARIANTS_PATH = "ResShader/shadervariants.shadervariants";
        private static ShaderVariantsYamlFileInfo<ShaderInfo, ShaderVariantsInfo> s_ShaderVariantsFileInfo;

        private static bool s_ShaderDirty = false;

        public static bool IsShaderDirty()
        {
            if (s_ShaderDirty)
            {
                s_ShaderDirty = false;
                return true;
            }
            return false;
        }

        public static void Refresh(string assetPath)
        {
            var path = Application.dataPath + "/" + assetPath;
            s_ShaderVariantsFileInfo = new ShaderVariantsYamlFileInfo<ShaderInfo, ShaderVariantsInfo>(path);
            s_ShaderDirty = true;
        }

        public static List<ShaderInfo> GetShaderInfoList()
        {
            return s_ShaderVariantsFileInfo.firsts;
        }

        public static void SaveToFile()
        {
            s_ShaderVariantsFileInfo.SaveToFile();
        }
    }
}