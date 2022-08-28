using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapLift : Trap
{
    public float speed;

    private bool isRising;
    private float startPositionY;
    private Vector3 toVector;

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
        this.startPositionY = this.transform.position.y;
        this.toVector = new Vector3(this.transform.position.x, -startPositionY, this.transform.position.z);

        // 바닥과 충돌 방지를 위해 보정해준다.
        this.transform.position = this.toVector + new Vector3(0, ValueManager.TRAP_LIFT_CALIBRATION_START_POSITION_Y, 0);
    }

    private void Update()
    {
        UpdatePosition(maxY: this.startPositionY);
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

                    StopAllCoroutines();
                    StartCoroutine(UpdateRisingState());
                }

                this.transform.position = toVector;
            }
        }
        else
        {
            if (this.toVector.y > - maxY)
            {
                this.toVector.y -= this.speed * Time.deltaTime;

                if (this.toVector.y < - maxY)
                {
                    this.toVector.y = - maxY;

                    StopAllCoroutines();
                    StartCoroutine(UpdateRisingState());
                }

                this.transform.position = toVector;
            }
        }
    }

    private IEnumerator UpdateRisingState()
    {
        yield return new WaitForSeconds(ValueManager.TRAP_LIFT_DERECTION_CHANGE_DELAY);
        this.isRising = !this.isRising;
    }
}
