using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject[] traps;
    public TrapDeactivator deactivator;
    public bool isVolatility;

    GameObject deactivatorObject;

    public void ActivateTrap()
    {
        foreach(GameObject trap in traps)
        {
            trap.GetComponent<Trap>().ActivateEvent();
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

    public void DeactivateTrap()
    {
        foreach (GameObject trap in traps)
        {
            trap.GetComponent<Trap>().DeactivateEvent();
        }
    }
}
