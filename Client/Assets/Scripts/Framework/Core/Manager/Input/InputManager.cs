// author:KIPKIPS
// date:2024.10.25 00:42
// describe:

using System;
using Framework.Core.Manager.Event;
using Framework.Core.Manager.ResourcesLoad;
using Framework.Core.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Core.Manager.Input
{
    [MonoSingletonPath("[Manager]/InputManager")]
    public class InputManager : MonoSingleton<InputManager>
    {
        public InputControls _inputControls;
        public InputControls InputControls => _inputControls ??= new InputControls();

        public override void Initialize()
        {
            // inputControls = new InputControls();
        }

        private void OnEnable()
        {
            InputControls.Player.Enable();
        }

        private void OnDisable()
        {
            InputControls.Player.Disable();
        }

        private void Update()
        {
            if (IsBackpackKeySinglePressed())
            {
                EventManager.Dispatch(EEvent.INPUT_BACKPACK);
            }
        }

        public void Launch()
        {
        }

        public bool IsJumpKeySinglePressed()
        {
            // LogManager.Log("IsJumpKeyPressed");
            return InputControls.Player.Jump.WasPressedThisFrame();
        }

        public Vector2 GetAxisInput()
        {
            // LogManager.Log("GetAxisInput",InputControls.PC.Camera.ReadValue<Vector2>(),InputControls.PC.Camera.ReadValue<Vector2>().normalized,InputControls.PC.Camera.ReadValue<Vector2>().magnitude);
            return InputControls.Player.Camera.ReadValue<Vector2>();
        }

        public Vector2 GetMoveInput()
        {
            // LogManager.Log("GetMoveInput",InputControls.Player.Move.ReadValue<Vector2>());
            return InputControls.Player.Move.ReadValue<Vector2>();
        }

        public bool IsRunKeyPressed()
        {
            // LogManager.Log("IsRunKeyPressed");
            return InputControls.Player.Run.IsPressed();
        }

        public bool IsBackpackKeySinglePressed()
        {
            // LogManager.Log("IsRunKeyPressed");
            return InputControls.Player.Backpack.WasPressedThisFrame();
        }

        public bool IsPickKeySinglePressed()
        {
            // LogManager.Log("IsRunKeyPressed");
            return InputControls.Player.Pick.WasPressedThisFrame();
        }
    }
}