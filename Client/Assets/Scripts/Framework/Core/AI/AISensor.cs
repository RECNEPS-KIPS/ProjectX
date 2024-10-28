using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Common;
using UnityEngine;

namespace Framework.Core.AI
{
    public class AISensor
    {
        public static bool TrySphereDetect(Vector3 pos,float radius,LayerMask layerMask,out Transform target)
        {
            target = null; 
            var findColliders = CommonUtils.OverlapSphere_Max10(pos,radius, layerMask);
            var detectTargetCollider = findColliders[0];
            if (detectTargetCollider != null)
            {
                target = detectTargetCollider.transform;
                return true;
            }
            return false;
        }
    }
}

