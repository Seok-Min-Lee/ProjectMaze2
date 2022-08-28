using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapLift : Trap
{
    public float speed;

    private bool isRising;
    private float positionY;
    private Vector3 positionVector;

    public override void ActivateEvent(Player player = null)
    {
        if (!this.isActive)
        {
            this.isActive = true;
            this.isRising = true;
        }
    }

    private void Start()
    {
        this.positionY = this.transform.position.y;
        this.positionVector = new Vector3(this.transform.position.x, -positionY, this.transform.position.z);

        // 바닥과 충돌 방지를 위해 보정해준다.
        this.transform.position = this.positionVector + new Vector3(0, ValueManager.TRAP_LIFT_CALIBRATION_START_POSITION_Y, 0);
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (this.isRising)
        {
            if (this.positionVector.y < this.positionY)
            {
                this.positionVector.y += this.speed * Time.deltaTime;

                if(this.positionVector.y > this.positionY)
                {
                    this.positionVector.y = this.positionY;

                    StopAllCoroutines();
                    StartCoroutine(UpdateRisingState());

                }

                this.transform.position = this.positionVector;
            }
        }
        else
        {
            if (this.positionVector.y > -this.positionY)
            {
                this.positionVector.y -= this.speed * Time.deltaTime;

                if (this.positionVector.y < -this.positionY)
                {
                    this.positionVector.y = -this.positionY;

                    StopAllCoroutines();
                    StartCoroutine(UpdateRisingState());

                }

                this.transform.position = this.positionVector;
            }
        }
    }

    private IEnumerator UpdateRisingState()
    {
        yield return new WaitForSeconds(ValueManager.TRAP_LIFT_DERECTION_CHANGE_DELAY);
        this.isRising = !this.isRising;
    }
}
