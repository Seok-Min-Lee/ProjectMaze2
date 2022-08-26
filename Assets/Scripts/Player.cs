using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class Player : MonoBehaviour
{
    public StarterAssetsInputs _input { get; private set; }
    ThirdPersonController _controller;

    public GameManager manager;
    public GameObject addictEffect, detoxEffect, confusionEffect, confusionChargeEffect;
    public GameObject[] beads;

    public int currentHp, maxHp;
    public int currentLife, maxLife;

    public bool[] isActiveBeads;
    public bool isActiveMinimap;
    public int magicGiantStack { get; private set; }
    public bool isActiveMagicFairy { get; private set; }
    public bool isActiveMagicHuman { get; private set; }

    public bool isPoison;
    public bool isConfusion;
    public bool isInteract, wasInteractPreprocess, isInteractPreprocessReady;

    CharacterController controller;
    Vector3 respawnPoint, interactPoint;
    NPCInteractionZone interactNpc;

    float countTimeDetox = 0f, countTimeConfusion = 0f;

    public int confusionStack, confusionStackMax;
    public int poisonStack;
    int poisonStackMax, poisonTicDamage;
    float detoxActivateTime, confusionActivateTime, confusionDuration;

    #region ##### ����Ƽ ���� �Լ� #####
    
    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _controller = GetComponent<ThirdPersonController>();

        InitializeController();

        InitializePlayer(
            poisonStack: out this.poisonStack,
            confusionStack: out this.confusionStack,
            poisonStackMax: out this.poisonStackMax,
            confusionStackMax: out this.confusionStackMax,
            poisonTicDamage: out this.poisonTicDamage,
            detoxActivateTime: out this.detoxActivateTime,
            confusionStackUpdateTime: out this.confusionActivateTime,
            confusionDuration: out this.confusionDuration
        );
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        bool _isActiveMagicFairy, _isActiveMagicHuman;
        int _magicGiantStack;

        manager.SetPlayerIngameAttributes(
            isActiveBeads: out this.isActiveBeads,
            isActiveMinimap: out this.isActiveMinimap,
            isActiveMagicFairy: out _isActiveMagicFairy,
            isActiveMagicHuman: out _isActiveMagicHuman,
            life: out this.currentLife,
            currentHp: out this.currentHp,
            currentConfusion: out this.confusionStack,
            magicGiantStack: out _magicGiantStack,
            poisonStack: out this.poisonStack
        );

        this.isActiveMagicFairy = _isActiveMagicFairy;
        this.isActiveMagicHuman = _isActiveMagicHuman;
        this.magicGiantStack = _magicGiantStack;

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
            manager.UpdateInteractionUI(isContinuable: out isInteract);

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
        manager.UpdatePlayerIngameAttributes(
            isActiveBeads: this.isActiveBeads,
            isActiveMinimap: this.isActiveMinimap,
            isActiveMagicFairy: this.isActiveMagicFairy,
            isActiveMagicHuman: this.isActiveMagicHuman,
            life: this.currentLife,
            currentHp: this.currentHp,
            currentConfusion: this.confusionStack,
            magicGiantStack: this.magicGiantStack,
            poisonStack: this.poisonStack
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
        out int poisonStack,
        out int confusionStack,
        out int poisonStackMax,
        out int confusionStackMax,
        out int poisonTicDamage,
        out float detoxActivateTime,
        out float confusionStackUpdateTime,
        out float confusionDuration
    )
    {
        poisonStack = 0;
        confusionStack = 0;

        poisonStackMax = ValueManager.PLAYER_POISON_STACK_MAX;
        confusionStackMax = ValueManager.PLAYER_CONFUSION_STACK_MAX;
        poisonTicDamage = ValueManager.PLAYER_POISON_TIC_DAMAGE;
        detoxActivateTime = ValueManager.PLAYER_DETOX_ACTIVATE_TIME;
        confusionStackUpdateTime = ValueManager.PLAYER_CONFUSION_STACK_UPDATE_TIME;
        confusionDuration = ValueManager.PLAYER_CONFUSION_DURATION;
    }

    private void UpdateBeadVisibility()
    {
        for(int i=0; i<isActiveBeads.Length; i++)
        {
            beads[i].SetActive(isActiveBeads[i]);
        }
        manager.UpdateUIActivedBeads(isActives: isActiveBeads);
    }

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Heal:
                // hp, life �Ѵ� 0 �� ��� GameOver �߰� �ʿ�.
                ChangeCurrentHp(value: item.value, isDamage: false);
                break;
            case ItemType.Map:
                // �̴ϸ� ������ �÷��̾ �ƴ� �ܺο��� �ϰ� �� ��� ����.
                this.isActiveMinimap = true;
                manager.ActivateMinimap(isActive: this.isActiveMinimap);
                break;
            case ItemType.Bead:
                // ���� ������ �÷��̾ �ƴ� �ܺο��� �ϰ� �� ��� ����.
                ActivateBeadByIndex(index: item.value);
                break;
        }

        gameObject.SetActive(false);
    }

    private void ActivateBeadByIndex(int index)
    {
        if(index >= 0 && index < this.isActiveBeads.Length)
        {
            this.isActiveBeads[index] = true;
            beads[index].SetActive(true);
        }
        manager.UpdateUIActivedBeads(isActives: this.isActiveBeads);
        manager.ActivateSkyboxByPlayerBeads(isActiveBeads: this.isActiveBeads);
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
                    //Gameover �߰�.
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
