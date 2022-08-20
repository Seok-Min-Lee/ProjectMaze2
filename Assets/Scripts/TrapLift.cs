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

    public override void DeactivateEvent(Player player = null)
    {

    }

    private void Start()
    {
        positionY = transform.position.y;
        positionVector = new Vector3(this.transform.position.x, -positionY, this.transform.position.z);

        this.transform.position = positionVector;
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

                    StopCoroutine(UpdateRisingState());
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

                    StopCoroutine(UpdateRisingState());
                    StartCoroutine(UpdateRisingState());

                }

                this.transform.position = positionVector;
            }
        }
    }

    private IEnumerator UpdateRisingState()
    {
        yield return new WaitForSeconds(5f);
        isRising = !isRising;
    }
}
