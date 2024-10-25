using System;
using Framework.Core.Manager.ResourcesLoad;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.Item
{
    public class Item:MonoBehaviour
    {
        public int ID;
        
        public ItemConfig ItemConfig {
            get
            {
                var cf = ItemManager.Instance.GetItemConfigByID(ID);
                if (cf != null)
                {
                    return cf;
                }
                LogManager.LogError("Item",$"ID:{ID} has not config");
                return null;
            }
        }

        private void Awake()
        {
            InitItem();
        }

        public void InitItem()
        {
            if (ItemConfig != null)
            {
                var go = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(ItemConfig.Path),this.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.Euler(0, 0, 0);
                if (ItemConfig.HasCollider)
                {
                    var collider = go.transform.AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                }
            }
        }
    }
}