// using System;
// using UnityEngine;
//
// namespace GamePlay.Character
// {
//     public class AnimationControl : MonoBehaviour
//     {
//         public MovementController controller;
//  
//         private Animator _animator;
//
//         Animator animator
//         {
//             get
//             {
//                 if (_animator == null)
//                 {
//                     _animator = GetComponent<Animator>();
//                 }
//
//                 return _animator;
//             }
//         }
//
//         private Transform _animatorTransform;
//
//         Transform animatorTransform
//         {
//             get
//             {
//                 if (_animatorTransform != null) return _animatorTransform;
//                 if (animator != null)
//                 {
//                     _animatorTransform = animator.transform;
//                 }
//
//                 return _animatorTransform;
//             }
//         }
//         Transform tr;
//
//         //Whether the character is using the strafing blend tree;
//         public bool useStrafeAnimations = false;
//
//         //Velocity threshold for landing animation;
//         //Animation will only be triggered if downward velocity exceeds this threshold;
//         public float landVelocityThreshold = 5f;
//
//         private float smoothingFactor = 40f;
//         
//         private static readonly int ID_VerticalSpeed = Animator.StringToHash("VerticalSpeed");
//         private static readonly int ID_HorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
//         private static readonly int ID_ForwardSpeed = Animator.StringToHash("ForwardSpeed");
//         private static readonly int ID_StrafeSpeed = Animator.StringToHash("StrafeSpeed");
//         private static readonly int ID_IsGrounded = Animator.StringToHash("IsGrounded");
//         private static readonly int ID_IsStrafing = Animator.StringToHash("IsStrafing");
//         private static readonly int ID_OnLand = Animator.StringToHash("OnLand");
//         private static readonly int ID_IsRun = Animator.StringToHash("IsRun");
//         
//         private bool IsInit;
//
//         private void Awake()
//         {
//             IsInit = false;
//         }
//
//         //初始化
//         public void Init(MovementController ctrl)
//         {
//             controller = ctrl;
//             controller.OnLand += OnLand;
//             controller.OnJump += OnJump;
//             tr = transform;
//             IsInit = true;
//         }
//
//         //OnDisable;
//         private void OnDisable()
//         {
//             //Disconnect events to prevent calls to disabled gameobjects;
//             controller.OnLand -= OnLand;
//             controller.OnJump -= OnJump;
//         }
//
//         private float curVerticalVelocity;
//         private float curHorizontalVelocity;
//         // private float lastVerticalVelocity;
//         // private float lastHorizontalVelocity;
//         
//         Vector3 oldMovementVelocity = Vector3.zero;
//
//
//         //Update;
//         private void Update()
//         {
//             if (!IsInit)
//             {
//                 return;
//             }
//             //Get controller velocity;
//             Vector3 _velocity = controller.GetVelocity();
//
//             //Split up velocity;
//             var up = tr.up;
//             Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, up);
//             Vector3 _verticalVelocity = _velocity - _horizontalVelocity;
//
//             //Smooth horizontal velocity for fluid animation;
//             _horizontalVelocity = Vector3.Lerp(oldMovementVelocity, _horizontalVelocity, smoothingFactor * Time.deltaTime);
//             oldMovementVelocity = _horizontalVelocity;
//             curVerticalVelocity = _verticalVelocity.magnitude * VectorMath.GetDotProduct(_verticalVelocity.normalized, up);
//             curHorizontalVelocity = _horizontalVelocity.magnitude;
//             
//             // LogManager.Log("VerticalVelocity",curVerticalVelocity,_verticalVelocity);
//             // LogManager.Log("HorizontalVelocity",curHorizontalVelocity,_horizontalVelocity);
//             
//             animator.SetFloat(ID_VerticalSpeed, curVerticalVelocity);
//             animator.SetFloat(ID_HorizontalSpeed, curHorizontalVelocity);
//
//             //If animator is strafing, split up horizontal velocity;
//             if (useStrafeAnimations)
//             {
//                 Vector3 _localVelocity = animatorTransform.InverseTransformVector(_horizontalVelocity);
//                 animator.SetFloat(ID_ForwardSpeed, _localVelocity.z);
//                 animator.SetFloat(ID_StrafeSpeed, _localVelocity.x);
//             }
//
//             //Pass values to animator;
//             animator.SetBool(ID_IsGrounded, controller.IsGrounded());
//             animator.SetBool(ID_IsStrafing, useStrafeAnimations);
//             // lastHorizontalVelocity = curHorizontalVelocity;
//             // lastVerticalVelocity = curVerticalVelocity;
//         }
//
//         private void OnLand(Vector3 _v)
//         {
//             //Only trigger animation if downward velocity exceeds threshold;
//             if (VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
//             {
//                 return;
//             }
//             LogManager.Log("OnLand");
//             animator.SetTrigger(ID_OnLand);
//         }
//
//         private void OnJump(Vector3 _v)
//         {
//             LogManager.Log("OnJump");
//         }
//     }
// }