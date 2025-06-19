using System.Runtime.Remoting.Messaging;
using UnityEngine;
using HuHu;
using UnityEngine.InputSystem;

public class CharacterInputSystem : Singleton<CharacterInputSystem>
{
    public CharacterInput inputActions;

    protected override void Awake()
    {
        base.Awake();
        if (inputActions == null)
            inputActions = new CharacterInput();
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    public Vector2 PlayerMove
    {
        get  {
            var m = inputActions.Player.Movement.ReadValue<Vector2>();
            return m;
        }
    }

    public Vector2 CameraLook
    {
        get => inputActions.Player.CameraLook.ReadValue<Vector2>();
    }

    public bool Run
    {
        get => inputActions.Player.Run.triggered;
    }

    public bool Run_Continue
    {
        get => inputActions.Player.Run.phase == InputActionPhase.Performed;
    }

    public bool Jump
    {
        get => inputActions.Player.Jump.triggered;
    }

    public bool Crouch
    {
        get => inputActions.Player.Crouch.phase == InputActionPhase.Performed;
    }

    public bool L_Atk
    {
        get => inputActions.Player.L_AtK.triggered;
    }

    public bool R_Atk
    {
        get => inputActions.Player.R_Atk.triggered;
    }

    public bool Aim
    {
        get => inputActions.Player.R_Atk.phase == InputActionPhase.Performed;
    }

    public bool L_Atk_Continue
    {
        get => inputActions.Player.Continue_Atk.phase == InputActionPhase.Performed;
    }

    public bool Execute
    {
        get => inputActions.Player.Execute.triggered;
    }

    // public bool SwitchCharacter
    // { 
    //  get => inputActions.Player.SwitchCharacter.triggered;
    // }
    public bool Skill
    {
        get => inputActions.Player.Skill.triggered;
    }

    public bool FinishSkill
    {
        get => inputActions.Player.FinishSkill.triggered;
    }

    public bool Walk
    {
        get => inputActions.Player.Walk.triggered;
    }
}