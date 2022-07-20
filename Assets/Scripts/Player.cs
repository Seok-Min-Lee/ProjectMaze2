using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public Transform respawnPoint;
    public int currentHp, maxHp;
    public int currentLife, maxLife;
    public bool enableMinimap;
    public bool[] enableBeads;

    Vector3 respawnPoint;

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
        else if (other.tag == NameManager.TAG_PLAYER_RESPAWN)
        {
            // ������ ���� ������Ʈ
            respawnPoint = other.GetComponent<Transform>().position + (Vector3.up * 20);
            //Debug.Log("Respawn Point - x :" + respawnPoint.x + " y : " + respawnPoint.y + " z : " + respawnPoint.z);
            //respawnPoint = other.transform;
        }
        else if (other.tag == NameManager.TAG_FALL)
        {
            Respawn();
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
}
