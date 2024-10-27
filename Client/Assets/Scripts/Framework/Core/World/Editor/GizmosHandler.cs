// author:KIPKIPS
// date:2024.10.27 11:46
// describe:
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.World
{
    public class GizmosHandler : MonoBehaviour
    {
        [NonSerialized] public bool drawColliderBoxesGizmos;
        [NonSerialized] public Color colliderBoxesGizmosColor = new (1, 0.4f, 0.7f, 1);
        [NonSerialized] public List<GameObject> colliderList = new ();
        private void OnDrawGizmos()
        {
            //负责碰撞盒的边框绘制
            if (!drawColliderBoxesGizmos || colliderList == null) return;
            if (colliderList.Count <= 0) return;
            foreach (var t in colliderList)
            {
                if (!t) continue;
                BoxCollider collider = t.GetComponent<BoxCollider>();
                Gizmos.color = colliderBoxesGizmosColor;
                Gizmos.DrawWireCube(collider.transform.localPosition, collider.size);
            }
        }
    }
}