using UnityEngine;

public class TrapDeactivator : MonoBehaviour
{
    public TrapActivator activator;

    public void CallDeactivateTrap(Player player = null)
    {
        this.activator.DeactivateTrap(player: player);

        if (this.activator.isVolatility)
        {
            this.gameObject.SetActive(false);
        }
    }
}
