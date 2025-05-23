using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZZZ
{
    [System.Serializable]
    public class PlayerEnemyDetectionData 
    {
        [field: SerializeField, Header("µ–»ÀºÏ≤‚")] public float detectionRadius { get; private set; }
        [field: SerializeField] public float detectionLength { get; private set; }

        [field: SerializeField] public LayerMask WhatIsEnemy { get; private set; }

       
    }
}
