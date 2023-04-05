
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;

namespace ShaderTool
{
    internal class ShaderVariantsInfo : YamlFileSecondInfo, XIEditorTableItemInfo
    {
        public ShaderVariantsInfo()
        {
        }

        #region XEditorTableItem
        public string displayName { get { return ToString(); } }
        public int itemId { get { return ToString().GetHashCode(); } }
        public bool itemDisabled { get; set; }
        public bool itemSelected { get; set; }
        public string assetPath { get { return null; } }
        public Texture2D assetIcon { get; set; }
        public List<XIEditorTableItemInfo> children { get; set; }

        public static int totalColumn { get { return 2; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(50, 10, 100, "Index", "");
                case 1: return TianGlyphUtil.GetColumn(600, 50, 1500, "Variant", "");
                default: return TianGlyphUtil.GetColumn(75, 50, 100, "Unknow", "");
            }
        }

        public string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return index.ToString();
                case 1: return ToString();
                default: return "unkown:" + column;
            }
        }
        public object GetColumnOrder(int column)
        {
            switch (column)
            {
                case 0: return index;
                case 1: return ToString();
                default: return index;
            }
        }
    }
    #endregion
}