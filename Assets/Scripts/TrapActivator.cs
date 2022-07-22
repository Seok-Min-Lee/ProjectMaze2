using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject trap;
    public bool isVolatility;

    public void ActivateTrap()
    {
        trap.GetComponent<Trap>().ActivateEvent();

        if (isVolatility)
        {
            this.gameObject.SetActive(false);
        }
    }
}
