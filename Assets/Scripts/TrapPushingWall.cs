using UnityEngine;

public class TrapPushingWall : Trap
{
    public float pushingPower;
    public Material deactivateMaterial;
    
    private Player player;
    private Vector3 forceVec;

    private void Start()
    {
        this.forceVec = this.transform.forward * this.pushingPower * Time.deltaTime;
    }

    private void Update()
    {
        if (this.isActive && this.player != null)
        {
            this.player.ForceToMove(this.player.transform.position + this.forceVec);
        }
    }

    public override void ActivateEvent(Player player = null)
    {
        if(!this.isActive && player != null)
        {
            this.player = player;

            Vector3 initVec = this.transform.position;
            initVec.y = this.player.transform.position.y;
            player.ForceToMove(point: initVec);

            player._input.controlEnable = false;
            player.InputStop();

            this.isActive = true;
            this.player = player;
        }
    }

    public override void DeactivateEvent(Player player = null)
    {
        player._input.controlEnable = true;

        this.isActive = false;
        this.player = null;

        this.GetComponent<Renderer>().material = this.deactivateMaterial;
    }
}
