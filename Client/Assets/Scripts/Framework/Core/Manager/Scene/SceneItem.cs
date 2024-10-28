// author:KIPKIPS
// date:2024.10.25 21:07
// describe:
using System;
using Framework.Core.SpaceSegment;
using GamePlay.Item;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Core.Manager.Scene
{
    [Serializable]
    public class SceneItem : IScenable
    {
        public int itemID;
        public int GUID;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public GameObject model;
        public ItemConfig ItemConfig
        {
            get
            {
                var cf = ItemManager.Instance.GetItemConfigByID(itemID);
                if (cf != null)
                {
                    return cf;
                }
                LogManager.LogError("Item", $"ItemID:{itemID} has not config");
                return null;
            }
        }

        public bool LoadModel()
        {
            if (ItemConfig == null) return false;
            // model = Object.Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(ItemConfig.Path),SceneManager.Instance.SceneItemRoot);
            model.transform.localPosition = position;
            model.transform.eulerAngles = rotation;
            model.transform.localScale = scale;
            return true;
        }
        public Bounds Bounds { get; set; }
        public bool OnShow(Transform parent)
        {
            if (model != null) return true;
            bool loaded = LoadModel();
            return loaded;
        }
        public void OnHide()
        {
            Object.Destroy(model);
            model = null;
        }
    }
}