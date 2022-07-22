using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject[] traps;
    public bool isVolatility;

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
    }

    public void DeactivateTrap()
    {
        foreach (GameObject trap in traps)
        {
            trap.GetComponent<Trap>().DeactivateEvent();
        }
    }
}
