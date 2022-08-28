using UnityEngine;

public class TrapMachPairColumn : Trap
{
    public Material[] materials;
    public TrapMachPairManager manager;

    public TrapMachPairType machType;
    public float speed;

    private bool isRising;
    private float startPositionY;
    private Vector3 toVector;

    public override void ActivateEvent(Player player = null)
    {
        UpdateMaterial();
        this.isRising = true;
    }

    private void Start()
    {
        this.startPositionY = this.transform.position.y;
        this.toVector = new Vector3(this.transform.position.x, -this.startPositionY, this.transform.position.z);

        this.transform.position = this.toVector + new Vector3(0, ValueManager.TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y, 0);
    }

    private void Update()
    {
        UpdatePosition(maxY: this.startPositionY);
    }

    public void DetectPlayer()
    {
        this.manager.MachPair(type: this.machType);

        this.isActive = false;
        this.gameObject.SetActive(this.isActive);
    }

    private void UpdatePosition(float maxY)
    {
        if (this.isRising)
        {
            if (this.toVector.y < maxY)
            {
                this.toVector.y += this.speed * Time.deltaTime;

                if(this.toVector.y > maxY)
                {
                    this.toVector.y = maxY;
                }

                this.transform.position = this.toVector;
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
