using ZZZ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using Cinemachine;

public class CameraHitFeel : Singleton<CameraHitFeel>
{
    //*****依赖CharacterManager和特效管理器VFXManager*****//

    [SerializeField] private Animator currentCharacterAnimator;
    [SerializeField] private Animator currentEnemyAnimator;
    [SerializeField] private float slowMotionResetSpeed;
    [SerializeField] private Dictionary<CharacterNameList,Animator> characterAnimator=new Dictionary<CharacterNameList, Animator>();
    [SerializeField] private Dictionary<Transform, Animator> enemiesAnimator = new Dictionary<Transform, Animator>();
    [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
   // [SerializeField] private Camera_ZoomController zoomController;
    
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        List<Transform> allCharacters = CharacterManager.MainInstance.playerTransform;
        foreach (Transform character in allCharacters) 
        {
          
            if (character.gameObject.TryGetComponent<Player>(out var player))
            {
                if (character.TryGetComponent<Animator>(out var animator))
                {
                    characterAnimator.Add(player.characterName,animator);
                }
            }
        }
        List<GameObject> allenemies = CharacterManager.MainInstance.allEnemies;

        foreach (var enemy in allenemies)
        {
            if (enemy.gameObject.TryGetComponent<Animator>(out var animator))
            {
                enemiesAnimator.Add(enemy.transform, animator);
            }
        }


    }

    Coroutine PauseFrameCoroutine;
    Coroutine SlowMotionCoroutine;

    public void PF(float time)
    {
        if (time == 0) { Debug.Log("顿帧时间为0退出");return; }
        currentEnemyAnimator= GetEnemyAnimator();
        currentCharacterAnimator=GetCurrentCharacterAnimator();

        if (currentCharacterAnimator == null )
        {
            Debug.Log("currentCharacterAnimator is null!");
            return;
        }
        if (currentEnemyAnimator == null)
        {
            Debug.Log("currentEnemyAnimator is null!");
            return;
        }
   

        if (PauseFrameCoroutine != null)
        { 
        StopCoroutine(PauseFrameCoroutine);
        }
        PauseFrameCoroutine = StartCoroutine(PauseFrameOnAnimation(time));
    }
    /// <summary>
    /// 动画特效的慢速
    /// </summary>
    /// <param name="time"></param>
    /// <param name="speedMult"></param>
    public void SlowMotion(float time,float speedMult)
    {
        currentEnemyAnimator = GetEnemyAnimator();
        currentCharacterAnimator = GetCurrentCharacterAnimator();
        if (currentCharacterAnimator == null || currentEnemyAnimator == null)
        {
            Debug.LogWarning("Animator is null!");
            return;
        }
        if (SlowMotionCoroutine != null)
        { StopCoroutine(SlowMotionCoroutine); }
        SlowMotionCoroutine = StartCoroutine(SlowMotionOnAnimation(time, speedMult));
    }
    public void StartSlowTime(float timeScale)
    { 
      
       Time.timeScale=timeScale;
    }
    public void EndSlowTime()
    {
        Time.timeScale = 1;
    }
    IEnumerator SlowMotionOnAnimation(float time,float speedMult)
    {
        float currentSpeed = speedMult;
        currentCharacterAnimator.speed = currentSpeed;
        currentEnemyAnimator.speed = currentSpeed;
        VFXManager.MainInstance.SetVFXSpeed(currentSpeed);
        yield return new WaitForSeconds(time);
        float minValue = 0.001f;
        while (Mathf.Abs(currentSpeed-1)>minValue)
        {
            currentSpeed = Mathf.Lerp(currentSpeed,1,Time.deltaTime* slowMotionResetSpeed);
            currentCharacterAnimator.speed = currentSpeed;
            currentEnemyAnimator.speed = currentSpeed;
            VFXManager.MainInstance.SetVFXSpeed(currentSpeed);

            yield return null;

        }
        currentSpeed = 1;
        currentCharacterAnimator.speed = currentSpeed;
        currentEnemyAnimator.speed = currentSpeed;
        VFXManager.MainInstance.SetVFXSpeed(currentSpeed);

    }
    IEnumerator PauseFrameOnAnimation(float time)
    {
        Debug.LogWarning("进入顿帧协程"+ time);
        currentCharacterAnimator.speed = 0f;
        currentEnemyAnimator.speed = 0f;
        VFXManager.MainInstance.PauseVFX();
        yield return new WaitForSeconds(time);
        VFXManager.MainInstance.ResetVXF();
        currentCharacterAnimator.speed = 1f;
        currentEnemyAnimator.speed = 1f;
    }
    

    private Animator GetEnemyAnimator()
    {
        Transform enemy;
        enemy = GameBlackboard.MainInstance.GetEnemy();
        if (enemy == null) { return null; }   
        if (enemiesAnimator.TryGetValue(enemy, out var A))
        {
            return A;
        }
        return null;
    }
    private Animator GetCurrentCharacterAnimator()
    {
        CharacterNameList characterName = SwitchCharacter.MainInstance.newCharacterName.Value;
        if (characterAnimator.TryGetValue(characterName, out var A))
        {
            return A;
        }
        return null;

    }
    
    #region 震屏
    public void CameraShake(float shakeForce)
    {
        if (shakeForce == 0) { return; }
        cinemachineImpulseSource.GenerateImpulseWithForce(shakeForce);
    }


    #endregion

    #region ZoomIn
    //float currentZoom;
    //public void ZoomIn(float distance)
    //{
    //    currentZoom = zoomController.currentDistance;
    //    zoomController.SetZoom(distance,100);

    //}
    //public void ResetZoom()
    //{
    //    zoomController.SetZoom(currentZoom, 50);
    //}

    #endregion
}
