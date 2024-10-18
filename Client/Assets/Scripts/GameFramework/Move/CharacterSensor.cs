using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class GroundSensor
    {
        private static int GROUND_LAYER_MASK = LayerMask.GetMask("Ground");

        public GroundSensor()
        {
        }

        public bool IsGround(Vector3 center,float detectRadius)
        {
            return CustomUtils.OverlapSphereNonAllocIsHit(center, detectRadius, GROUND_LAYER_MASK);
        }
    }
}
