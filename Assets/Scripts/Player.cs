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
    public bool isInteract { get; private set; }        // ��ȣ�ۿ� ���� ���� ����
    public bool isInteractable { get; private set; }    // ��ȣ�ۿ� ���� ����
    public bool isTutorial { get; private set; }
    public InteractionType interactableInteractionType { get; private set; }
    public GuideType interactableGuideType { get; private set; }
    public NpcObject interactableNpcObject { get; private set; }

    private ThirdPersonController _controller;
    private CharacterController controller;

    private Vector3 respawnPoint, interactPoint;

    private int poisonStackMax, poisonTicDamage;
    private float detoxActivateTime, confusionActivateTime, confusionDuration, countTimeDetox, countTimeConfusion;

    #region ##### ����Ƽ ���� �Լ� #####

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

        // ȿ���� ���
        itemSound.Play();

        // �ý��� �޼��� ���
        string messageText = ValueManager.MESSAGE_PREFIX_ITEM + item.name + ValueManager.MESSAGE_SUFFIX_ITEM;
        manager.DisplayConfirmMessage(text: messageText, type: EventMessageType.Item);

        // ������ ������Ʈ ��Ȱ��ȭ
        obj.SetActive(false);
    }

    private void OnTriggerEnterToPlayerRespawnPoint(Transform transform)
    {
        // ������ ��ġ ����.
        this.respawnPoint = transform.position + (Vector3.up * ValueManager.PLAYER_CALIBRATION_RESPAWN_Y);
    }

    private void OnTriggerEnterToInteractionZone(InteractionZone zone)
    {
        // ��ȣ�ۿ� ���� ���� ����
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

        // ��� �߻��ؾ� �ϴ� ��� �Է��� ���� ������ ó��
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
        // �ʿ��� ������ ������ ���� ���� ��� ���� ó��.
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
        // ��ȣ�ۿ� ���� ������ ���� ���� �� ���� ���� �ʱ�ȭ
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
                // ���� ������ ���� �� ����Ʈ Ȱ��ȭ
                StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                this.confusionStack += value;
                this.countTimeConfusion = 0f;

                // ���� �������� �ִ뿡 �����ϸ� ���� ȿ�� Ȱ��ȭ
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

    #region ##### ���� ��� �Լ� #####
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
        // ��ȣ�ۿ� ������ ������ �� �Է��� ������ �� Ȯ��
        // ��ȣ�ۿ� ó���� GameManager ���� ó��.
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
        // MonsterAttack�� ��� �Ʒ� ������� Ȯ���Ͽ� ó���Ѵ�.
        // 1. ������ �������� ��������
        // 2. ĳ����Ʈ�� �߻��� ����ü����
        // 3. �ͷ��� �߻��� ����ü����
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
                // ������ �ο� �� ���� �� �Ҹ�
                OnDamage(value: rock.damage, isAvoidable: true);
                rock.ExplosionDestroy();
            }
            else
            {
                if (TryGetMonsterMissileByGameObject(obj: obj, missile: out MonsterMissile missile))
                {
                    // ������ �ο� �� ���� �� �Ҹ�
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
            // �⺻���� ������ �ο�
            OnDamage(value: damage, isAvoidable: true); 

            // �ߵ� Ȱ��ȭ
            this.isPoison = true;
            this.poisonStack = GetValueIncreaseToNotExceedMaxValue(
                startValue: this.poisonStack, 
                maxValue: this.poisonStackMax, 
                increaseValue: 1
            );

            // �ߵ� ����Ʈ Ȱ��ȭ
            ChangePoisonEffect(isAddict: isPoison);
            
            // �ص� Ÿ�̸� �ʱ�ȭ
            countTimeDetox = 0;       
        }
    }

    private IEnumerator Addict(int stack, int ticDamage)
    {
        if (this.isPoison)
        {
            // �ý��� �޼��� ���
            manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_ADDICT, type: EventMessageType.Debuff);

            // �ߵ� ��Ʈ ������ �ο�
            OnDamage(value: stack * ticDamage, isAvoidable: false);
            
            yield return new WaitForSeconds(ValueManager.PLAYER_POISON_TIC_DELAY);
            
            StartCoroutine(routine: Addict(stack: stack, ticDamage: ticDamage));
        }
    }

    private void Detoxify()
    {
        if (this.isPoison)
        {
            // ���� �ð� üũ
            UpdateCountTimeDetox();

            if(this.countTimeDetox > this.detoxActivateTime)
            {
                // �ߵ� ���� ��Ȱ��ȭ
                this.isPoison = false;
                ChangePoisonEffect(isAddict: isPoison);

                // �ý��� �޼��� ���
                manager.DisplayConfirmMessage(text: ValueManager.MESSAGE_PLAYER_DETOX, type: EventMessageType.Recovery);
            }
        }
    }

    private bool TryGetNecessaryBeadIndexByNextSceneType(SceneType type, out int index)
    {
        // ���� ������ �Ѿ�� ���� ������ �ε����� ã�� ���̱� ������ �ϳ��� �մ��ٰ� �����ϸ� �ȴ�.
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
        // ����, �̵� �Է� ���� ��� ���� ���·� �Ǵ�
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

        // �÷��̾� ����Ʈ Ȱ��ȭ �߰�
        confusionEffect.SetActive(true);

        yield return new WaitForSeconds(confusionDuration);

        // �÷��̾� ����Ʈ ��Ȱ��ȭ �߰�
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

    #region ##### �ٿ뵵 �Լ� #####

    public void ForceToMove(Vector3 point)
    {
        // CharacterController �� Ȱ��ȭ�Ǿ� ������ Transform.position ���� �����ص� ������� �ʴ´�.
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
        // isAvoidable ���� ȿ���� ���� �� �ִ� ���������� ���� bool ��.

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
        // IngameAttribute �� ����
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

        // �÷��̾� �ʱ� ��ġ ����
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
