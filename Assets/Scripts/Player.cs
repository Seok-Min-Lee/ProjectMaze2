using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform respawnPoint;
    public int currentHp, maxHp;
    public int currentLife, maxLife;
    public bool enableMinimap;
    public bool[] enableBeads;

    private void Awake()
    {
        enableBeads = new bool[3];
    }

    private void Update()
    {
        //OnDamage();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == NameManager.TAG_ITEM)
        {
            GameObject gameObject = other.gameObject;

            GetItem(gameObject: gameObject);
            gameObject.SetActive(false);
        }
        else if(other.tag == NameManager.TAG_PLAYER_RESPAWN)
        {
            // 리스폰 지점 업데이트
            respawnPoint = other.GetComponent<Transform>();
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
            if(currentLife > 0)
            {
                Respawn();
            }

            currentHp = 0;
        }
    }

    private void Respawn()
    {
        this.transform.position = respawnPoint.position;
        currentHp = maxHp;
        currentLife--;
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
}
