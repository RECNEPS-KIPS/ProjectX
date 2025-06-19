using UnityEngine;

namespace ZZZ
{
    [System.Serializable]
    public class PlayerComboData
    {
        [field: SerializeField, Header("comboData")]
        public PlayerComboSOData comboData { get; private set; }

        [field: SerializeField, Header("playerEnemyDetectionData")]
        public PlayerEnemyDetectionData playerEnemyDetectionData { get; private set; }
    }
}