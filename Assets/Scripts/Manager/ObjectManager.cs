using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectManager : MonoBehaviour
{
    private GameManager gameManager;
    private Player player;
    private List<GameObject> npcObjectAll;
    private List<GameObject> npcGiantStatues, npcGiantTwins;
    private List<GameObject> guideObjectAll;
    private GameObject portal;

    private bool isLatestPreferenceGuideVisible;
    private bool isClearGame;
    private string currentSceneName;

    private void Start()
    {
        this.gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        this.player = GameObject.FindGameObjectWithTag(NameManager.TAG_PLAYER).GetComponent<Player>();

        this.isClearGame = SystemManager.instance.isClearGame;
        this.currentSceneName = SceneManager.GetActiveScene().name;

        this.npcObjectAll = GetGameObjectsByTag(tag: NameManager.TAG_NPC);
        this.guideObjectAll = GetGameObjectsByTag(tag: NameManager.TAG_GUIDE);
        this.portal = GameObject.FindGameObjectWithTag(NameManager.TAG_PORTAL);

        SplitNpcObjectsByNpcType(
            npcObjectAll: this.npcObjectAll, 
            npcGiantStatues: out this.npcGiantStatues, 
            npcGiantTwins: out this.npcGiantTwins
        );

        SetGameObjectsByClearOrNot();
        SetGuideObjectByPreference();
    }

    private void Update()
    {
        UpdateGuideObjectVisiblityByPreference();
    }

    public void ActivatePortal()
    {
        this.portal.SetActive(true);
    }

    private void UpdateGuideObjectVisiblityByPreference()
    {
        if(this.isLatestPreferenceGuideVisible != gameManager.preferenceGuideVisible)
        {
            this.isLatestPreferenceGuideVisible = gameManager.preferenceGuideVisible;

            foreach (GameObject guide in guideObjectAll)
            {
                guide.SetActive(this.isLatestPreferenceGuideVisible);
            }

            if (!this.isLatestPreferenceGuideVisible && player.interactableGuideType != GuideType.None)
            {
                player.OnTriggerExitFromInteractionZone();
            }
        }
    }

    private void SetGameObjectsByClearOrNot()
    {
        foreach (GameObject statue in this.npcGiantStatues)
        {
            statue.SetActive(!this.isClearGame);
        }

        foreach (GameObject twin in this.npcGiantTwins)
        {
            twin.SetActive(this.isClearGame);
        }

        if (!this.isClearGame && this.currentSceneName == NameManager.SCENE_VILLAGE)
        {
            portal.SetActive(false);
        }
    }

    private void SetGuideObjectByPreference()
    {
        this.isLatestPreferenceGuideVisible = gameManager.preferenceGuideVisible;

        foreach (GameObject guide in guideObjectAll)
        {
            guide.SetActive(this.isLatestPreferenceGuideVisible);
        }
    }

    private void SplitNpcObjectsByNpcType(
        List<GameObject> npcObjectAll,
        out List<GameObject> npcGiantStatues,
        out List<GameObject> npcGiantTwins)
    {
        npcObjectAll = GetGameObjectsByTag(tag: NameManager.TAG_NPC);
        npcGiantStatues = GetNpcGameObjectsByNpcType(npcAll: npcObjectAll, type: NpcType.GiantStoneStatue);

        npcGiantTwins = new List<GameObject>();
        npcGiantTwins.AddRange(GetNpcGameObjectsByNpcType(npcAll: npcObjectAll, type: NpcType.GiantTwinA));
        npcGiantTwins.AddRange(GetNpcGameObjectsByNpcType(npcAll: npcObjectAll, type: NpcType.GiantTwinB));
    }

    private List<GameObject> GetGameObjectsByTag(string tag)
    {
        List<GameObject> gameObjects = new List<GameObject>();

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag(tag))
        {
            gameObjects.Add(gameObject);
        }

        return gameObjects;
    }

    private List<GameObject> GetNpcGameObjectsByNpcType(IEnumerable<GameObject> npcAll, NpcType type)
    {
        List<GameObject> npcs = new List<GameObject>();

        foreach (GameObject npc in npcAll)
        {
            if(npc.GetComponent<NpcObject>().type == type)
            {
                npcs.Add(npc);
            }
        }

        return npcs;
    }
}
