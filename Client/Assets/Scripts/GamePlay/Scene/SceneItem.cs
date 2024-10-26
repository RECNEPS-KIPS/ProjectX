// author:KIPKIPS
// date:2024.10.25 21:07
// describe:
using System;
using Framework.Core.Manager.ResourcesLoad;
using GamePlay.Item;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.Scene
{
    [Serializable]
    public class SceneItem : MonoBehaviour, IItemable, IOctrable
    {
        public int id;
        public int ID
        {
            get => id;
            set => id = value;
        }
        public ItemConfig ItemConfig
        {
            get
            {
                var cf = ItemManager.Instance.GetItemConfigByID(ID);
                if (cf != null)
                {
                    return cf;
                }
                LogManager.LogError("Item", $"ID:{ID} has not config");
                return null;
            }
        }
        public Collider _collider;
        public Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = ColliderTrs.GetComponent<Collider>();
                }
                return _collider;
            }
        }
        public Transform SelfTrs => transform;
        public Transform ColliderTrs { get; set; }
        private void Awake()
        {
            InitItem();
        }
        public void InitItem()
        {
            if (ItemConfig == null) return;
            var go = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(ItemConfig.Path), transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(0, 0, 0);
            var collider = go.transform.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            ColliderTrs = go.transform;
        }
    }
}