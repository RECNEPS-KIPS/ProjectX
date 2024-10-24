using System.Collections;
using System.Collections.Generic;
using Framework.Core.Manager.Input;
using UnityEngine;

namespace GamePlay.Character
{
    //This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {
        // public string horizontalInputAxis = "Horizontal";
        // public string verticalInputAxis = "Vertical";
        // public KeyCode jumpKey = KeyCode.Space;
        // public KeyCode runKey = KeyCode.LeftShift;

        //If this is enabled, Unity's internal input smoothing is bypassed;
        public bool useRawInput = true;

        public override float GetHorizontalMovementInput()
        {
            return InputManager.Instance.GetMoveInput().x;
        }

        public override float GetVerticalMovementInput()
        {
            return InputManager.Instance.GetMoveInput().y;
        }

        public override bool IsJumpKeyPressed()
        {
            return InputManager.Instance.IsJumpKeyPressed();
        }
        public override bool IsRunKeyPressed()
        {
            return InputManager.Instance.IsRunKeyPressed();
        }
    }
}