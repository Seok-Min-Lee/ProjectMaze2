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
    public GameObject[] beads;

    public int currentHp, maxHp;
    public int currentLife, maxLife;
    public int currentConfusion = 0, maxConfusion = 100;
    public bool[] enableBeads;

    public bool isPoison;
    public bool isConfusion;
    public bool isInteract, wasInteractPreprocess, isInteractPreprocessReady;

    CharacterController controller;
    Vector3 respawnPoint, interactPoint;
    NPC interactNpc;

    int poisonStack = 0;
    float countTimeDetox = 0f, countTimeConfusion = 0f;

    int poisonStackMax, poisonTicDamage;
    float activateTimeDetox, activateTimeConfusion, durationConfusion;

    #region ##### ����Ƽ ���� �Լ� #####
    
    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        enableBeads = new bool[3];

        manager.InitializePlayer(
            playerPoisonStackMax: out poisonStackMax,
            playerPoisonTicDamage: out poisonTicDamage,
            playerActivateTimeDetox: out activateTimeDetox,
            playerActivateTimeConfusion: out activateTimeConfusion,
            playerDurationConfusion: out durationConfusion
        );
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //OnDamage();
        Detoxify();
        InteractPreprocess();
        Interact();
        InteractPostProcess();
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
                Respawn();
                break;

            case NameManager.TAG_MONSTER_ATTACK:
                OnDamage(monster: other.GetComponentInParent<Monster>());
                break;

            case NameManager.TAG_MONSTER_TURN_BACK_AREA:
                other.GetComponentInParent<MonsterRange>().TurnBack();
                break;

            case NameManager.TAG_TRAP_ACTIVATOR:
                other.GetComponentInParent<TrapActivator>().ActivateTrap(player: this);
                break;

            case NameManager.TAG_TRAP_DEACTIVATOR:
                other.GetComponentInParent<TrapDeactivator>().CallDeactivateTrap(player: this);
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
                OnTriggerStayInNpcInteractionZone(npc: other.GetComponent<NPC>());
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

    #endregion

    #region ##### ���� ��� �Լ� #####

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Heal:
                // hp, life �Ѵ� 0 �� ��� GameOver �߰� �ʿ�.
                ChangeCurrentHp(value: item.value);
                break;
            case ItemType.Map:
                // �̴ϸ� ������ �÷��̾ �ƴ� �ܺο��� �ϰ� �� ��� ����.
                manager.ActivateMinimap(isActive: true);
                break;
            case ItemType.Bead:
                // ���� ������ �÷��̾ �ƴ� �ܺο��� �ϰ� �� ��� ����.
                ActivateBead(index: item.value);
                break;
        }

        gameObject.SetActive(false);
    }

    private void ActivateBead(int index)
    {
        if(index >= 0 && index < enableBeads.Length)
        {
            enableBeads[index] = true;
            beads[index].SetActive(true);
        }
    }

    private void OnDamage(Monster monster)
    {
        KnockBack();    // ���� �߰� �ʿ�.

        // monster �� ������Ʈ �� ��.
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

        StartCoroutine(routine: Addict(summaryDamage: -poisonTicDamage * poisonStack));
    }

    private void KnockBack()
    {

    }

    private void OnDamageByInsect(int damage)
    {
        currentHp -= damage;    // �⺻���� ������

        isPoison = true;        // �ߵ� ���� Ȱ��ȭ
        poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;  // �ߵ� ���� ����
        ChangePoisonEffect(isAddict: isPoison); // �ߵ� ����Ʈ Ȱ��ȭ

        countTimeDetox = 0;       // �ص� Ÿ�̸� �ʱ�ȭ
    }

    private IEnumerator Addict(int summaryDamage)
    {
        if (isPoison)
        {
            ChangeCurrentHp(value: summaryDamage);
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(routine: Addict(summaryDamage: summaryDamage));
    }

    private void Detoxify()
    {
        if (isPoison)
        {
            UpdateCountTimeDetox();

            if(countTimeDetox > activateTimeDetox)
            {
                isPoison = false;
                ChangePoisonEffect(isAddict: isPoison);
            }
        }
    }

    private void OnTriggerStayInNegativeEffectZone(NegativeEffectZone negativeEffect)
    {
        if (!isConfusion)
        {
            Timer(tick: Time.deltaTime, time: ref countTimeConfusion);
            if (countTimeConfusion >= activateTimeConfusion)
            {
                StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                currentConfusion += negativeEffect.value;
                countTimeConfusion = 0f;

                if (currentConfusion >= maxConfusion)
                {
                    StartCoroutine(routine: Confuse());
                }
            }
        }
    }

    private void OnTriggerStayInNpcInteractionZone(NPC npc)
    {
        interactNpc = npc;
        isInteractPreprocessReady = true;
        interactPoint = npc.transform.position;
    }

    private void OnTriggerStayInTrap(GameObject trapGameObject)
    {
        TrapTrafficLight trap = trapGameObject.GetComponent<TrapTrafficLight>();
        if (trap.type == TrapTrafficLightType.Red &&
            IsMoving())
        {
            Respawn();
            trap.DeactivateEvent(player: this);
        }
    }

    private void OnTriggerExitFromNpcInteractionZone()
    {
        isInteractPreprocessReady = false;
        wasInteractPreprocess = false;
        isInteract = false;
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

            // ī�޶� ��ġ ����
            Vector3 cameraPosition = manager.npcInteractionCamera.gameObject.transform.position;
            cameraPosition.x = this.transform.position.x;
            manager.MoveGameObject(gameObject: manager.npcInteractionCamera, vector: cameraPosition);

            // ī�޶� �� UI ������Ʈ.
            manager.UpdateUINormalToInteraction(npc: interactNpc);

            // ��ȣ�ۿ� �غ� ���� ������Ʈ.
            wasInteractPreprocess = true;
            isInteract = true;

            _input.interact = false;
        }
    }

    private void Interact()
    {
        //InteractPreprocess();

        if (wasInteractPreprocess && isInteract)
        {
            // ��ȣ�ۿ� ����
            if (_input.interact)
            {
                isInteract = !manager.TryUpdateInteractionUI();

                _input.interact = false;
            }
        }

        //InteractPostProcess();
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
            // UI ������Ʈ.
            manager.UpdateUIWhetherInteraction(isInteract: false);

            // ��ȣ�ۿ� �ٽ� �� �� �ְ� �ϱ� ���� ���� �ʱ�ȭ
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
        yield return new WaitForSeconds(durationConfusion);

        //�÷��̾� ����Ʈ ��Ȱ��ȭ �߰�
        confusionEffect.SetActive(false);
        _input.isReverse = false;
        isConfusion = false;

        currentConfusion = 0;
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

    public void ChangeCurrentHp(int value)
    {
        currentHp += value;

        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        else if (currentHp < 0)
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

    private void StopPlayerMotion()
    {
        _input.MoveInput(newMoveDirection: Vector2.zero);
        _input.JumpInput(newJumpState: false);
        _input.SprintInput(newSprintState: false);
        _input.InteractInput(newInteractState: false);
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
