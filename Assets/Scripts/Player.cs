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

    #region ##### ����Ƽ ���� �Լ� #####

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
                respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * 10);    // ������ ���� ������Ʈ
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

    #region ##### ���� ��� �Լ� #####
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
            // ��ȣ�ۿ� �� ���� ��ũ��Ʈ ���� ���ο� ���� ��ȣ�ۿ� ���� ������Ʈ.
            manager.UpdateInteractionUI(isContinuable: out bool _isInteract);
            this.isInteract = _isInteract;

            // Ű �Է� �ʱ�ȭ.
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
                // ���� ������ �÷��̾ �ƴ� �ܺο��� �ϰ� �� ��� ����.
                GetItemBead(index: item.value);
                break;
            case ItemType.Heal:
                // hp, life �Ѵ� 0 �� ��� GameOver �߰� �ʿ�.
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
        KnockBack();    // ���� �߰� �ʿ�.

        // monster �� ������Ʈ �� ��.
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
        OnDamage(value: damage, isAvoidable: true); // �⺻���� ������

        isPoison = true;        // �ߵ� ���� Ȱ��ȭ
        poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;  // �ߵ� ���� ����
        ChangePoisonEffect(isAddict: isPoison); // �ߵ� ����Ʈ Ȱ��ȭ

        countTimeDetox = 0;       // �ص� Ÿ�̸� �ʱ�ȭ
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
        // ����, �̵� �Է� ���� ��� ���� ���·� �Ǵ�
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
        // ��ȣ�ۿ� �� �� ���� �����ϱ� ���� ���� ����
        // 1. ��ȣ�ۿ� ���°� �ƴϴ�.
        // 2. ��ȣ�ۿ� ��ó���� ���� ���� �����̴�.
        // 3. ��ȣ�ۿ� ��ó�� �غ� �����̴�.
        // 4. ��ȣ�ۿ� ��ư �Է��� ���Դ�.
        if (!isInteract &&
            !wasInteractPreprocess &&
            isInteractPreprocessReady && 
            _input.interact)
        {
            // �÷��̾� ��ġ NPC ������ �̵� �� ����
            // NPC �ٶ󺸵��� ���� ����.
            ForceToMove(point: interactPoint);
            StopPlayerMotion();
            this.transform.LookAt(target: interactNpc.transform);

            // ī�޶� ��ġ ����
            manager.MoveGameObject(gameObject: manager.npcInteractionCamera, vector: interactNpc.cameraPoint.position);
            manager.npcInteractionCamera.transform.rotation = interactNpc.cameraPoint.rotation;

            // ī�޶� �� UI ������Ʈ.
            manager.UpdateUINormalToInteraction(npc: interactNpc);

            // ��ȣ�ۿ� �غ� ���� ������Ʈ.
            wasInteractPreprocess = true;
            isInteract = true;
        }
    }

    private void InteractPostProcess()
    {
        // ��ȣ�ۿ� �� �� ���� �����ϱ� ���� ���� ����
        // 1. ��ȣ�ۿ� ���°� �ƴϴ�.
        // 2. ��ȣ�ۿ� ��ó���� �� �����̴�.
        // 3. ��ȣ�ۿ� ��ư �Է��� ���Դ�.
        if (!isInteract &&
            wasInteractPreprocess &&
            _input.interact)
        {
            // UI �� NPC ������Ʈ.
            manager.UpdateUIWhetherInteraction(isInteract: false);
            interactNpc.DisappearByVolatility(isVolatility: interactNpc.isVolatility);

            // ��ȣ�ۿ� �ٽ� �� �� �ְ� �ϱ� ���� ���� �ʱ�ȭ
            isInteractPreprocessReady = false;
            wasInteractPreprocess = false;

            // ��ȣ�ۿ� Ű �Է� �ʱ�ȭ
            _input.interact = false;
        }
    }

    IEnumerator Confuse()
    {
        isConfusion = true;
        _input.isReverse = true;

        // �÷��̾� ����Ʈ Ȱ��ȭ �߰�
        confusionEffect.SetActive(true);
        yield return new WaitForSeconds(confusionDuration);

        //�÷��̾� ����Ʈ ��Ȱ��ȭ �߰�
        confusionEffect.SetActive(false);
        _input.isReverse = false;
        isConfusion = false;

        confusionStack = 0;
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
