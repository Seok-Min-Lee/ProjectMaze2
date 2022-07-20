using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform respawnPosition;
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
        //Respawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == NameManager.TAG_ITEM)
        {
            GameObject gameObject = other.gameObject;

            GetItem(gameObject: gameObject);
            gameObject.SetActive(false);
        }
    }

    private void GetItem(GameObject gameObject)
    {
        Item item = gameObject.GetComponent<Item>();

        switch (item.type)
        {
            case ItemType.Heal:
                ChangeCurrentHp(value: item.value);
                break;
            case ItemType.Map:
                ActivateMinimap();
                break;
            case ItemType.Bead:
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
}
