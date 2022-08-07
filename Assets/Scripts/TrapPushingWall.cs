using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPushingWall : Trap
{
    public float pushingPower;
    
    Player player;
    bool isActivate;
    float positionModifyValue = 6f;

    private void Update()
    {
        if (isActivate && player != null)
        {
            player.ForceToMove(player.transform.position + this.transform.forward * pushingPower * Time.deltaTime);
        }
    }

    public override void ActivateEvent(Player player = null)
    {
        isActivate = true;
        this.player = player;

        Vector3 initVec = this.transform.position;
        initVec.y = player.transform.position.y;
        initVec += this.transform.forward * positionModifyValue;
        player.ForceToMove(point: initVec);
    }

    public override void DeactivateEvent(Player player = null)
    {
        isActivate = false;
        this.player = null;
    }
}
