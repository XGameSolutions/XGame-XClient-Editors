
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;

namespace ShaderTool
{
    internal class ShaderInfo : YamlFileFirstInfo, XIEditorTableItemInfo
    {
        public List<string> remainingKeywords = new List<string>();
        public List<string> allKeywords = new List<string>();
        public List<string> selectedCurrentKeywords = new List<string>();
        public List<int> selectedSecondIndex = new List<int>();

        public ShaderInfo()
        {

        }
        public override bool AddSecond(YamlFileSecondInfo second)
        {
            base.AddSecond(second);
            foreach (var keyword in second.keywords)
            {
                if (!allKeywords.Contains(keyword) && !selectedCurrentKeywords.Contains(keyword))
                {
                    allKeywords.Add(keyword);
                }
            }
            return true;
        }

        public bool AddVariant(ShaderVariantCollection.ShaderVariant variant)
        {
            if (!variants.Contains(variant))
            {
                variants.Add(variant);
                var second = new YamlFileSecondInfo();
                second.passType = (int)variant.passType;
                second.keywords.AddRange(variant.keywords);
                AddSecond(second);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddToAllKeyword(string keyword, bool sort = false)
        {
            if (!allKeywords.Contains(keyword) && !selectedCurrentKeywords.Contains(keyword))
            {
                allKeywords.Add(keyword);
            }
            if (sort)
            {
                allKeywords.Sort();
            }
        }

        public bool IsContainsAllSelectedKeywords(List<string> keywords)
        {
            foreach (var keyword in selectedCurrentKeywords)
            {
                if (!keywords.Contains(keyword)) return false;
            }
            return true;
        }


        #region XEditorTableItem
        public string displayName { get { return shaderName; } }
        public int itemId { get { return uuid.GetHashCode(); } }
        public bool itemDisabled { get; set; }
        public bool itemSelected { get; set; }
        public string assetPath { get { return path; } }
        public Texture2D assetIcon { get; set; }
        public List<XIEditorTableItemInfo> children { get; set; }

        public static int totalColumn { get { return 3; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(30, 30, 30, "Index", "");
                case 1: return TianGlyphUtil.GetColumn(300, 50, 400, "Shader", "");
                case 2: return TianGlyphUtil.GetColumn(75, 50, 75, "Variants", "");
                default: return TianGlyphUtil.GetColumn(75, 50, 100, "Unknow", "");
            }
        }

        public string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return index.ToString();
                case 1: return shaderName;
                case 2: return seconds.Count.ToString();
                default: return "unkown:" + column;
            }
        }
        public object GetColumnOrder(int column)
        {
            switch (column)
            {
                case 0: return index;
                case 1: return shaderName;
                case 2: return seconds.Count;
                default: return shaderName;
            }
        }
    }
    #endregion
}