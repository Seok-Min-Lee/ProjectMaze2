using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public TrapType type;
    protected bool isActive;

    public virtual void ActivateEvent(Player player = null)
    {
        
    }

    public virtual void DeactivateEvent(Player player = null)
    {

    }
}
