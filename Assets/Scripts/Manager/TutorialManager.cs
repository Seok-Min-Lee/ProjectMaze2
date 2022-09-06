using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] tutorialWalls;

    Player player;
    GameManager gameManager;

    List<GameObject> itemAll;

    bool wasAddict, wasDetox, wasConfusion, isGuideVisible;

    private void Start()
    {
        this.gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        this.player = GameObject.FindGameObjectWithTag(NameManager.TAG_PLAYER).GetComponent<Player>();

        itemAll = new List<GameObject>();
        itemAll.AddRange(GameObject.FindGameObjectsWithTag(NameManager.TAG_ITEM));
    }

    private void Update()
    {
        CheckPlayerAddictAndDetox();
        CheckPlayerConfuion();
        if(!this.isGuideVisible && gameManager.preferenceGuideVisible)
        {
            this.isGuideVisible = gameManager.preferenceGuideVisible;
        }

        // 1���� : ������ ��� �Ա�
        if (tutorialWalls[0].activeSelf && !ExistItem())
        {
            tutorialWalls[0].SetActive(false);
        }

        // 2���� : �ߵ� �� �ص�
        if (tutorialWalls[1].activeSelf && this.wasAddict && this.wasDetox)
        {
            tutorialWalls[1].SetActive(false);
        }

        // 3���� : ����
        if(tutorialWalls[2].activeSelf && this.wasConfusion)
        {
            tutorialWalls[2].SetActive(false);
        }

        // 4���� : ���̵� �ѱ�
        if (tutorialWalls[3].activeSelf && this.isGuideVisible)
        {
            tutorialWalls[3].SetActive(false);
        }
    }

    private bool ExistItem()
    {
        foreach(GameObject item in itemAll)
        {
            if (item.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void CheckPlayerAddictAndDetox()
    {
        if (this.wasAddict)
        {
            // �ص��� �ƴ� ��Ȱ�� ���� �����Ѵ�.
            if(!player.isPoison && player.poisonStack > 0)
            {
                this.wasDetox = true;
            }
        }
        else
        {
            if (player.isPoison)
            {
                this.wasAddict = true;
            }
        }
    }

    private void CheckPlayerConfuion()
    {
        if(!this.wasConfusion && player.isConfusion)
        {
            this.wasConfusion = true;
        }
    }
}
