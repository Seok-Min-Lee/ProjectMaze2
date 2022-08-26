using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapLift : Trap
{
    public float speed;

    bool isRising;
    float positionY;
    Vector3 positionVector;

    public override void ActivateEvent(Player player = null)
    {
        if (!isActive)
        {
            isActive = true;
            isRising = true;
        }
    }

    private void Start()
    {
        positionY = transform.position.y;
        positionVector = new Vector3(this.transform.position.x, -positionY, this.transform.position.z);

        // 바닥과 충돌 방지를 위해 보정해준다.
        this.transform.position = positionVector + new Vector3(0, ValueManager.TRAP_LIFT_CALIBRATION_START_POSITION_Y, 0);
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (this.isRising)
        {
            if (positionVector.y < positionY)
            {
                positionVector.y += speed * Time.deltaTime;

                if(positionVector.y > positionY)
                {
                    positionVector.y = positionY;

                    StopAllCoroutines();
                    //StopCoroutine(UpdateRisingState());
                    StartCoroutine(UpdateRisingState());

                }

                this.transform.position = positionVector;
            }
        }
        else
        {
            if (positionVector.y > -positionY)
            {
                positionVector.y -= speed * Time.deltaTime;

                if (positionVector.y < -positionY)
                {
                    positionVector.y = -positionY;

                    StopAllCoroutines();
                    //StopCoroutine(UpdateRisingState());
                    StartCoroutine(UpdateRisingState());

                }

                this.transform.position = positionVector;
            }
        }
    }

    private IEnumerator UpdateRisingState()
    {
        yield return new WaitForSeconds(ValueManager.TRAP_LIFT_DERECTION_CHANGE_DELAY);
        isRising = !isRising;
    }
}
