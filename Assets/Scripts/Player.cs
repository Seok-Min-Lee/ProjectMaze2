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
    public AudioSource itemSound, trapSound, damageSound;

    public int currentHp { get; private set; }
    public int maxHp { get; private set; }
    public int currentLife { get; private set; }
    public int maxLife { get; private set; }
    public int currentKey { get; private set; }
    public int maxKey { get; private set; }
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
    public bool isInteract { get; private set; }        // 상호작용 현재 진행 여부
    public bool isInteractable { get; private set; }    // 상호작용 가능 여부
    public bool isTutorial { get; private set; }
    public InteractionType interactableInteractionType { get; private set; }
    public GuideType interactableGuideType { get; private set; }
    public NpcObject interactableNpcObject { get; private set; }

    private ThirdPersonController _controller;
    private CharacterController controller;

    private Vector3 respawnPoint, interactPoint;

    private int poisonStackMax, poisonTicDamage;
    private float detoxActivateTime, confusionActivateTime, confusionDuration, countTimeDetox, countTimeConfusion;

    #region ##### 유니티 내장 함수 #####

    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _controller = GetComponent<ThirdPersonController>();

        InitPlayerPreconditions(
            hpMax: out int _maxHp,
            lifeMax: out int _maxLife,
            keyMax: out int _keyMax,
            poisonStackMax: out int _poisonStackMax,
            confusionStackMax: out int _confusionStackMax,
            poisonTicDamage: out int _poisonTicDamage,
            detoxActivateTime: out float _detoxActivateTime,
            confusionStackUpdateTime: out float _confusionActivateTime,
            confusionDuration: out float _confusionDuration
        );

        this.maxHp = _maxHp;
        this.maxLife = _maxLife;
        this.maxKey = _keyMax;
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
        
        InitPlayerContidions();
        InitControllerSettings();
    }

    private void Update()
    {
        Detoxify();
        Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        switch (other.tag)
        {
            case NameManager.TAG_ITEM:
                OnTriggerEnterToItem(obj: obj);
                break;

            case NameManager.TAG_PLAYER_RESPAWN:
                OnTriggerEnterToPlayerRespawnPoint(transform: other.GetComponent<Transform>());
                break;

            case NameManager.TAG_FALL:
                OnDamage(value: maxHp, isAvoidable: false);
                break;

            case NameManager.TAG_MONSTER_ATTACK:
                OnDamageByMonsterAttack(obj: obj);
                break;

            case NameManager.TAG_MONSTER_TURN_BACK_AREA:
                other.GetComponentInParent<MonsterTurret>().TurnBack();
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

            case NameManager.TAG_PORTAL:
                OnTriggerEnterToPortal(portal: other.GetComponentInParent<Portal>());
                break;

            case NameManager.TAG_INTERACTION_ZONE:
                OnTriggerEnterToInteractionZone(zone: other.GetComponent<InteractionZone>());
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case NameManager.TAG_NEGATIVE_EFFECT:
                OnTriggerStayInNegativeEffectZone(negativeEffect: other.GetComponent<NegativeEffectZone>());
                break;

            case NameManager.TAG_TRAP:
                OnTriggerStayInTrap(obj: other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case NameManager.TAG_INTERACTION_ZONE:
                OnTriggerExitFromInteractionZone();
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

    private void OnTriggerEnterToItem(GameObject obj)
    {
        Item item = obj.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Bead:
                GetItemBead(index: item.value);
                break;
            case ItemType.Heal:
                ChangeCurrentHp(value: item.value, isDamage: false);
                break;
            case ItemType.Life:
                GetItemLife(value: item.value);
                break;
            case ItemType.Map:
                GetItemMinimap();
                break;
            case ItemType.Key:
                GetItemKey(value: item.value);
                break;
            default:
                break;
        }

        // 효과음 재생
        itemSound.Play();

        // 시스템 메세지 출력
        string messageText = ValueManager.MESSAGE_PREFIX_ITEM + item.name + ValueManager.MESSAGE_SUFFIX_ITEM;
        manager.DisplayConfirmMessage(text: messageText, type: EventMessageType.Item);

        // 아이템 오브젝트 비활성화
        obj.SetActive(false);
    }

    private void OnTriggerEnterToPlayerRespawnPoint(Transform transform)
    {
        // 리스폰 위치 저장.
        this.respawnPoint = transform.position + (Vector3.up * ValueManager.PLAYER_CALIBRATION_RESPAWN_Y);
    }

    private void OnTriggerEnterToInteractionZone(InteractionZone zone)
    {
        // 상호작용 관련 변수 세팅
        this.isInteractable = true;
        this.interactableInteractionType = zone.interactionType;

        switch (zone.interactionType)
        {
            case InteractionType.Diaglogue:
                this.interactableNpcObject = zone.GetComponent<NpcInteractionZone>().npcObject;
                this.interactPoint = zone.transform.position;
                break;
            case InteractionType.Guide:
                this.interactableGuideType = zone.GetComponent<GuideInteractionZone>().guideType;
                break;
            default:
                break;
        }

        // 즉시 발생해야 하는 경우 입력이 들어온 것으로 처리
        if (zone.isImmediate)
        {
            _input.interact = true;
            this.isInteract = true;
        }
    }

    private void OnTriggerEnterToTrapActivator(TrapActivator trapActivator)
    {
        trapActivator.ActivateTrap(player: this);

        trapSound.Play();
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

    private void OnTriggerEnterToPortal(Portal portal)
    {
        // 필요한 구슬을 가지고 있지 않은 경우 에러 처리.
        if (TryGetNecessaryBeadIndexByNextSceneType(portal.nextSceneType, index: out int index) &&
            !this.isActiveBeads[index])
        {
            manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PORTAL_FAIL, type: EventMessageType.Error);
        }
        else
        {
            manager.LoadSceneBySceneType(sceneType: portal.nextSceneType);
        }
    }

    private void OnTriggerStayInNegativeEffectZone(NegativeEffectZone negativeEffect)
    {
        switch (negativeEffect.type)
        {
            case NegativeEffectType.Confusion:
                OnTriggerStayInNegativeEffectZoneTypeConfusion(value: negativeEffect.value);
                break;
            case NegativeEffectType.Poison:
                OnTriggerStayInNegativeEffectZoneTypePoison(value: negativeEffect.value);
                break;
        }
    }

    private void OnTriggerStayInTrap(GameObject obj)
    {
        if(TryGetTrapByGameObject(obj: obj, trap: out Trap trap))
        {
            switch (trap.type)
            {
                case TrapType.TrafficLight:
                    trap.GetComponent<TrapTrafficLight>().Jaywalk(player: this);
                    break;
            }
        }
    }

    public void OnTriggerExitFromInteractionZone()
    {
        // 상호작용 가능 영역을 벗어 났을 때 관련 변수 초기화
        this.isInteractable = false;
        this.interactableInteractionType = InteractionType.None;
        this.interactableGuideType = GuideType.None;
        this.interactableNpcObject = null;
    }

    private void OnTriggerStayInNegativeEffectZoneTypeConfusion(int value)
    {
        if (!this.isConfusion)
        {
            Timer(tick: Time.deltaTime, time: ref this.countTimeConfusion);

            if (this.countTimeConfusion >= this.confusionActivateTime)
            {
                // 공포 게이지 증가 및 이펙트 활성화
                StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                this.confusionStack += value;
                this.countTimeConfusion = 0f;

                // 공포 게이지가 최대에 도달하면 공포 효과 활성화
                if (this.confusionStack >= this.confusionStackMax)
                {
                    this.confusionStack = this.confusionStackMax;
                    StartCoroutine(routine: Confuse());
                }
            }
        }
    }

    private void OnTriggerStayInNegativeEffectZoneTypePoison(int value)
    {
        if (!this.isPoison)
        {
            this.isPoison = true;
            this.poisonStack += value;
            StartCoroutine(Addict(stack: this.poisonStack, ticDamage: this.poisonTicDamage));
        }
    }

    #endregion

    #region ##### 실질 기능 함수 #####
    public void ActivateMagicFairy(bool isActive)
    {
        this.isActiveMagicFairy = isActive;
        InitControllerSettings();
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

    public void CallUpdatePlayerIngameAttributes()
    {
        Vector3 position = this.transform.position;

        manager.UpdatePlayerIngameAttributes(
            isActiveBeads: this.isActiveBeads,
            isActiveMinimap: this.isActiveMinimap,
            isActiveMagicFairy: this.isActiveMagicFairy,
            isActiveMagicHuman: this.isActiveMagicHuman,
            life: this.currentLife,
            key: this.currentKey,
            currentHp: this.currentHp,
            currentConfusion: this.confusionStack,
            magicGiantStack: this.magicGiantStack,
            poisonStack: this.poisonStack,
            attributeSavedPositionX: (int)position.x,
            attributeSavedPositionY: (int)position.y,
            attributeSavedPositionZ: (int)position.z
        );
    }

    public void Interact()
    {
        // 상호작용 가능한 상태일 때 입력이 들어오는 지 확인
        // 상호작용 처리는 GameManager 에서 처리.
        if (this.isInteractable && _input.interact)
        {
            this.isInteract = true;
        }
        else
        {
            this.isInteract = false;
        }
    }

    private void GetItemBead(int index)
    {
        if(index >= 0 && index < this.isActiveBeads.Length)
        {
            this.isActiveBeads[index] = true;
        }

        manager.UpdateByPlayerActiveBeads(isActives: this.isActiveBeads);
    }

    private void GetItemMinimap()
    {
        this.isActiveMinimap = true;
        manager.ActivateMinimap(isActive: this.isActiveMinimap);
    }

    private void GetItemLife(int value)
    {
        this.currentLife = GetValueIncreaseToNotExceedMaxValue(
            startValue: this.currentLife,
            maxValue: this.maxLife,
            increaseValue: value
        );
    }

    private void GetItemKey(int value)
    {
        this.currentKey = GetValueIncreaseToNotExceedMaxValue(
            startValue: this.currentKey,
            maxValue: this.maxKey,
            increaseValue: value
        );
    }

    private void OnDamageByMonsterAttack(GameObject obj)
    {
        // MonsterAttack의 경우 아래 순서대로 확인하여 처리한다.
        // 1. 몬스터의 직접적인 공격인지
        // 2. 캐터펄트가 발사한 투사체인지
        // 3. 터렛이 발사한 투사체인지
        if (TryGetMontserByGameObject(obj: obj, monster: out Monster monster))
        {
            switch (monster.type)
            {
                case MonsterType.Insect:
                    OnDamageByInsect(insect: monster.GetComponent<MonsterInsect>(), damage: monster.damage);
                    break;
                case MonsterType.Zombie:
                    break;
                case MonsterType.Turret:
                    break;
                case MonsterType.Catapult:
                    break;
                case MonsterType.Ghost:
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (TryGetMonsterAttackRockByGameObject(obj: obj, rock: out MonsterAttackRock rock))
            {
                // 데미지 부여 후 폭발 및 소멸
                OnDamage(value: rock.damage, isAvoidable: true);
                rock.ExplosionDestroy();
            }
            else
            {
                if (TryGetMonsterMissileByGameObject(obj: obj, missile: out MonsterMissile missile))
                {
                    // 데미지 부여 후 폭발 및 소멸
                    OnDamage(value: missile.damage, isAvoidable: true);
                    missile.ExplosionDestroy();
                }
            }

        }

        this.damageSound.Play();
        StartCoroutine(routine: Addict(stack: this.poisonStack, ticDamage: this.poisonTicDamage));
    }

    private void OnDamageByInsect(MonsterInsect insect, int damage)
    {
        if (insect.isAttack)
        {
            // 기본공격 데미지 부여
            OnDamage(value: damage, isAvoidable: true); 

            // 중독 활성화
            this.isPoison = true;
            this.poisonStack = GetValueIncreaseToNotExceedMaxValue(
                startValue: this.poisonStack, 
                maxValue: this.poisonStackMax, 
                increaseValue: 1
            );

            // 중독 이펙트 활성화
            ChangePoisonEffect(isAddict: isPoison);
            
            // 해독 타이머 초기화
            countTimeDetox = 0;       
        }
    }

    private IEnumerator Addict(int stack, int ticDamage)
    {
        if (this.isPoison)
        {
            // 시스템 메세지 출력
            manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_ADDICT, type: EventMessageType.Debuff);

            // 중독 도트 데미지 부여
            OnDamage(value: stack * ticDamage, isAvoidable: false);
            
            yield return new WaitForSeconds(ValueManager.PLAYER_POISON_TIC_DELAY);
            
            StartCoroutine(routine: Addict(stack: stack, ticDamage: ticDamage));
        }
    }

    private void Detoxify()
    {
        if (this.isPoison)
        {
            // 정지 시간 체크
            UpdateCountTimeDetox();

            if(this.countTimeDetox > this.detoxActivateTime)
            {
                // 중독 상태 비활성화
                this.isPoison = false;
                ChangePoisonEffect(isAddict: isPoison);

                // 시스템 메세지 출력
                manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_DETOX, type: EventMessageType.Recovery);
            }
        }
    }

    private bool TryGetNecessaryBeadIndexByNextSceneType(SceneType type, out int index)
    {
        // 다음 씬으로 넘어가기 위한 구슬의 인덱스를 찾는 것이기 때문에 하나씩 앞당긴다고 생각하면 된다.
        switch (type)
        {
            case SceneType.Stage2:
                index = 0;
                break;
            case SceneType.Stage3:
                index = 1;
                break;
            case SceneType.Village:
                index = 2;
                break;
            default:
                index = -1;
                break;
        }

        return index > -1;
    }

    private void UpdateCountTimeDetox()
    {
        // 점프, 이동 입력 없을 경우 정지 상태로 판단
        if (IsInputtingPlayerMovement())
        {
            countTimeDetox = 0;
        }
        else
        {
            Timer(tick: Time.deltaTime, time: ref countTimeDetox);
        }
    }

    private void ChangePoisonEffect(bool isAddict)
    {
        addictEffect.SetActive(isAddict);
        detoxEffect.SetActive(!isAddict);

        if (isAddict)
        {
            addictEffect.GetComponent<ParticleSystem>().maxParticles = this.poisonStack;
        }
        else
        {
            detoxEffect.GetComponent<ParticleSystem>().maxParticles = this.poisonStack;
        }
    }

    IEnumerator Confuse()
    {
        isConfusion = true;
        manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_CONFUSE, type: EventMessageType.Debuff);

        ReversePlayerMove();

        // 플레이어 이펙트 활성화 추가
        confusionEffect.SetActive(true);

        yield return new WaitForSeconds(confusionDuration);

        // 플레이어 이펙트 비활성화 추가
        confusionEffect.SetActive(false);

        ReversePlayerMove();

        isConfusion = false;
        manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_CALM_DOWN, type: EventMessageType.Recovery);

        confusionStack = 0;
    }

    private void ReversePlayerMove()
    {
        _input.isReverse = !_input.isReverse;
        _input.MoveInput(_input.move * (-1));
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

    public void InputStop()
    {
        _input.JumpInput(newJumpState: false);
        _input.SprintInput(newSprintState: false);
        _input.MoveInput(newMoveDirection: Vector2.zero);
        _input.LookInput(newLookDirection: Vector2.zero);
    }

    public bool IsInputtingPlayerMovement()
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
            this.currentHp = GetValueDecreaseToNotExceedMinValue(
                startValue: this.currentHp,
                minValue: 0,
                decreaseValue: value
            );

            if (this.currentHp <= 0 &&
                this.currentLife > 0)
            {
                Resurrect();
            }
        }
        else
        {
            this.currentHp = GetValueIncreaseToNotExceedMaxValue(
                startValue: this.currentHp,
                maxValue: this.maxHp,
                increaseValue: value
            );
        }        
    }

    private void Resurrect()
    {
        StopAllCoroutines();

        this.currentHp = this.maxHp;
        this.currentLife--;
        if (isTutorial && this.currentLife < 0)
        {
            this.currentLife = 0;
        }

        this.isPoison = false;
        this.isConfusion = false;
        this.poisonStack = 0;
        this.confusionStack = 0;

        this.addictEffect.SetActive(false);
        this.detoxEffect.SetActive(false);
        this.confusionEffect.SetActive(false);
        this.confusionChargeEffect.SetActive(false);

        _input.isReverse = false;
        ForceToMove(point: this.respawnPoint);
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

    private int GetValueIncreaseToNotExceedMaxValue(int startValue, int maxValue, int increaseValue)
    {
        startValue += increaseValue;

        if(startValue > maxValue)
        {
            startValue = maxValue;
        }

        return startValue;
    }

    private int GetValueDecreaseToNotExceedMinValue(int startValue, int minValue, int decreaseValue)
    {
        startValue -= decreaseValue;

        if(startValue < minValue)
        {
            startValue = minValue;
        }

        return startValue;
    }

    #endregion

    private void InitPlayerPreconditions(
        out int hpMax,
        out int lifeMax,
        out int keyMax,
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
        keyMax = ValueManager.PLAYER_KEY_MAX;
        poisonStackMax = ValueManager.PLAYER_POISON_STACK_MAX;
        confusionStackMax = ValueManager.PLAYER_CONFUSION_STACK_MAX;
        poisonTicDamage = ValueManager.PLAYER_POISON_TIC_DAMAGE;
        detoxActivateTime = ValueManager.PLAYER_DETOX_ACTIVATE_TIME;
        confusionStackUpdateTime = ValueManager.PLAYER_CONFUSION_STACK_UPDATE_TIME;
        confusionDuration = ValueManager.PLAYER_CONFUSION_DURATION;
    }

    private void InitPlayerContidions()
    {
        // IngameAttribute 값 적용
        manager.SetPlayerIngameAttributes(
            isActiveBeads: out bool[] _isActieBeads,
            isActiveMinimap: out bool _isActiveMinimap,
            isActiveMagicFairy: out bool _isActiveMagicFairy,
            isActiveMagicHuman: out bool _isActiveMagicHuman,
            isTutorialClear: out bool _isTutorial,
            life: out int _currentLife,
            key: out int _currentKey,
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
        this.isTutorial = !_isTutorial;
        this.currentLife = _currentLife;
        this.currentKey = _currentKey;
        this.currentHp = _currentHp;
        this.confusionStack = _confusionStack;
        this.magicGiantStack = _magicGiantStack;
        this.poisonStack = _poisonStack;

        // 플레이어 초기 위치 세팅
        if (_savedPositionEnabled)
        {
            ForceToMove(new Vector3(_savedPositionX, _savedPositionY, _savedPositionZ));
        }
    }

    private void InitControllerSettings()
    {
        if (this._controller != null)
        {
            this._controller.MoveSpeed = ValueManager.PLAYER_MOVE_SPEED_DEFAULT;
            this._controller.SprintSpeed = ValueManager.PLAYER_SPRINT_SPEED_DEFAULT;

            if (this.isActiveMagicFairy)
            {
                this._controller.MoveSpeed *= ValueManager.PLAYER_MAGIC_SPEED_RATIO;
                this._controller.SprintSpeed *= ValueManager.PLAYER_MAGIC_SPEED_RATIO;
            }
        }
    }

    private bool TryGetTrapByGameObject(GameObject obj, out Trap trap)
    {
        trap = obj.GetComponent<Trap>();

        return trap != null;
    }

    private bool TryGetMontserByGameObject(GameObject obj, out Monster monster)
    {
        monster = obj.GetComponentInParent<Monster>();

        return monster != null;
    }

    private bool TryGetMonsterAttackRockByGameObject(GameObject obj, out MonsterAttackRock rock)
    {
        rock = obj.GetComponentInParent<MonsterAttackRock>();

        return rock != null;
    }

    private bool TryGetMonsterMissileByGameObject(GameObject obj, out MonsterMissile missile)
    {
        missile = obj.GetComponentInParent<MonsterMissile>();

        return missile != null;
    }
}
