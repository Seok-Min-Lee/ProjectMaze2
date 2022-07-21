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
                respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * 10);    // 리스폰 지점 업데이트
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
                // hp, life 둘다 0 인 경우 GameOver 추가 필요.
                ChangeCurrentHp(value: item.value);
                break;
            case ItemType.Map:
                // 미니맵 관리를 플레이어가 아닌 외부에서 하게 될 경우 수정.
                ActivateMinimap();
                break;
            case ItemType.Bead:
                // 구슬 관리를 플레이어가 아닌 외부에서 하게 될 경우 수정.
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
        KnockBack();    // 내용 추가 필요.

        // monster 별 업데이트 할 것.
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
        currentHp -= damage;    // 기본공격 데미지

        isPoison = true;        // 중독 상태 활성화
        poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;  // 중독 스택 변경
        ChangePoisonEffect(isAddict: isPoison); // 중독 이펙트 활성화

        stopTime = 0;       // 해독 타이머 초기화
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

        // 점프, 이동 입력 없을 경우 정지 상태로 판단
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
                //Gameover 추가.
            }
        }
    }

    private void Respawn()
    {
        // CharacterController 가 활성화되어 있으면 Transform.position 값을 변경해도 적용되지 않는다.
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;
        this.transform.position = respawnPoint;
        controller.enabled = true;

        currentHp = maxHp;
        currentLife--;
    }
}
