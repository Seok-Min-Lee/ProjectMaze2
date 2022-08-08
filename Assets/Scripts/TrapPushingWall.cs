using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPushingWall : Trap
{
    public float pushingPower;
    
    Player player;
    Vector3 forceVec;
    bool isActivate;

    private void Start()
    {
        forceVec = this.transform.forward * pushingPower * Time.deltaTime;
    }

    private void Update()
    {
        if (isActivate && player != null)
        {
            player.ForceToMove(player.transform.position + forceVec);
        }
    }

    public override void ActivateEvent(Player player = null)
    {
        if(!isActivate && player != null)
        {
            Vector3 initVec = this.transform.position;
            initVec.y = player.transform.position.y;
            player.ForceToMove(point: initVec);

            isActivate = true;
            this.player = player;
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        isActivate = false;
        this.player = null;
    }
}
