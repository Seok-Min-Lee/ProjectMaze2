using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class Player : MonoBehaviour
{
    //public Transform respawnPoint;
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
        if (other.tag == NameManager.TAG_ITEM)
        {
            GameObject gameObject = other.gameObject;

            GetItem(gameObject: gameObject);
            gameObject.SetActive(false);
        }
        else if (other.tag == NameManager.TAG_PLAYER_RESPAWN)
        {
            // 리스폰 지점 업데이트
            respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * 20);
            //Debug.Log("Respawn Point - x :" + respawnPoint.x + " y : " + respawnPoint.y + " z : " + respawnPoint.z);
            //respawnPoint = other.transform;
        }
        else if (other.tag == NameManager.TAG_FALL)
        {
            Respawn();
        }
        else if (other.tag == NameManager.TAG_MONSTER_ATTACK)
        {
            OnDamage(monster: other.GetComponentInParent<Monster>());
            //StartCoroutine(routine: OnDamage(monster: other.GetComponentInParent<Monster>()));
        }
    }

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Heal:
                // 체력 0 이하일 경우 리스폰과 라이프 감소 추가 필요.
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
        }
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

    private void Respawn()
    {
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;
        this.transform.position = respawnPoint;
        controller.enabled = true;

        Debug.Log("Respawn Point - x :" + this.transform.position.x + " y : " + this.transform.position.y + " z : " + this.transform.position.z);
        currentHp = maxHp;
        currentLife--;
    }

    private void OnDamage(Monster monster)
    {
        KnockBack();

        switch (monster.type)
        {
            case MonsterType.Insect:
                currentHp -= monster.damage;
                isPoison = true;
                poisonStack = poisonStack < poisonStackMax ? poisonStack + 1 : poisonStackMax;

                break;
            default:
                break;
        }

        StartCoroutine(routine: Poisoned(summaryDamage: poisonTicDamage * poisonStack));
    }

    private void KnockBack()
    {

    }

    IEnumerator Poisoned(int summaryDamage)
    {
        if (isPoison)
        {
            ChangeCurrentHp(value: summaryDamage);
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(routine: Poisoned(summaryDamage: summaryDamage));
    }

    void Detoxify()
    {
        if (isPoison)
        {
            CountStopTime();

            if(stopTime > detoxStopTime)
            {
                isPoison = false;
            }
        }
    }

    void CountStopTime()
    {
        StarterAssetsInputs input = GetComponent<StarterAssetsInputs>();

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
}
