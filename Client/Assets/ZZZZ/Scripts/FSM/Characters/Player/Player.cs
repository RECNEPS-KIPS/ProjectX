using UnityEngine;
using static OnAnimationTranslation;

namespace ZZZ  
{
    [RequireComponent(typeof(Animator), typeof(CharacterController))]
    public class Player : CharacterMoveControllerBase
    {
        [SerializeField] public CharacterNameList characterName;

        [SerializeField] public string currentMovementState;

        [SerializeField] public string currentComboState;

        [SerializeField] public Transform enemy;

        [SerializeField] public PlayerSO playerSO;

        [SerializeField] public PlayerCameraUtility playerCameraUtility;

        public PlayerMovementStateMachine movementStateMachine { get;private set; }

        public PlayerComboStateMachine comboStateMachine { get;private set; }
        public new Transform camera { get; private set; }

        private bool canSprintOnSwitch;
        public bool CanSprintOnSwitch
        { 
        get { return canSprintOnSwitch; } 

        set {
                if (value != canSprintOnSwitch)
                { canSprintOnSwitch = value; } 
            }
        }

        protected override void Awake()
        {
            base.Awake();

            camera=Camera.main.transform;
            movementStateMachine =new PlayerMovementStateMachine(this);
            comboStateMachine = new PlayerComboStateMachine(this);
            playerCameraUtility.Init();
        }

       

        protected override void Start()
        {
            base.Start();
            if (characterName == CharacterNameList.AnBi)
            {
                movementStateMachine.ChangeState(movementStateMachine.idlingState); 
            }
            else
            {
                
                movementStateMachine.ChangeState(movementStateMachine.onSwitchOutState);
            }
         
            comboStateMachine.ChangeState(comboStateMachine.NullState);


            Player player= GetComponent<Player>();
            //ע��ڰ���Ϣ
            GameBlackboard.MainInstance.SetGameData<Player>(characterName.ToString(), player);
          
        }
       protected override void Update()
        {
            base.Update();

            if (characterName==CharacterNameList.AnBi)
            {
                movementStateMachine.HandInput();

                movementStateMachine.Update();

                comboStateMachine.Update();
            }
        }
       

        #region ��ض���������˳������ķ���
        public void OnAnimationTranslateEvent(OnEnterAnimationPlayerState playerState)
        {
            switch (playerState)
            {
                case OnEnterAnimationPlayerState.TurnBack:
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.returnRunState);
                    break;
                case OnEnterAnimationPlayerState.Dash:
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.dashingState);
                    comboStateMachine.OnAnimationTranslateEvent(comboStateMachine.NullState);
                    break;
                case OnEnterAnimationPlayerState.Switch:
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.onSwitchState);
                    break;
                case OnEnterAnimationPlayerState.SwitchOut:
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.onSwitchOutState);
                    comboStateMachine.OnAnimationTranslateEvent(comboStateMachine.NullState);
                    break;
                case OnEnterAnimationPlayerState.ATK:
                    comboStateMachine.OnAnimationTranslateEvent(comboStateMachine.ATKIngState);
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.playerMovementNullState);
                    break;
                case OnEnterAnimationPlayerState.DashBack:
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.dashBackingState);
                    comboStateMachine.OnAnimationTranslateEvent(comboStateMachine.NullState);
                    break;
            }
          
        }

        public void OnAnimationExitEvent()
        {
            movementStateMachine.OnAnimationExitEvent();

            comboStateMachine.OnAnimationExitEvent();
        }
        #endregion

        #region ״̬����¼�
        public void OnEnable()
        {
            //ע��movement״̬���е�״̬����¼�
            movementStateMachine.currentState.OnValueChanged += MovementStateChanged;
            comboStateMachine.currentState.OnValueChanged += ComboStateChanged;
            GameBlackboard.MainInstance.enemy.OnValueChanged += EnemyChanged;
        }
       

        public void OnDisable()
        {
            movementStateMachine.currentState.OnValueChanged -= MovementStateChanged;
            comboStateMachine.currentState.OnValueChanged -= ComboStateChanged;
            GameBlackboard.MainInstance.enemy.OnValueChanged -= EnemyChanged;
        }
    

        public void MovementStateChanged(IState currentState)
        {
            currentMovementState= currentState.GetType().Name;
        }
        private void ComboStateChanged(IState state)
        {
           currentComboState= state.GetType().Name;
        }
        private void EnemyChanged(Transform transform)
        {
            enemy = transform;
        }
        #endregion

        #region ���ж����¼�
        public void EnablePreInput()
        {
            comboStateMachine.ATKIngState.EnablePreInput();
        }
        public void CancelAttackColdTime()
        { 
        comboStateMachine.ATKIngState.CancelAttackColdTime();
        }
      
        public void DisableLinkCombo()
        { 
        comboStateMachine.ATKIngState.DisableLinkCombo();
        }
        public void EnableMoveInterrupt()
        {
            comboStateMachine.ATKIngState.EnableMoveInterrupt();
        }
     
        public void ATK()
        {
            comboStateMachine.ATKIngState.ATK();
        }

        #endregion

        #region �����¼���Ч
        //KeLin_Saw
        public void PlayVFX(string name)
        {
            VFX_PoolManager.MainInstance.TryGetVFX(characterName, name);
        }

        public void PlayFootSound()
        {
            //Debug.Log("���ŽŲ���");
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.FOOT,transform.position,Quaternion.identity);
        }
        public void PlayFootBackSound()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.FOOTBACK,transform.position,Quaternion.identity);
        }

        public void PlayWeaponBackSound()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.WeaponBack, characterName.ToString(), transform.position);
        }

        public void PlayWeaponEndSound()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.WeaponEnd, characterName.ToString(), transform.position);
        }

        #endregion
        public void PlayDodgeSound()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.DodgeSound, transform.position, Quaternion.identity);
        }
        public void PlaySwitchWindSound()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.SwitchInWindSound, transform.position, Quaternion.identity);
        }
        public void PlaySwitchInVoice()
        {
            SFX_PoolManager.MainInstance.TryGetSoundPool(SoundStyle.SwitchInVoice, characterName.ToString(), transform.position);
        }    
    }
}
