using Cinemachine.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool;

public class EnemyMoveController : CharacterMoveControllerBase
{
    [Range(0.1f, 80), SerializeField] private float moveMult;
    [SerializeField] private bool moveCommand;
    [SerializeField] private Transform nearestPlayer;
    public void SetMoveCommand(bool Command)
    {
        moveCommand= Command;
    }
    protected override void Awake()
    {
        base.Awake();
        CharacterManager.MainInstance.AddEnemy(this.gameObject);
        
    }
    protected override void Update()
    {
        base.Update();
        FindNearestPlayer();
        LookAtPlayer(nearestPlayer);
    }
    protected override void OnAnimatorMove()
    { 
        base.OnAnimatorMove();
        UpdateAIMove(characterAnimator.deltaPosition);
    }

    private void UpdateAIMove(Vector3 dir )   
    {
        Vector3 resetDir=ResetVelocityOnSlop(dir);
        characterController.Move(resetDir*Time.deltaTime*moveMult);
    }
    public void AIMovementFace(float HorizontalValue,float VerticalValue)
    {
        if (moveCommand == true)
        {
            characterAnimator.SetBool(AnimatorID.HasInputID, true);
            characterAnimator.SetFloat(AnimatorID.LockID, 1);
            characterAnimator.SetFloat(AnimatorID.HorizontalID, HorizontalValue,0.2f,Time.deltaTime);
            characterAnimator.SetFloat(AnimatorID.VerticalID, VerticalValue, 0.2f, Time.deltaTime);
        }
        else
        {
            characterAnimator.SetFloat(AnimatorID.LockID, 0);
            characterAnimator.SetFloat(AnimatorID.HorizontalID, 0, 0.2f, Time.deltaTime);
            characterAnimator.SetFloat(AnimatorID.VerticalID, 0,0.2f, Time.deltaTime);
        }
      

    }
    private void FindNearestPlayer()
    {
        float minDistance = 3;

        foreach (var players in CharacterManager.MainInstance.playerTransform)
        {
            if (players == null){ return; }
            
            float mathfDistance = Vector3.Distance(transform.position, players.position);

            if (mathfDistance < minDistance)
            {
                minDistance = mathfDistance;

                nearestPlayer = players;
            }
        }

    }

    private void LookAtPlayer(Transform Player)
    {
        if (Player == null) { return; }
        transform.Look(Player.position, 50);
    }
}
