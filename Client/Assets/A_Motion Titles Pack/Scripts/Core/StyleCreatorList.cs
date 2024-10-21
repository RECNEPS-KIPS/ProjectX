using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Michsky.UI.MTP
{
    [CreateAssetMenu(fileName = "New MTP List", menuName = "Motion Titles Pack/New MTP List", order = 600)]
    public class StyleCreatorList : ScriptableObject
    {
        [Header("Resources")]
        public Texture2D ccEnabled;
        public Texture2D ccDisabled;
        public Texture2D cwEnabled;
        public Texture2D cwDisabled;
        public Texture2D chEnabled;
        public Texture2D chDisabled;

        [Header("Do not change! Dev-only options")]
        public List<StyleItem> styles = new List<StyleItem>();

        [System.Serializable]
        public class StyleItem
        {
            public string styleTitle = "Music Title";
            public Texture2D stylePreview;
            public VideoClip styleVideoPreview;
            public GameObject stylePrefab;
            public bool customContent = false;
            public bool customizableWidth = false;
            public bool customizableHeight = false;
        }
    }
}