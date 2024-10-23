using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Common;
using UnityEngine;

namespace Framework.Core.AI
{
    public class AISensorConfig
    {
        public bool isFindTargetDetect; // 目标检测
        public float detectTargetRadius; // 目标检测半径
        public LayerMask findLayerMask;  // 目标FindLayer
        public bool isAttackTargetDetect;// 攻击目标检测
        public float attackTargetRadius; // 攻击目标半径
    }
    
    public class AISensor
    {
        public Transform AITrans;
        public Transform targetTrans;
        
        protected AISensorConfig m_sensorConfig;
        
        public void InitSensor(AISensorConfig sensorConfig)
        {
            m_sensorConfig = sensorConfig;
        }
        
        public void SensorUpdate()
        {
            if (m_sensorConfig.isFindTargetDetect)
            {
                var findColliders = CommonUtils.OverlapSphere_Max10(AITrans.position, m_sensorConfig.detectTargetRadius,
                    m_sensorConfig.findLayerMask);
                var detectTargetCollider = findColliders[0];
                if (detectTargetCollider != null)
                {
                    targetTrans = detectTargetCollider.transform;
                }
            }

            if (m_sensorConfig.isAttackTargetDetect)
            {
                
            }
            
        }
    }
}

