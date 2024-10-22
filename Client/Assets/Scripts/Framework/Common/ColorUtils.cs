// author:KIPKIPS
// date:2024.10.22 00:57
// describe:
using System.Collections.Generic;
using Framework.Core.Manager.Config;
using UnityEngine;

namespace Framework.Common {
    public enum ColorDef
    {
        LOW_HP = 0,
        HEALTHY_HP = 1,
    }
    public static class ColorUtils {
        
        public static Color Hex2Color(string hexColor) {
            ColorUtility.TryParseHtmlString(hexColor.StartsWith("#") ? hexColor : $"#{hexColor}",out var nowColor);
            return nowColor;
        }
        public static string Color2HexStr(Color color) {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        private static readonly Dictionary<ColorDef, string> ColorDefMap = new()
        {
            {ColorDef.LOW_HP, "LOW_HP"},
            {ColorDef.HEALTHY_HP,"HEALTHY_HP"},
        };
        private static Dictionary<string, Color> _colorMap;
        private static Dictionary<string, Color> ColorMap {
            get {
                if (_colorMap == null) {
                    _colorMap = new Dictionary<string, Color>();
                    var list = ConfigManager.GetConfig(EConfig.ColorDef);
                    foreach (var cf in list) {
                        _colorMap.Add(cf["key"],Hex2Color(cf["hexCode"]));
                    }
                }
                return _colorMap;
            }
        }

        public static Color GetColorByKey(ColorDef key) {
            if (!ColorDefMap.TryGetValue(key, out var colorStr)) return Color.white;
            ColorMap.TryGetValue(colorStr, out var color);
            return color;
        }
    }
}