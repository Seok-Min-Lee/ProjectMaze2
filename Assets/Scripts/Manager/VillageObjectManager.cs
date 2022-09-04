using System.Collections.Generic;
using UnityEngine;

public class VillageObjectManager : MonoBehaviour
{
    public GameObject portal;
    public GameObject dragonGroup;
    public MeshRenderer[] invisuableWalls;
    public AudioSource backgroundMusicA, backgroundMusicB;
    bool isClearGame;

    private GameManager gameManager;
    private Player player;
    private List<NpcObject> npcs;

    private void Start()
    {
        this.gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        this.player = GameObject.FindGameObjectWithTag(NameManager.TAG_PLAYER).GetComponent<Player>();

        this.isClearGame = SystemManager.instance.isClearGame;

        foreach(MeshRenderer wall in invisuableWalls)
        {
            wall.enabled = !this.isClearGame;
        }

        this.npcs = new List<NpcObject>();
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag(NameManager.TAG_NPC))
        {
            NpcObject npc = gameObject.GetComponent<NpcObject>();
            this.npcs.Add(npc);
        }

        this.dragonGroup.SetActive(this.isClearGame);
        this.portal.SetActive(this.isClearGame);

        if (this.isClearGame)
        {
            this.backgroundMusicB.Play();
            this.backgroundMusicB.loop = true;
        }
        else
        {
            this.backgroundMusicA.Play();
            this.backgroundMusicA.loop = true; 
        }
    }

    public void OpenPortal()
    {
        this.portal.SetActive(true);
    }
}
