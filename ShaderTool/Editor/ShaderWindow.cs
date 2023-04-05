
using UnityEngine;
using UnityEditor;
using XCommon.Editor;
using System.Collections.Generic;

namespace ShaderTool.Editor
{

    internal class ShaderWindow : EditorWindow, ITianGlyphPanelParent
    {
        class Styles
        {
            public static readonly GUIStyle invisibleButton = "InvisibleButton";
        }
        private static ShaderWindow s_Instance;
        internal static ShaderWindow Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GetWindow<ShaderWindow>();
                }
                return s_Instance;
            }
        }

        [SerializeField] private TianGlyphPanel m_Panel;
        [SerializeField] private string m_VariantsAssetPath = ShaderDatabase.VARIANTS_PATH;
        private XEditorTable m_ShaderTree;
        private XEditorTable m_ShaderVariantsTree;
        private GUIContent m_RefreshTexture;
        private GUIContent m_SaveTexture;
        private Rect m_Position;
        const float k_MenubarHeight = 20f;
        const float k_MenubarXGap = 1f;
        const float k_MenubarYGap = 4f;


        [MenuItem("X/ShaderTool/ShaderWindow")]
        static void ShowWindow()
        {
            s_Instance = null;
            Instance.titleContent = new GUIContent("ShaderWindow");
            Instance.Show();
        }

        internal void RefreshShaderList()
        {
            ShaderDatabase.Refresh(m_VariantsAssetPath);
        }

        private void OnEnable()
        {
            if (m_Panel == null)
            {
                m_Panel = new TianGlyphPanel();
            }
            m_Panel.OnEnable(this);
            RefreshShaderList();
            m_RefreshTexture = new GUIContent(EditorGUIUtility.FindTexture("Refresh"), "Refresh shader from file");
            m_SaveTexture = new GUIContent(EditorGUIUtility.FindTexture("SaveAs"), "save shader variants to file");
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            if (m_ShaderTree != null)
            {
                if (ShaderDatabase.IsShaderDirty())
                {
                    m_ShaderTree.UpdateInfoList(ShaderDatabase.GetShaderInfoList());
                }
            }
        }

        private void OnGUI()
        {
            InitPanel();
            OnGUIMenu();
            OnGUIPanel();
        }

        private void OnGUIMenu()
        {
            var rectMenu = new Rect(k_MenubarXGap, k_MenubarYGap,
                position.width - 2 * k_MenubarXGap, k_MenubarHeight);
            var rectRefresh = new Rect(rectMenu.x, rectMenu.y, k_MenubarHeight, k_MenubarHeight);
            if (GUI.Button(rectRefresh, m_RefreshTexture, Styles.invisibleButton))
            {
                RefreshShaderList();
            }
            var refreshWidth = rectRefresh.x + k_MenubarHeight + k_MenubarXGap;
            var rectPath = new Rect(refreshWidth, rectMenu.y, rectMenu.width - refreshWidth - k_MenubarHeight, k_MenubarHeight);
            m_VariantsAssetPath = GUI.TextField(rectPath, m_VariantsAssetPath);

            var rectSave = new Rect(rectMenu.width - k_MenubarHeight, rectMenu.y, k_MenubarHeight, k_MenubarHeight);
            if (GUI.Button(rectSave, m_SaveTexture, Styles.invisibleButton))
            {
                ShaderDatabase.SaveToFile();
            }
        }

        private void OnGUIPanel()
        {
            var rectPanel = new Rect(0, k_MenubarHeight + 2 * k_MenubarYGap,
                position.width, position.height - k_MenubarHeight - 2 * k_MenubarYGap);
            m_Panel.OnGUI(rectPanel);
        }

        private void InitPanel()
        {
            InitShaderTree();
            InitShaderVariantsTree();
        }

        private void InitShaderTree()
        {
            if (m_ShaderTree != null) return;
            var column = ShaderInfo.totalColumn;
            m_ShaderTree = XEditorTable.CreateTable(column, true);
            for (int i = 0; i < column; i++)
            {
                m_ShaderTree.SetColumnHeader(i, ShaderInfo.GetColumnHeader(i));
            }
            m_ShaderTree.OnSelectionChanged = OnSelectedShaderList;
            m_Panel.SetLeftTopPanel(m_ShaderTree, false);
        }

        private void InitShaderVariantsTree()
        {
            if (m_ShaderVariantsTree != null) return;
            var column = ShaderVariantsInfo.totalColumn;
            m_ShaderVariantsTree = XEditorTable.CreateTable(column, true);
            for (int i = 0; i < column; i++)
            {
                m_ShaderVariantsTree.SetColumnHeader(i, ShaderVariantsInfo.GetColumnHeader(i));
            }
            m_ShaderVariantsTree.OnSelectionChanged = OnSelectedVariantList;
            m_Panel.SetRigthBottomPanel(m_ShaderVariantsTree, false);
        }

        private void OnSelectedShaderList(List<XIEditorTableItemInfo> shaderList)
        {
            if (shaderList.Count > 0)
            {
                var selectedShader = shaderList[0] as ShaderInfo;
                var list = selectedShader.seconds.ConvertAll<ShaderVariantsInfo>(info => info as ShaderVariantsInfo);
                m_ShaderVariantsTree.UpdateInfoList(list);
            }
            else
            {
                m_ShaderVariantsTree.UpdateInfoList(null);
            }
        }
        private void OnSelectedVariantList(List<XIEditorTableItemInfo> assetsList)
        {
            var list = assetsList.ConvertAll<ShaderVariantsInfo>(info => info as ShaderVariantsInfo);
            //m_ShaderTree.SetSelectedAssets(list);
        }
    }
}