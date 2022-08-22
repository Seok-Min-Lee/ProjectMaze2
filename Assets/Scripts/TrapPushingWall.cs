using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPushingWall : Trap
{
    public float pushingPower;
    public GameObject mark;
    
    Player player;
    Vector3 forceVec;

    private void Start()
    {
        forceVec = this.transform.forward * pushingPower * Time.deltaTime;
    }

    private void Update()
    {
        if (isActive && player != null)
        {
            player.ForceToMove(player.transform.position + forceVec);
        }
    }

    public override void ActivateEvent(Player player = null)
    {
        if(!isActive && player != null)
        {
            Vector3 initVec = this.transform.position;
            initVec.y = player.transform.position.y;
            player.ForceToMove(point: initVec);

            isActive = true;
            this.player = player;
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        isActive = false;
        this.player = null;
        
        mark.SetActive(false);
    }
}
