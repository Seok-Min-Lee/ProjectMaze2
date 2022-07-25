using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDeactivator : MonoBehaviour
{
    public TrapActivator activator;

    public void CallDeactivateTrap()
    {
        activator.DeactivateTrap();

        this.gameObject.SetActive(false);
    }
}
