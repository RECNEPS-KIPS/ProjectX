// author:KIPKIPS
// date:2024.10.25 00:42
// describe:
using System;
using Framework.Core.Manager.Event;
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
            InputControls.Keyboard.Enable();
        }
        private void OnDisable(){
            InputControls.Keyboard.Disable();
        }
        private void Update(){
            // IsJumpKeyPressed();
            // IsRunKeyPressed();
            // GetAxisInput();
            // GetMoveInput();
            if (InputControls.Keyboard.Backpack.IsPressed())
            {
                EventManager.Dispatch(EEvent.PLAYER_ATTR_UPDATE);
            }
        }

        public void Launch(){
            
        }

        public bool IsJumpKeySinglePressed(){
            // LogManager.Log("IsJumpKeyPressed");
            return InputControls.Keyboard.Jump.WasPressedThisFrame();
        }
        
        public Vector2 GetAxisInput(){
            // LogManager.Log("GetAxisInput",InputControls.PC.Camera.ReadValue<Vector2>(),InputControls.PC.Camera.ReadValue<Vector2>().normalized,InputControls.PC.Camera.ReadValue<Vector2>().magnitude);
            return InputControls.Keyboard.Camera.ReadValue<Vector2>();
        }
        
        public Vector2 GetMoveInput(){
            // LogManager.Log("GetMoveInput",InputControls.PC.Move.ReadValue<Vector2>());
            return InputControls.Keyboard.Move.ReadValue<Vector2>();
        }

        public bool IsRunKeyPressed(){
            // LogManager.Log("IsRunKeyPressed");
            return InputControls.Keyboard.Run.IsPressed();
        }
        
        public bool IsBackpackKeySinglePressed(){
            // LogManager.Log("IsRunKeyPressed");
            return InputControls.Keyboard.Backpack.WasPressedThisFrame();
        }
        public bool IsPickKeySinglePressed(){
            // LogManager.Log("IsRunKeyPressed");
            return InputControls.Keyboard.Pick.WasPressedThisFrame();
        }
    }
}