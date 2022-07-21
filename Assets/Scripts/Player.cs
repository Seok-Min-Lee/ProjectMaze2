using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class Player : MonoBehaviour
{
    public GameObject addictEffect, detoxEffect;
    public int currentHp, maxHp;
    public int currentLife, maxLife;
    public bool enableMinimap;
    public bool[] enableBeads;

    Vector3 respawnPoint;

    public bool isPoison;

    int poisonStack = 0, poisonStackMax = 5, poisonTicDamage = 2, poisonSumDamage = 0;
    float stopTime = 0f, detoxStopTime = 5f;

    private void Awake()
    {
        enableBeads = new bool[3];
    }

    private void Update()
    {
        //OnDamage();
        Detoxify();
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
        if(index > 0 && index < enableBeads.Length)
        {
            enableBeads[index] = true;
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
            case MonsterType.Rock:
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

        stopTime = 0;       // �ص� Ÿ�̸� �ʱ�ȭ
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
            CountStopTime();

            if(stopTime > detoxStopTime)
            {
                isPoison = false;
                ChangePoisonEffect(isAddict: isPoison);
            }
        }
    }

    private void CountStopTime()
    {
        StarterAssetsInputs input = GetComponent<StarterAssetsInputs>();

        // ����, �̵� �Է� ���� ��� ���� ���·� �Ǵ�
        if (!input.jump &&
            input.move == Vector2.zero)
        {
            stopTime += Time.deltaTime;
        }
        else
        {
            stopTime = 0;
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
        // CharacterController �� Ȱ��ȭ�Ǿ� ������ Transform.position ���� �����ص� ������� �ʴ´�.
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;
        this.transform.position = respawnPoint;
        controller.enabled = true;

        currentHp = maxHp;
        currentLife--;
    }
}
