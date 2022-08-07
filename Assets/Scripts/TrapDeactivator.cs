using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDeactivator : MonoBehaviour
{
    public TrapActivator activator;

    public void CallDeactivateTrap(Player player = null)
    {
        activator.DeactivateTrap(player: player);

        if (activator.isVolatility)
        {
            this.gameObject.SetActive(false);
        }
    }
}
