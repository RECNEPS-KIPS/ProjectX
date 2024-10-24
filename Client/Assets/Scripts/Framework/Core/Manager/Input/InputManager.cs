// author:KIPKIPS
// date:2024.10.25 00:42
// describe:
using System;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Core.Manager.Input {
    [MonoSingletonPath("[Manager]/InputManager")]
    public class InputManager : MonoSingleton<InputManager> {
        public InputControls _inputControls;
        public InputControls InputControls {
            get {
                if (_inputControls == null) {
                    _inputControls = new InputControls();
                }
                return _inputControls;
            }
        }
        public override void Initialize(){
            // inputControls = new InputControls();
        }
        private void OnEnable(){
            InputControls.PC.Enable();
        }
        private void Update(){
            IsJumpKeyPressed();
            IsRunKeyPressed();
            GetAxisInput();
            GetMoveInput();
        }

        public void Launch(){
            
        }

        public bool IsJumpKeyPressed(){
            LogManager.Log("IsJumpKeyPressed");
            return InputControls.PC.Jump.IsPressed();
        }
        
        public Vector2 GetAxisInput(){
            LogManager.Log("GetAxisInput",InputControls.PC.Camera.ReadValue<Vector2>());
            return InputControls.PC.Camera.ReadValue<Vector2>().normalized;
        }
        
        public Vector2 GetMoveInput(){
            LogManager.Log("GetMoveInput");
            return InputControls.PC.Move.ReadValue<Vector2>();
        }

        public bool IsRunKeyPressed(){
            LogManager.Log("IsRunKeyPressed");
            return InputControls.PC.Run.IsPressed();
        }
    }
}