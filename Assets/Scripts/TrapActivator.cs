using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapActivator : MonoBehaviour
{
    public GameObject trap;
    public bool isVolatility;

    public void ActivateTrap()
    {
        trap.SetActive(!trap.activeSelf);

        if (isVolatility)
        {
            this.gameObject.SetActive(false);
        }
    }
}
