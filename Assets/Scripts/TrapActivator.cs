using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject effect;
    public GameObject[] traps;
    public TrapDeactivator deactivator;
    public bool isVolatility;

    private void Start()
    {
        if (isVolatility && effect != null)
        {
            effect.SetActive(true);
        }
    }

    public void ActivateTrap(Player player = null)
    {
        foreach(GameObject trap in traps)
        {
            trap.GetComponent<Trap>().ActivateEvent(player: player);
        }
        
        if (isVolatility)
        {
            this.gameObject.SetActive(false);
        }

        if(deactivator != null)
        {
            deactivator.gameObject.SetActive(true);
        }
    }

    public void DeactivateTrap(Player player = null)
    {
        foreach (GameObject trap in traps)
        {
            trap.GetComponent<Trap>().DeactivateEvent(player: player);
        }
    }
}
