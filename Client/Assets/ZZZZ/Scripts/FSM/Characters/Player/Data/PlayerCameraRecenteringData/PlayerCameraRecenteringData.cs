using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PlayerCameraRecenteringData 
{
    [field: SerializeField, Range(0, 360)] public float minAngleRange { get; private set; }

    [field: SerializeField, Range(0, 360)] public float maxAngleRange { get; private set; }

    [field: SerializeField, Range(-1, 10)] public float waitingTime { get; private set; }

    [field: SerializeField, Range(-1, 10)] public float recenteringTime { get; private set; }

    public bool IsWithInAngle(float angle)
    {
        return angle > minAngleRange && angle < maxAngleRange;
    }
}
