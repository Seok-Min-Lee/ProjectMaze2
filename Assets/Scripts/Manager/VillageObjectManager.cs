using UnityEngine;

public class VillageObjectManager : MonoBehaviour
{
    public GameObject[] giantStoneStatues;
    public GameObject[] giantTwins;
    public GameObject portal;
    public MeshRenderer[] invisuableWalls;
    public AudioSource backgroundMusicA, backgroundMusicB;
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
        foreach(MeshRenderer wall in invisuableWalls)
        {
            wall.enabled = !this.isClearGame;
        }

        if (this.isClearGame)
        {
            backgroundMusicB.Play();
            backgroundMusicB.loop = true;
            
            portal.SetActive(true);
        }
        else
        {
            backgroundMusicA.Play();
            backgroundMusicA.loop = true; 
            
            portal.SetActive(false);
        }
    }

    public void OpenPortal()
    {
        portal.SetActive(true);
    }
}
