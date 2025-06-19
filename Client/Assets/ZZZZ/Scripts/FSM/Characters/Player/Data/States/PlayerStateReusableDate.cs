using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateReusableDate
{
    public float inputMult { get; set; }

    public bool shouldWalk { get; set; }

    public bool canDash { get; set; } = true;

    public float poseThreshold { get; set; }

    public Vector2 inputDirection { get; set; }

    public float rotationTime { get; set; }

    public float targetAngle { get; set; }
}