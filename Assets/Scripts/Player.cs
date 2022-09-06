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

        InitializePlayer(
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

        if (_savedPositionEnabled)
        {
            ForceToMove(new Vector3(_savedPositionX, _savedPositionY, _savedPositionZ));
        }

        InitControllerSettings(controller: this._controller);
    }

    private void Update()
    {
        Detoxify();
        Interact();
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
                // 리스폰 지점 업데이트
                respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * ValueManager.PLAYER_CALIBRATION_RESPAWN_Y);
                break;

            case NameManager.TAG_FALL:
                OnDamage(value: maxHp, isAvoidable: false);
                break;

            case NameManager.TAG_MONSTER_ATTACK:
                OnDamageByMonsterAttack(obj: gameObject);
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
                OnTriggerStayInTrap(trapGameObject: other.gameObject);
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

    #endregion

    #region ##### 실질 기능 함수 #####
    public void ActivateMagicFairy(bool isActive)
    {
        this.isActiveMagicFairy = isActive;
        InitControllerSettings(controller: this._controller);
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
        if (this.isInteractable && _input.interact)
        {
            this.isInteract = true;
        }
        else
        {
            this.isInteract = false;
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

    private void InitializePlayer(
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

    private void InitControllerSettings(ThirdPersonController controller)
    {
        if(controller != null)
        {
            controller.MoveSpeed = ValueManager.PLAYER_MOVE_SPEED_DEFAULT;
            controller.SprintSpeed = ValueManager.PLAYER_SPRINT_SPEED_DEFAULT;

            if (this.isActiveMagicFairy)
            {
                controller.MoveSpeed *= ValueManager.PLAYER_MAGIC_SPEED_RATIO;
                controller.SprintSpeed *= ValueManager.PLAYER_MAGIC_SPEED_RATIO;
            }
        }
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
            case ItemType.Key:
                GetItemKey(value: item.value);
                break;
        }

        itemSound.Play();

        string messageText = ValueManager.MESSAGE_PREFIX_ITEM + item.name + ValueManager.MESSAGE_SUFFIX_ITEM;
        manager.DisplayConfirmMessage(text: messageText, type: EventMessageType.Item);

        gameObject.SetActive(false);
    }

    private void GetItemBead(int index)
    {
        if(index >= 0 && index < this.isActiveBeads.Length)
        {
            this.isActiveBeads[index] = true;
        }

        manager.UpdateByPlayerActiveBeads(isActives: this.isActiveBeads);
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

    private void GetItemKey(int value)
    {
        if (this.currentKey >= this.maxKey)
        {
            this.currentKey = this.maxKey;
        }
        else
        {
            this.currentKey += value;
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
                    OnDamageByInsect(insect: monster.GetComponent<MonsterInsect>() ,damage: monster.damage);
                    break;
                case MonsterType.Zombie:
                    break;
                case MonsterType.Turret:
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

        this.damageSound.Play();
        StartCoroutine(routine: Addict(stack: this.poisonStack, ticDamage: this.poisonTicDamage));
    }

    private void KnockBack()
    {

    }

    private void OnDamageByInsect(MonsterInsect insect, int damage)
    {
        if (insect.isAttack)
        {
            OnDamage(value: damage, isAvoidable: true); // 기본공격 데미지

            isPoison = true;        // 중독 상태 활성화
            poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;  // 중독 스택 변경

            ChangePoisonEffect(isAddict: isPoison); // 중독 이펙트 활성화

            countTimeDetox = 0;       // 해독 타이머 초기화
        }
    }

    private IEnumerator Addict(int stack, int ticDamage)
    {
        if (this.isPoison)
        {
            manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_ADDICT, type: EventMessageType.Debuff);

            OnDamage(value: stack * ticDamage, isAvoidable: false);
            
            yield return new WaitForSeconds(ValueManager.PLAYER_POISON_TIC_DELAY);
            
            StartCoroutine(routine: Addict(stack: stack, ticDamage: ticDamage));
        }
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

                manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_DETOX, type: EventMessageType.Recovery);
            }
        }
    }

    private void OnTriggerEnterToInteractionZone(InteractionZone zone)
    {
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
                OnTriggerStayInNegativeEffectZoneTypePoison(value : negativeEffect.value);
                break;
        }
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

    public void OnTriggerExitFromInteractionZone()
    {
        this.isInteractable = false;
        this.interactableInteractionType = InteractionType.None;
        this.interactableGuideType = GuideType.None;
        this.interactableNpcObject = null;
    }

    private void OnTriggerStayInNegativeEffectZoneTypeConfusion(int value)
    {
        if (!isConfusion)
        {
            Timer(tick: Time.deltaTime, time: ref countTimeConfusion);
            if (countTimeConfusion >= confusionActivateTime)
            {
                StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                this.confusionStack += value;
                this.countTimeConfusion = 0f;

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
                    Resurrect();
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

    #endregion
}
