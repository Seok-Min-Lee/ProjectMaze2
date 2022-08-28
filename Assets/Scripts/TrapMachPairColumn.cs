using UnityEngine;

public class TrapMachPairColumn : Trap
{
    public Material[] materials;
    public TrapMachPairManager manager;

    public TrapMachPairType machType;
    public float speed;

    private bool isRising;
    private float positionY;
    private Vector3 positionVector;

    public override void ActivateEvent(Player player = null)
    {
        UpdateMaterial();
        this.isRising = true;
    }

    private void Start()
    {
        this.positionY = this.transform.position.y;
        this.positionVector = new Vector3(this.transform.position.x, -this.positionY, this.transform.position.z);

        this.transform.position = this.positionVector + new Vector3(0, ValueManager.TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y, 0);
    }

    private void Update()
    {
        UpdatePosition();
    }

    public void DetectPlayer()
    {
        this.manager.MachPair(type: this.machType);

        this.isActive = false;
        this.gameObject.SetActive(this.isActive);
    }

    private void UpdatePosition()
    {
        if (this.isRising)
        {
            if (this.positionVector.y < this.positionY)
            {
                this.positionVector.y += this.speed * Time.deltaTime;
                this.positionVector.y = this.positionVector.y > this.positionY ? this.positionY : this.positionVector.y;

                this.transform.position = this.positionVector;
            }
        }
    }

    private void UpdateMaterial()
    {
        switch (this.machType)
        {
            case TrapMachPairType.Red:
                this.GetComponent<Renderer>().material = this.materials[0];
                break;
            case TrapMachPairType.Orange:
                this.GetComponent<Renderer>().material = this.materials[1];
                break;
            case TrapMachPairType.Yellow:
                this.GetComponent<Renderer>().material = this.materials[2];
                break;
        }
    }
}
