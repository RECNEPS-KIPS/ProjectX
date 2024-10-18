using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameFramework
{
    public static class CustomUtils
    {
        public static void SetSprite(this Image img,SpriteInfo spriteInfo)
        {
            var atlas = GameResourceLoader.Instance.LoadResource<SpriteAtlas>($"Atlas/{spriteInfo.atlasName}");
            img.sprite = atlas.GetSprite(spriteInfo.spriteName);
        }
        
        public static void SetSprite(this SpriteRenderer sp,SpriteInfo spriteInfo)
        {
            var atlas = GameResourceLoader.Instance.LoadResource<SpriteAtlas>($"Atlas/{spriteInfo.atlasName}");
            sp.sprite = atlas.GetSprite(spriteInfo.spriteName);
        }
        
        public static void Shuffle<T>(IList<T> list)
        {
            System.Random random = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static T Get<T>(this Transform root, string path) where T : Component
        {
            var targetTrans = root.Find(path);
            if (targetTrans != null)
            {
                return targetTrans.GetComponent<T>();
            }
            return null;
        }

        public static GameObject Get(this Transform root, string path)
        {
            var targetTrans = root.Find(path);
            return targetTrans != null ? targetTrans.gameObject : null;
        }

        public static void SetActiveOptimize(this GameObject go,bool isActive)
        {
            if (go.activeSelf != isActive)
            {
                go.SetActive(isActive);
            }
        }
        
        public static void SetActiveOptimize(this Transform trans,bool isActive)
        {
            trans.gameObject.SetActiveOptimize(isActive);
        }
        
        private static Collider[] m_hitOverlapCollider = new Collider[10];
        public static bool OverlapSphereNonAllocIsHit(Vector3 center, float radius,int layerMask)
        {
           return Physics.OverlapSphereNonAlloc(center, radius, m_hitOverlapCollider,layerMask) > 0;
        }
        
        public static Collider[] OverlapSphereNonAllocGetColliders(Vector3 center, float radius,int layerMask)
        {
            int hitNumber = Physics.OverlapSphereNonAlloc(center, radius, m_hitOverlapCollider);
            var hitResult = new Collider[hitNumber];
            Array.Copy(m_hitOverlapCollider, hitResult, hitNumber);
            return hitNumber > 0 ? hitResult : null;
        }

        public static Vector3 SetYZero(this Vector3 source)
        {
            return new Vector3(source.x, 0, source.z);
        }
    }
    
}