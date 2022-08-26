using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageObjectManger : MonoBehaviour
{
    public GameObject[] giantStoneStatues;
    public GameObject[] giantTwins;

    bool isClearGame;

    private void Start()
    {
        this.isClearGame = SystemManager.instance.isClearGame;

        foreach(GameObject giantStoneStatue in giantStoneStatues)
        {
            giantStoneStatue.SetActive(!this.isClearGame);
        }
        foreach (GameObject giantTwin in giantTwins)
        {
            giantTwin.SetActive(this.isClearGame);
        }
    }
}
