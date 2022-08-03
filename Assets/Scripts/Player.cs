using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class Player : MonoBehaviour
{
    public GameManager manager;
    public GameObject addictEffect, detoxEffect, confusionEffect, confusionChargeEffect;
    public GameObject[] beads;

    public int currentHp, maxHp;
    public int currentLife, maxLife;
    public int currentConfusion = 0, maxConfusion = 100;
    public bool enableMinimap;
    public bool[] enableBeads;

    public bool isPoison;
    public bool isConfusion;
    public bool isInteract, wasInteractPreprocess, isInteractPreprocessReady;

    public StarterAssetsInputs _input { get; private set; }
    CharacterController controller;
    Vector3 respawnPoint, interactPoint;
    NPC interactNpc;

    int poisonStack = 0;
    float countTimeDetox = 0f, countTimeConfusion = 0f;

    int poisonStackMax, poisonTicDamage;
    float activateTimeDetox, activateTimeConfusion, durationConfusion;

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
                other.GetComponentInParent<TrapActivator>().ActivateTrap();
                break;

            case NameManager.TAG_TRAP_DEACTIVATOR:
                other.GetComponentInParent<TrapDeactivator>().CallDeactivateTrap();
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == NameManager.TAG_NEAGTIVE_EFFECT)
        {
            if (!isConfusion)
            {
                Timer(tick: Time.deltaTime, time: ref countTimeConfusion);
                if (countTimeConfusion >= activateTimeConfusion)
                {
                    StartCoroutine(routine: ActiveVolatileEffect(effect: confusionChargeEffect, duration: confusionChargeEffect.GetComponent<ParticleSystem>().duration));

                    currentConfusion += other.GetComponent<NegativeEffectZone>().value;
                    countTimeConfusion = 0f;

                    if(currentConfusion >= maxConfusion)
                    {
                        StartCoroutine(routine: Confuse());
                    }
                }
            }
        }
        else if(other.tag == NameManager.TAG_NPC_INTERACTION_ZONE)
        {
            interactNpc = other.GetComponent<NPC>();
            isInteractPreprocessReady = true;
            interactPoint = other.transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == NameManager.TAG_NPC_INTERACTION_ZONE)
        {
            isInteractPreprocessReady = false;
            wasInteractPreprocess = false;
            isInteract = false;
        }
    }

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
                ActivateMinimap();
                break;
            case ItemType.Bead:
                // ���� ������ �÷��̾ �ƴ� �ܺο��� �ϰ� �� ��� ����.
                ActivateBead(index: item.value);
                break;
        }

        gameObject.SetActive(false);
    }

    private void ActivateMinimap()
    {
        enableMinimap = true;
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
            // �÷��̾� ��ġ NPC ������ �̵�.
            ForceToMove(point: interactPoint);

            // ī�޶� ��ġ ����
            Vector3 cameraPosition = manager.npcInteractionCamera.gameObject.transform.position;
            cameraPosition.x = this.transform.position.x;
            manager.MoveGameObject(gameObject: manager.npcInteractionCamera, vector: cameraPosition);

            // ī�޶� �� UI ������Ʈ.
            manager.UpdateUINormalToInteract(isInteract: true, npc: interactNpc);

            // ��ȣ�ۿ� �غ� ���� ������Ʈ.
            wasInteractPreprocess = true;
            isInteract = true;

            _input.interact = false;
        }
    }

    private void Interact()
    {
        if (wasInteractPreprocess && isInteract)
        {
            // ��ȣ�ۿ� ����
            if (_input.interact)
            {
                manager.UpdateInteractionUI(isEnd: out bool isEnd);

                isInteract = isEnd ? false : true;

                _input.interact = false;
            }
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
            // UI ������Ʈ.
            manager.UpdateUIWhetherInteraction(isInteract: false);

            // ��ȣ�ۿ� �ٽ� �� �� �ְ� �ϱ� ���� ���� �ʱ�ȭ
            wasInteractPreprocess = false;

            // ��ȣ�ۿ� Ű �Է� �ʱ�ȭ
            _input.interact = false;
        }
    }

    private void ChangeCurrentHp(int value)
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

    private void Respawn()
    {
        ForceToMove(point: respawnPoint);

        currentHp = maxHp;
        currentLife--;
    }

    private void ForceToMove(Vector3 point)
    {
        // CharacterController �� Ȱ��ȭ�Ǿ� ������ Transform.position ���� �����ص� ������� �ʴ´�.
        controller.enabled = false;
        this.transform.position = point;
        controller.enabled = true;
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
}
