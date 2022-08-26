using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class Player : MonoBehaviour
{
    public StarterAssetsInputs _input { get; private set; }
        
    public GameManager manager;
    public GameObject addictEffect, detoxEffect, confusionEffect, confusionChargeEffect;

    public int currentHp { get; private set; }
    public int maxHp { get; private set; }
    public int currentLife { get; private set; }
    public int maxLife { get; private set; }
    public int confusionStack { get; private set; }
    public int confusionStackMax { get; private set; }
    public int poisonStack { get; private set; }
    public int magicGiantStack { get; private set; }

    public bool[] isActiveBeads { get; private set; }
    public bool isActiveMinimap { get; private set; }
    public bool isActiveMagicFairy { get; private set; }
    public bool isActiveMagicHuman { get; private set; }
    public bool isPoison { get; private set; }
    public bool isConfusion { get; private set; }
    public bool isInteract { get; private set; }
    public bool wasInteractPreprocess { get; private set; }
    public bool isInteractPreprocessReady { get; private set; }

    private ThirdPersonController _controller;
    private CharacterController controller;
    private NPCInteractionZone interactNpc;

    private Vector3 respawnPoint, interactPoint;

    private int poisonStackMax, poisonTicDamage;
    private float detoxActivateTime, confusionActivateTime, confusionDuration, countTimeDetox, countTimeConfusion;

    #region ##### 유니티 내장 함수 #####

    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _controller = GetComponent<ThirdPersonController>();

        InitializeController();

        InitializePlayer(
            hpMax: out int _maxHp,
            lifeMax: out int _maxLife,
            poisonStackMax: out int _poisonStackMax,
            confusionStackMax: out int _confusionStackMax,
            poisonTicDamage: out int _poisonTicDamage,
            detoxActivateTime: out float _detoxActivateTime,
            confusionStackUpdateTime: out float _confusionActivateTime,
            confusionDuration: out float _confusionDuration
        );

        this.maxHp = _maxHp;
        this.maxLife = _maxLife;
        this.poisonStackMax = _poisonStackMax;
        this.confusionStackMax = _confusionStackMax;
        this.poisonTicDamage = _poisonTicDamage;
        this.detoxActivateTime = _detoxActivateTime;
        this.confusionActivateTime = _confusionActivateTime;
        this.confusionDuration = _confusionDuration;

        this.countTimeDetox = 0f;
        this.countTimeConfusion = 0f;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        manager.SetPlayerIngameAttributes(
            isActiveBeads: out bool[]  _isActieBeads,
            isActiveMinimap: out bool _isActiveMinimap,
            isActiveMagicFairy: out bool _isActiveMagicFairy,
            isActiveMagicHuman: out bool _isActiveMagicHuman,
            life: out int _currentLife,
            currentHp: out int _currentHp,
            currentConfusion: out int _confusionStack,
            magicGiantStack: out int _magicGiantStack,
            poisonStack: out int _poisonStack,
            savedPositionEnabled: out bool _savedPositionEnabled,
            savedPositionX: out int _savedPositionX,
            savedPositionY: out int _savedPositionY,
            savedPositionZ: out int _savedPositionZ
        );

        this.isActiveBeads = _isActieBeads;
        this.isActiveMinimap = _isActiveMinimap;
        this.isActiveMagicFairy = _isActiveMagicFairy;
        this.isActiveMagicHuman = _isActiveMagicHuman;
        this.currentLife = _currentLife;
        this.currentHp = _currentHp;
        this.confusionStack = _confusionStack;
        this.magicGiantStack = _magicGiantStack;
        this.poisonStack = _poisonStack;

        if (_savedPositionEnabled)
        {
            ForceToMove(new Vector3(_savedPositionX, _savedPositionY, _savedPositionZ));
        }

        UpdateBeadVisibility();
    }

    private void Update()
    {
        //OnDamage();
        Detoxify();
        Interact();
        Escape();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject gameObject = other.gameObject;

        switch (other.tag)
        {
            case NameManager.TAG_ITEM:
                GetItem(gameObject: gameObject);
                break;

            case NameManager.TAG_PLAYER_RESPAWN:
                respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * 10);    // 리스폰 지점 업데이트
                break;

            case NameManager.TAG_FALL:
                OnDamage(value: maxHp, isAvoidable: false);
                break;

            case NameManager.TAG_MONSTER_ATTACK:
                OnDamageByMonsterAttack(obj: gameObject);
                break;

            case NameManager.TAG_MONSTER_TURN_BACK_AREA:
                other.GetComponentInParent<MonsterRange>().TurnBack();
                break;

            case NameManager.TAG_TRAP_ACTIVATOR:
                OnTriggerEnterToTrapActivator(trapActivator: other.GetComponentInParent<TrapActivator>());
                break;

            case NameManager.TAG_TRAP_DEACTIVATOR:
                other.GetComponentInParent<TrapDeactivator>().CallDeactivateTrap(player: this);
                break;

            case NameManager.TAG_TRAP:
                OnTriggerEnterToTrap(trap: other.GetComponentInParent<Trap>());
                break;

            case NameManager.TAG_GUIDE_ACTIVATOR:
                OnTriggerEnterToGuideActivator(guideActivator: other.GetComponent<GuideActivator>());
                break;

            case NameManager.TAG_PORTAL:
                Portal portal = other.GetComponentInParent<Portal>();
                manager.LoadSceneBySceneType(sceneType: portal.nextSceneType);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case NameManager.TAG_NEAGTIVE_EFFECT:
                OnTriggerStayInNegativeEffectZone(negativeEffect: other.GetComponent<NegativeEffectZone>());
                break;

            case NameManager.TAG_NPC_INTERACTION_ZONE:
                OnTriggerStayInNpcInteractionZone(npc: other.GetComponent<NPCInteractionZone>());
                break;

            case NameManager.TAG_TRAP:
                OnTriggerStayInTrap(trapGameObject: other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case NameManager.TAG_NPC_INTERACTION_ZONE:
                OnTriggerExitFromNpcInteractionZone();
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject gameObject = collision.gameObject;
        switch (gameObject.tag)
        {
            case NameManager.TAG_MONSTER_ATTACK:
                OnDamageByMonsterAttack(obj: gameObject);
                break;
        }
    }

    #endregion

    #region ##### 실질 기능 함수 #####
    public void ActivateMagicFairy(bool isActive)
    {
        this.isActiveMagicFairy = isActive;
        InitializeController();
    }

    public void ActivateMagicGiant(bool isActive)
    {
        if(isActive && magicGiantStack < ValueManager.PLAYER_MAGIC_GIANT_STACK_MAX)
        {
            magicGiantStack++;
        }
    }

    public void ActivateMagicHuman(bool isActive)
    {
        this.isActiveMagicHuman = isActive;
    }

    public void Interact()
    {
        InteractPreprocess();

        if (wasInteractPreprocess &&
            isInteract &&
            _input.interact)
        {
            // 상호작용 및 다음 스크립트 존재 여부에 따라 상호작용 상태 업데이트.
            manager.UpdateInteractionUI(isContinuable: out bool _isInteract);
            this.isInteract = _isInteract;

            // 키 입력 초기화.
            _input.interact = false;
        }

        InteractPostProcess();
    }

    public void Escape()
    {
        if (_input.escape)
        {
            if (isInteract)
            {
                isInteract = false;
                _input.interact = false;
            }
            else
            {
                manager.DisplayGameMenu();
            }
        }
    }

    public void CallUpdatePlayerIngameAttributes()
    {
        Vector3 position = this.transform.position;

        manager.UpdatePlayerIngameAttributes(
            isActiveBeads: this.isActiveBeads,
            isActiveMinimap: this.isActiveMinimap,
            isActiveMagicFairy: this.isActiveMagicFairy,
            isActiveMagicHuman: this.isActiveMagicHuman,
            life: this.currentLife,
            currentHp: this.currentHp,
            currentConfusion: this.confusionStack,
            magicGiantStack: this.magicGiantStack,
            poisonStack: this.poisonStack,
            attributeSavedPositionX: (int)position.x,
            attributeSavedPositionY: (int)position.y,
            attributeSavedPositionZ: (int)position.z
        );
    }

    private void InitializeController()
    {
        if(_controller != null)
        {
            _controller.MoveSpeed = ValueManager.PLAYER_MOVE_SPEED_DEFAULT;
            _controller.SprintSpeed = ValueManager.PLAYER_SPRINT_SPEED_DEFAULT;

            if (this.isActiveMagicFairy)
            {
                _controller.MoveSpeed *= ValueManager.PLAYER_MAGIC_SPEED_RATIO;
                _controller.SprintSpeed *= ValueManager.PLAYER_MAGIC_SPEED_RATIO;
            }
        }
    }

    private void InitializePlayer(
        out int hpMax,
        out int lifeMax,
        out int poisonStackMax,
        out int confusionStackMax,
        out int poisonTicDamage,
        out float detoxActivateTime,
        out float confusionStackUpdateTime,
        out float confusionDuration
    )
    {
        hpMax = ValueManager.PLAYER_HP_MAX;
        lifeMax = ValueManager.PLAYER_LIFE_MAX;
        poisonStackMax = ValueManager.PLAYER_POISON_STACK_MAX;
        confusionStackMax = ValueManager.PLAYER_CONFUSION_STACK_MAX;
        poisonTicDamage = ValueManager.PLAYER_POISON_TIC_DAMAGE;
        detoxActivateTime = ValueManager.PLAYER_DETOX_ACTIVATE_TIME;
        confusionStackUpdateTime = ValueManager.PLAYER_CONFUSION_STACK_UPDATE_TIME;
        confusionDuration = ValueManager.PLAYER_CONFUSION_DURATION;
    }

    private void UpdateBeadVisibility()
    {
        manager.UpdateUIActivedBeads(isActives: isActiveBeads);
    }

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.BackMirror:
                break;
            case ItemType.Bead:
                // 구슬 관리를 플레이어가 아닌 외부에서 하게 될 경우 수정.
                GetItemBead(index: item.value);
                break;
            case ItemType.Heal:
                // hp, life 둘다 0 인 경우 GameOver 추가 필요.
                ChangeCurrentHp(value: item.value, isDamage: false);
                break;
            case ItemType.Life:
                GetItemLife(value: item.value);
                break;
            case ItemType.Map:
                this.isActiveMinimap = true;
                manager.ActivateMinimap(isActive: this.isActiveMinimap);
                break;
        }

        gameObject.SetActive(false);
    }

    private void GetItemBead(int index)
    {
        if(index >= 0 && index < this.isActiveBeads.Length)
        {
            this.isActiveBeads[index] = true;
        }

        manager.UpdateUIActivedBeads(isActives: this.isActiveBeads);
        manager.ActivateSkyboxByPlayerBeads(isActiveBeads: this.isActiveBeads);
    }

    private void GetItemLife(int value)
    {
        if(this.currentLife >= this.maxLife)
        {
            this.currentLife = this.maxLife;
        }
        else
        {
            this.currentLife += value;
        }
    }

    private void OnDamageByMonsterAttack(GameObject obj)
    {
        KnockBack();    // 내용 추가 필요.

        // monster 별 업데이트 할 것.
        Monster monster = obj.gameObject.GetComponentInParent<Monster>();

        if (monster != null)
        {
            switch (monster.type)
            {
                case MonsterType.Insect:
                    OnDamageByInsect(damage: monster.damage);
                    break;
                case MonsterType.Zombie:
                    break;
                case MonsterType.Range:
                    break;
                case MonsterType.Catapult:
                    break;
                default:
                    break;
            }
        }
        else
        {
            MonsterAttackRock rock = obj.gameObject.GetComponent<MonsterAttackRock>();

            if (rock != null)
            {
                OnDamage(value: rock.damage, isAvoidable: true);
                rock.ExplosionDestroy();
            }
            else
            {
                MonsterMissile missile = obj.gameObject.GetComponent<MonsterMissile>();

                if (missile != null)
                {
                    OnDamage(value: missile.damage, isAvoidable: true);
                    missile.ExplosionDestroy();
                }
            }

        }

        StartCoroutine(routine: Addict(summaryDamage: poisonTicDamage * poisonStack));
    }

    private void KnockBack()
    {

    }

    private void OnDamageByInsect(int damage)
    {
        OnDamage(value: damage, isAvoidable: true); // 기본공격 데미지

        isPoison = true;        // 중독 상태 활성화
        poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;  // 중독 스택 변경
        ChangePoisonEffect(isAddict: isPoison); // 중독 이펙트 활성화

        countTimeDetox = 0;       // 해독 타이머 초기화
    }

    private IEnumerator Addict(int summaryDamage)
    {
        if (isPoison)
        {
            OnDamage(value: summaryDamage, isAvoidable: false);
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(routine: Addict(summaryDamage: summaryDamage));
    }

    private void Detoxify()
    {
        if (isPoison)
        {
            UpdateCountTimeDetox();

            if(countTimeDetox > detoxActivateTime)
            {
                isPoison = false;
                ChangePoisonEffect(isAddict: isPoison);
            }
        }
    }

    private void OnTriggerStayInNegativeEffectZone(NegativeEffectZone negativeEffect)
    {
        if (negativeEffect.type == NegativeEffectType.Confusion && !isConfusion)
        {
            Timer(tick: Time.deltaTime, time: ref countTimeConfusion);
            if (countTimeConfusion >= confusionActivateTime)
            {
                StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                confusionStack += negativeEffect.value;
                countTimeConfusion = 0f;

                if (confusionStack >= confusionStackMax)
                {
                    StartCoroutine(routine: Confuse());
                }
            }
        }
    }

    private void OnTriggerStayInNpcInteractionZone(NPCInteractionZone npc)
    {
        interactNpc = npc;
        isInteractPreprocessReady = true;
        interactPoint = npc.transform.position;
    }

    private void OnTriggerStayInTrap(GameObject trapGameObject)
    {
        Trap trap = trapGameObject.GetComponent<Trap>();

        if(trap != null)
        {
            switch (trap.type)
            {
                case TrapType.TrafficLight:
                    trap.GetComponent<TrapTrafficLight>().Jaywalk(player: this);
                    break;
            }
        }
    }

    private void OnTriggerExitFromNpcInteractionZone()
    {
        isInteractPreprocessReady = false;
        wasInteractPreprocess = false;
        isInteract = false;
    }

    private void OnTriggerEnterToTrapActivator(TrapActivator trapActivator)
    {
        trapActivator.ActivateTrap(player: this);

        if(trapActivator.traps.Length > 0)
        {
            TrapType trapType = trapActivator.traps[0].GetComponent<Trap>().type;

            manager.DisplayGuideByGuideType(guideType: ConvertManager.ConvertTrapTypeToGuideType(trapType: trapType));
        }
    }

    private void OnTriggerEnterToTrap(Trap trap)
    {
        switch (trap.type)
        {
            case TrapType.MachPair:
                trap.GetComponent<TrapMachPairColumn>().DetectPlayer();
                break;
        }
    }

    private void OnTriggerEnterToGuideActivator(GuideActivator guideActivator)
    {
        manager.DisplayGuideByGuideType(guideType: guideActivator.type);
    }

    private void UpdateCountTimeDetox()
    {
        // 점프, 이동 입력 없을 경우 정지 상태로 판단
        if (!_input.jump &&
            _input.move == Vector2.zero)
        {
            Timer(tick: Time.deltaTime, time: ref countTimeDetox);
        }
        else
        {
            countTimeDetox = 0;
        }
    }

    private void ChangePoisonEffect(bool isAddict)
    {
        addictEffect.SetActive(isAddict);
        detoxEffect.SetActive(!isAddict);

        if (isAddict)
        {
            addictEffect.GetComponent<ParticleSystem>().maxParticles = poisonStack;
        }
        else
        {
            detoxEffect.GetComponent<ParticleSystem>().maxParticles = poisonStack;
        }
    }

    private void InteractPreprocess()
    {
        // 상호작용 전 한 번만 실행하기 위한 조건 세팅
        // 1. 상호작용 상태가 아니다.
        // 2. 상호작용 전처리를 하지 않은 상태이다.
        // 3. 상호작용 전처리 준비 상태이다.
        // 4. 상호작용 버튼 입력이 들어왔다.
        if (!isInteract &&
            !wasInteractPreprocess &&
            isInteractPreprocessReady && 
            _input.interact)
        {
            // 플레이어 위치 NPC 앞으로 이동 및 정지
            // NPC 바라보도록 추후 보완.
            ForceToMove(point: interactPoint);
            StopPlayerMotion();
            this.transform.LookAt(target: interactNpc.transform);

            // 카메라 위치 조정
            manager.MoveGameObject(gameObject: manager.npcInteractionCamera, vector: interactNpc.cameraPoint.position);
            manager.npcInteractionCamera.transform.rotation = interactNpc.cameraPoint.rotation;

            // 카메라 및 UI 업데이트.
            manager.UpdateUINormalToInteraction(npc: interactNpc);

            // 상호작용 준비 상태 업데이트.
            wasInteractPreprocess = true;
            isInteract = true;
        }
    }

    private void InteractPostProcess()
    {
        // 상호작용 후 한 번만 실행하기 위한 조건 세팅
        // 1. 상호작용 상태가 아니다.
        // 2. 상호작용 전처리를 한 상태이다.
        // 3. 상호작용 버튼 입력이 들어왔다.
        if (!isInteract &&
            wasInteractPreprocess &&
            _input.interact)
        {
            // UI 및 NPC 업데이트.
            manager.UpdateUIWhetherInteraction(isInteract: false);
            interactNpc.DisappearByVolatility(isVolatility: interactNpc.isVolatility);

            // 상호작용 다시 할 수 있게 하기 위한 변수 초기화
            isInteractPreprocessReady = false;
            wasInteractPreprocess = false;

            // 상호작용 키 입력 초기화
            _input.interact = false;
        }
    }

    IEnumerator Confuse()
    {
        isConfusion = true;
        _input.isReverse = true;

        // 플레이어 이펙트 활성화 추가
        confusionEffect.SetActive(true);
        yield return new WaitForSeconds(confusionDuration);

        //플레이어 이펙트 비활성화 추가
        confusionEffect.SetActive(false);
        _input.isReverse = false;
        isConfusion = false;

        confusionStack = 0;
    }
    #endregion

    #region ##### 다용도 함수 #####

    public void ForceToMove(Vector3 point)
    {
        // CharacterController 가 활성화되어 있으면 Transform.position 값을 변경해도 적용되지 않는다.
        controller.enabled = false;
        this.transform.position = point;
        controller.enabled = true;
    }

    public bool IsMoving()
    {
        if(_input.jump || _input.move != Vector2.zero)
        {
            return true;
        }

        return false;
    }

    public void OnDamage(int value, bool isAvoidable)
    {
        // isAvoidable 마법 효과로 피할 수 있는 공격인지에 대한 bool 값.

        if (isAvoidable &&
            magicGiantStack > 0)
        {
            magicGiantStack--;
        }
        else
        {
            ChangeCurrentHp(value: value, isDamage: true);
        }
    }

    public void ChangeCurrentHp(int value, bool isDamage)
    {
        if (isDamage)
        {
            currentHp -= value;

            if (currentHp <= 0)
            {
                currentHp = 0;

                if (currentLife > 0)
                {
                    Respawn();
                }
                else
                {
                    manager.DisplayGameOver();
                }
            }
        }
        else
        {
            currentHp += value;

            if (currentHp > maxHp)
            {
                currentHp = maxHp;
            }
        }        
    }

    public void StopPlayerMotion()
    {
        _input.MoveInput(newMoveDirection: Vector2.zero);
        _input.JumpInput(newJumpState: false);
        _input.SprintInput(newSprintState: false);
    }

    private void Respawn()
    {
        ForceToMove(point: respawnPoint);

        currentHp = maxHp;
        currentLife--;
    }

    private void Timer(float tick, ref float time)
    {
        time += tick;
    }

    IEnumerator ActiveVolatileEffect(GameObject effect, float duration)
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    #endregion
}
