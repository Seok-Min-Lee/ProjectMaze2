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
            // ������ ���� ������Ʈ
            respawnPoint = other.GetComponent<Transform>();
        }
    }

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Heal:
                // ü�� 0 ������ ��� �������� ������ ���� �߰� �ʿ�.
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
