using UnityEngine;

public class TrapMachPairColumn : Trap
{
    public Material[] materials;
    public TrapMachPairManager manager;

    public TrapMachPairType machType;
    public float speed;

    bool isRising;
    float positionY;
    Vector3 positionVector;

    public override void ActivateEvent(Player player = null)
    {
        UpdateMaterial();
        this.isRising = true;
    }

    private void Start()
    {
        positionY = transform.position.y;
        positionVector = new Vector3(this.transform.position.x, -positionY, this.transform.position.z);

        this.transform.position = positionVector + new Vector3(0, ValueManager.TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y, 0);
    }

    private void Update()
    {
        UpdatePosition();
    }

    public void DetectPlayer()
    {
        manager.MachPair(type: this.machType);

        isActive = false;
        this.gameObject.SetActive(isActive);
    }

    private void UpdatePosition()
    {
        if (this.isRising)
        {
            if (positionVector.y < positionY)
            {
                positionVector.y += speed * Time.deltaTime;
                positionVector.y = positionVector.y > positionY ? positionY : positionVector.y;

                this.transform.position = positionVector;
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
