using Assets._02_Scripts.Player.State;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _rotateSpeed = 30f;
    public float MoveSpeed => _moveSpeed;

    [Header("Gravity Stats")]
    [SerializeField] private float _gravity = -9.81f; // �߷� ���ӵ�
    private float _verticalVelocity; // ���� ���� �ӵ�
    public float VerticalVelocity
    { get
        {
            return _verticalVelocity;
        }
        set
        { _verticalVelocity = value; }
    } // ���� �ӵ��� ������ �� �ֵ��� ������Ƽ
    public float Gravity => _gravity;

    [Header("Jump Stats")]
    public bool IsJumpTriggered { get; private set; } //���� Ʈ���� (������Ʈ ��)
    [SerializeField] private float _jumpHeight = 1.5f;
    public float JumpHeight => _jumpHeight;

    [Header("Roll Stats")]
    public bool IsRollTriggered { get; private set; }

    [Header("Parry Stats")]
    public bool IsParryTriggered { get; private set; }

    [Header("PickUp")]
    public bool IsPickUpTriggered { get; private set; }

    [Header("Inventory")]
    InputAction _inventoryOpenAction;
    InputAction _inventoryCloseAction;

    [Header("Interact")]
    InputAction _interactAction;

    [Header("Status")]
    InputAction _statusOpenAction;
    InputAction _statusCloseAction;

    [Header("Quest")]
    InputAction _questLogOpenAction;
    InputAction _questLogCloseAction;

    [Header("Ref")]
    [SerializeField] PlayerInteractor _playerInteractor;
    [SerializeField] CameraEffectController _cameraEffectController;

    [Header("Footstep Settings")]
    [SerializeField] private float _walkFootstepThreshold = 0.3f;
    [SerializeField] private float _runFootstepThreshold = 0.15f;

    private float _lastFootstepTime; // 마지막 재생 시간 기록
    private int _lastFootstepFrame = -1;

    [Header("Footstep Sounds")]
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _footstep_Default;
    [SerializeField] private AudioClip _footstep_Grass;
    [SerializeField] private AudioClip _footstep_Dungeon;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _sfxMixerGroupName = "SFX";
    #region ������Ƽ
    public PlayerStatus Stats { get; private set; }
    [SerializeField] private CurrentUsingItems _currentItems;
    public CurrentUsingItems CurrentItems => _currentItems;
    public Vector3 CurrentMovement { get; set; } // �̹� �����ӿ� �̵��� ���� ����
    public bool CanRotate { get; set; } = true;  // ���� ���¿��� ȸ���� �������� ����
    public Vector2 LookInput { get; private set; }

    public Transform MainCamTransform { get; private set; }
    #endregion
    #region ������Ʈ ������
    // ������Ʈ ������
    public Animator Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    #endregion
    #region FSM
    // FSM
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerDefenceState DefenceState { get; private set; }
    public PlayerRollState RollState { get; private set; }
    public PlayerWeaponSwitchState WeaponSwitchState { get; private set; }
    public PlayerParryState ParryState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    // ... ������, ��� ��� ���� �߰�
    #endregion
    #region �Է� ������
    // �Է� ������
    public Vector2 MoveInput { get; private set; }
    public bool IsAttackTriggered { get; private set; }
    public bool IsRun { get; private set; } // �޸��� ����
    public Transform LeftFoot { get; private set; }
    public Transform RightFoot { get; private set; }
    public bool IsBlocking { get; private set; } // ��� ����

    public bool IsInputLocked { get; set; }

    //public Vector2 SmoothedInput { get; private set; }
    //private Vector2 _smoothInputVelocity;
    #endregion

    private void Awake()
    {
        // init
        _playerInteractor = GetComponent<PlayerInteractor>();
        Stats = GetComponent<PlayerStatus>();
        Animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        PlayerInput = GetComponent<PlayerInput>();
        _audioSource = GetComponent<AudioSource>();
        BindFootstepAudioSourceToSFXMixer();
        if (Camera.main != null)
        {
            MainCamTransform = Camera.main.transform;
        }
        #region FSM 

        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine);
        WalkState = new PlayerWalkState(this, StateMachine);
        AttackState = new PlayerAttackState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        RollState = new PlayerRollState(this, StateMachine);
        DefenceState = new PlayerDefenceState(this, StateMachine);
        ParryState = new PlayerParryState(this, StateMachine);
        HitState = new PlayerHitState(this, StateMachine);
        WeaponSwitchState = new PlayerWeaponSwitchState(this, StateMachine);
        #endregion

        if (PlayerInput != null)
        {
            _inventoryOpenAction = PlayerInput.actions["InventoryOpen"];
            _inventoryCloseAction = PlayerInput.actions["InventoryClose"];
            _interactAction = PlayerInput.actions["Interact"];
            _statusOpenAction = PlayerInput.actions["StatusOpen"];
            _statusCloseAction = PlayerInput.actions["StatusClose"];
            _questLogOpenAction = PlayerInput.actions["QuestLogOpen"];
            _questLogCloseAction = PlayerInput.actions["QuestLogClose"];

            _inventoryOpenAction.performed += HandleOnInventoryOpen;
            _inventoryCloseAction.performed += HandleOnInventoryClose;
            _interactAction.performed += HandleOnTryInteract;
            _statusOpenAction.performed += HandleOnStatusOpen;
            _statusCloseAction.performed += HandleOnStatusClose;
            _questLogOpenAction.performed += HandleOnQuestLogOpen;
            _questLogCloseAction.performed += HandleOnQuestLogClose;

            PlayerInput.SwitchCurrentActionMap("Player");
        }
        LeftFoot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        RightFoot = Animator.GetBoneTransform(HumanBodyBones.RightFoot);

        UIEvents.RaiseCursorHide();
    }
    private void Start()
    {
        // ���� ���� ����
        if (_currentItems == null)
        {
            _currentItems = FindFirstObjectByType<CurrentUsingItems>();
        }

        StateMachine.Initialize(IdleState);
        
        if (DungeonSceneManager.Instance != null)
        {
            DungeonSceneManager.Instance.DSMRegisterPlayer(this);
        }

    }
    private void OnEnable()
    {
        if (PlayerInput != null && PlayerInput.currentActionMap != null)
        {
            PlayerInput.currentActionMap.Enable();
        }

        UIEvents.UIOpened += HandleUIOpened;
        UIEvents.UIClosed += HandleUIClosed;
    }
    private void OnDisable()
    {
        if (PlayerInput != null && PlayerInput.currentActionMap != null)
        {
            PlayerInput.currentActionMap.Disable();
        }
        UIEvents.UIOpened -= HandleUIOpened;
        UIEvents.UIClosed -= HandleUIClosed;


        if (PlayerInput != null)
        {
            _inventoryOpenAction.performed  -= HandleOnInventoryOpen;
            _inventoryCloseAction.performed -= HandleOnInventoryClose;
            _interactAction.performed       -= HandleOnTryInteract;
            _statusOpenAction.performed     -= HandleOnStatusOpen;
            _statusCloseAction.performed    -= HandleOnStatusClose;
            _questLogOpenAction.performed   -= HandleOnQuestLogOpen;
            _questLogCloseAction.performed  -= HandleOnQuestLogClose;
        }
    }
    private void Update()
    {
        //UI Open State
        if (IsInputLocked)
        {
            CurrentMovement = Vector3.zero;
            SetSprintCameraEffect(false);
            UpdateAnimator();
            return;
        }

        if (Stats.IsDead)
        {
            SetSprintCameraEffect(false);
            ApplyRotation();
            return;
        }


        ReadInput();

        UpdateSprintCameraEffect();

        ApplyGravity();

        CurrentMovement = Vector3.zero;

        // ���� ������ ������ �����
        StateMachine.CurrentState.LogicUpdate();

        ApplyRotation();
        ApplyMovement();
        TryPickUp();
        UpdateAnimator();
        //HandleFootsteps();
    }
 
    private void ReadInput()
    {
        MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
        LookInput = PlayerInput.actions["Look"].ReadValue<Vector2>();
        IsAttackTriggered = PlayerInput.actions["Attack"].triggered;
        IsJumpTriggered = PlayerInput.actions["Jump"].triggered;
        IsRun = PlayerInput.actions["Sprint"].IsPressed();
        IsRollTriggered = PlayerInput.actions["Roll"].triggered;
        IsBlocking = PlayerInput.actions["Block"].IsPressed();
        IsParryTriggered = PlayerInput.actions["Parry"].triggered;
        IsPickUpTriggered = PlayerInput.actions["PickUp"].triggered;
        //SmoothedInput = Vector2.SmoothDamp(SmoothedInput, MoveInput, ref _smoothInputVelocity, 0.1f);
        //// 0�̳� 1�� ��� Ƣ�� �ʰ�, �� 0.1�ʿ� ���� ��ǥ �Է°�(MoveInput)�� ���� �ε巴�� ���� ��ȭ
        //// ref _smoothInputVelocity�� ���� �������� �ӵ��� ��� �ε巴�� �������� ����

        //if (SmoothedInput.sqrMagnitude < 0.0001f) //���� �ӵ� �ʱ�ȭ (�Ķ���� �� �ʼ��� �ƴ�)
        //{
        //    SmoothedInput = Vector2.zero;
        //    _smoothInputVelocity = Vector2.zero; // �ӵ��� �Բ� �ʱ�ȭ
        //}
    }
    private void ApplyGravity()
    {
        //�߷�
        if (CharacterController.isGrounded)
        //ĳ���� ��Ʈ�ѷ� ���� ��ü���� �׶��� üĿ
        {
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }
    //  ���� �̵��� �߷��� ���� �̵� ����
    private void ApplyMovement()
    {
        Vector3 finalVelocity = CurrentMovement;
        finalVelocity.y = _verticalVelocity; // �߷� �ջ�

        CharacterController.Move(finalVelocity * Time.deltaTime);
    }
    //  ȸ�� ����
    private void ApplyRotation()
    {
        // ���°� ȸ���� ����߰�, ȸ�� �Է°��� ���� ���� ȸ��
        if (CanRotate && MoveInput.sqrMagnitude > 0.01f)
        {
            Vector3 dir = CameraForward;

            if (dir != Vector3.zero)
            {
                // ��ǥ ȸ���� ��� (ī�޶� ���� ��)
                Quaternion targetRotation = Quaternion.LookRotation(dir);

                // �ε巴�� ĳ���� ȸ��
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
            }
        }
    }
    private void TryPickUp()
    {
        if (IsPickUpTriggered) _playerInteractor.TryPickUp();
    }
    //  �ִϸ����� �Ķ���� ������Ʈ
    private void UpdateAnimator()
    {
        float speed = 1f;
        if (!IsBlocking && IsRun && Stats.CurrentStamina > 0)
            speed = 2f;

        float targetY = MoveInput.y * speed;
        float targetX = MoveInput.x * speed;

        Animator.SetFloat("Forward", targetY, 0.1f, Time.deltaTime);
        Animator.SetFloat("Side", targetX, 0.1f, Time.deltaTime);
    }
    private void SwitchActionMap(string mapName)
    {
        PlayerInput.ActivateInput();
        PlayerInput.SwitchCurrentActionMap(mapName);
    }
    private void UpdateSprintCameraEffect()
    {
        if (_cameraEffectController == null)
            return;

        bool canSprintEffect =
            IsRun &&
            MoveInput.sqrMagnitude > 0.01f &&
            Stats.CurrentStamina > 0 &&
            !IsBlocking &&
            CharacterController.isGrounded;

        _cameraEffectController.SetSprintEffect(canSprintEffect);
    }
    private void SetSprintCameraEffect(bool isSprinting)
    {
        if (_cameraEffectController == null)
            return;

        _cameraEffectController.SetSprintEffect(isSprinting);
    }
    private void BindFootstepAudioSourceToSFXMixer()
    {
        if (_audioSource == null)
        {
            Debug.LogWarning("Player AudioSource is NULL");
            return;
        }

        if (_audioMixer == null)
        {
            _audioMixer = Resources.Load<AudioMixer>("MasterMixer");
        }

        if (_audioMixer == null)
        {
            Debug.LogWarning("MasterMixer를 찾을 수 없습니다.");
            return;
        }

        AudioMixerGroup[] groups = _audioMixer.FindMatchingGroups(_sfxMixerGroupName);

        if (groups == null || groups.Length == 0)
        {
            Debug.LogWarning($"{_sfxMixerGroupName} Mixer Group을 찾을 수 없습니다.");
            return;
        }

        _audioSource.outputAudioMixerGroup = groups[0];
    }
    /*private void HandleFootsteps()
    {
        if (!CharacterController.isGrounded)
            return;

        if (MoveInput.sqrMagnitude < 0.01f)
            return;

        if (CharacterController.velocity.sqrMagnitude < 0.1f)
            return;

        float interval = IsRun ? _runStepInterval : _walkStepInterval;

        _footstepTimer += Time.deltaTime;

        if (_footstepTimer >= interval)
        {
            _footstepTimer = 0f;

            string footstepSound = "Footstep_Default";

            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
                Vector3.down,
                out hit,
                0.5f))
            {
                switch (hit.collider.tag)
                {
                    case "Grass":
                        footstepSound = "Footstep_Grass";
                        break;

                    case "Dungeon":
                        footstepSound = "Footstep_Dungeon";
                        break;
                }
            }

            SoundEvents.RaiseSFXPlayedEvent(footstepSound);
        }
    }*/
    #region Event
    public void HandleUIOpened()
    {
        SwitchActionMap("UI");
    }
    public void HandleUIClosed()
    {
        SwitchActionMap("Player");
    }
    public void HandleOnInventoryOpen(InputAction.CallbackContext callback)
    {
        UIEvents.RaiseInventoryOpened();
        UIEvents.RaiseCursorShowed();
        SwitchActionMap("UI");
    }
    public void HandleOnInventoryClose(InputAction.CallbackContext callback)
    {
        UIEvents.RaiseInventoryClosed();
        UIEvents.RaiseCursorHide();
        SwitchActionMap("Player");
    }
    public void HandleOnTryInteract(InputAction.CallbackContext callback)
    {
        if (_playerInteractor.TryInteract())
        {
            UIEvents.RaiseCursorShowed();
            SwitchActionMap("UI");
        }
    }
    public void HandleOnStatusOpen(InputAction.CallbackContext context)
    {
        UIEvents.RaiseStatusOpened();
        UIEvents.RaiseCursorShowed();
        SwitchActionMap("UI");
    }
    public void HandleOnStatusClose(InputAction.CallbackContext context)
    {
        UIEvents.RaiseStatusClosed();
        UIEvents.RaiseCursorHide();
        SwitchActionMap("Player");
    }

    public void HandleOnQuestLogOpen(InputAction.CallbackContext context)
    {
        QuestEvents.RaiseQuestLogOpened();
        UIEvents.RaiseCursorShowed();
        SwitchActionMap("UI");
    }
    public void HandleOnQuestLogClose(InputAction.CallbackContext context)
    {
        QuestEvents.RaiseQuestLogClosed();
        UIEvents.RaiseCursorHide();
        SwitchActionMap("Player");
    }
    #endregion
    #region Animation_Event
    // �ִϸ��̼� �̺�Ʈ ��Ŀ: �޺� ���� ����
    public void Anim_OpenCombo()
    {
        if (StateMachine.CurrentState == AttackState)
        {
            AttackState.OpenCombo();
            
        }
    }

    // �ִϸ��̼� �̺�Ʈ ��Ŀ: �޺� �Ǵ� �� ���� ����
    public void Anim_CloseCombo()
    {
        if (StateMachine.CurrentState == AttackState)
        {
            AttackState.CloseCombo();
            
        }
    }

    public void Anim_WeaponSwap()
    {
        // 스왑 상태가 중간에 캔슬되지 않고 정상적으로 유지되고 있을 때만 실제 스왑 처리
        if (StateMachine.CurrentState == WeaponSwitchState)
        {
            WeaponSwitchState.PerformSwap();
        }
    }

    public void Anim_WeaponSwapEnd()
    {
        if (StateMachine.CurrentState == WeaponSwitchState)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    public void Anim_Footstep(AnimationEvent evt)
    {
        if (!CharacterController.isGrounded)
            return;

        if (_lastFootstepFrame == Time.frameCount)
            return;

        // 현재 재생 중인 애니메이션 클립 이름
        string clipName = evt.animatorClipInfo.clip.name.ToLower();

        float forwardAbs = Mathf.Abs(MoveInput.y);
        float sideAbs = Mathf.Abs(MoveInput.x);

        bool isDiagonal = forwardAbs > 0.1f && sideAbs > 0.1f;

        // 애니메이션 방향 판별
        bool isForwardBack =
            clipName.Contains("front") ||
            clipName.Contains("forward") ||
            clipName.Contains("fwd") ||
            clipName.Contains("fwdexploration") ||
            clipName.Contains("back") ||
            clipName.Contains("backward") ||
            clipName.Contains("bwd");

        bool isSide =
            clipName.Contains("left") ||
            clipName.Contains("right");


        if (isDiagonal)
        {
            // 전후 입력이 더 크면 전후 애니메이션만 허용
            if (forwardAbs >= sideAbs)
            {
                if (!isForwardBack)
                    return;
            }
            // 좌우 입력이 더 크면 좌우 애니메이션만 허용
            else
            {
                if (!isSide)
                    return;
            }
        }

        // 블렌드 weight 체크
        if (evt.animatorClipInfo.weight < 0.4f)
            return;

        float currentThreshold = IsRun ? _runFootstepThreshold : _walkFootstepThreshold;

        if (Time.time < _lastFootstepTime + currentThreshold)
            return;

        if (!Physics.Raycast(
                transform.position + Vector3.up * 0.1f,
                Vector3.down,
                out RaycastHit hit,
                1f))
            return;

        AudioClip clip = _footstep_Default;

        if (hit.collider.CompareTag("Grass"))
            clip = _footstep_Grass;
        else if (hit.collider.CompareTag("Dungeon"))
            clip = _footstep_Dungeon;

        _lastFootstepTime = Time.time;
        _lastFootstepFrame = Time.frameCount;

        _audioSource.pitch = Random.Range(0.95f, 1.05f);

        _audioSource.PlayOneShot(clip);

    }

    // �ִϸ��̼� �̺�Ʈ ��Ŀ : ���ڱ� �Ҹ�
    /*public void Anim_Footstep()
    {
        
        //string stepSound = IsRun ? "Footstep_Run" : "Footstep_Walk";
        //SoundEvents.RaiseSFXPlayedEvent(stepSound);
    }*/
    #endregion

    #region ī�޶� ���� ���� �޼���

    // ȸ����
    public Vector3 CameraForward
    {
        get
        {
            if (MainCamTransform == null) return transform.forward;
            Vector3 forward = MainCamTransform.forward;
            forward.y = 0f;
            return forward.normalized;
        }
    }

    // �Է°�(WASD)�� ������ ī�޶� ������ �̵� ���͸� ����ؼ� ��ȯ �̵���
    public Vector3 GetCameraDirection(Vector2 input)
    {
        if (MainCamTransform == null) return Vector3.zero;

        Vector3 right = MainCamTransform.right;
        right.y = 0f;
        right.Normalize();

        return (CameraForward * input.y + right * input.x).normalized;
    }

    #endregion
}