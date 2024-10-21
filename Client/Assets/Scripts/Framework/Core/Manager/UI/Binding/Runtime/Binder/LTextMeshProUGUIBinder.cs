// author:KIPKIPS
// date:2024.10.22 01:28
// describe:
using UnityEngine;

namespace Framework.Core.Manager.UI {

    [BinderComponent(typeof(LTextMeshProUGUI))]
    public class LTextMeshProUGUIBinder : BaseBinder {
        [BinderField(typeof(LTextMeshProUGUI))]
        public enum AttributeType {
            text = 10000 + LinkerType.String,
            fontSize = 20000 + LinkerType.Int32,
            color = 30000 + LinkerType.Color,
            raycastTarget = 40000 + LinkerType.Boolean,
            enabled = 50000 + LinkerType.Boolean
        }

        public override void SetString(Object mono, int linkerType, string value) {
            if (mono == null) return;
            var target = mono as LTextMeshProUGUI;
            if (target == null) {
                return;
            }

            switch ((AttributeType)linkerType) {
                case AttributeType.text:
                    target.text = value;
                    break;
            }
        }
    }
}