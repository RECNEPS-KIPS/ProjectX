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

    //如果本类只是new一次，那么当我获取这个成员的时候，我实际上是获取到了这个成员的引用，当你希望多次new这个类，但还是修改原来的成员，可以用ref+属性封装字段，从而返回这个类型的引用

}
