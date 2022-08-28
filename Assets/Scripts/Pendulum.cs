using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public pendulumDirectionType direction;

    public float angle = 0;
    public float speed = 2f;

    public float lerpTime;

    void Update()
    {
        this.lerpTime += Time.deltaTime * this.speed;
        this.transform.rotation = CalculateMovementOfPendulum();
    }

    private Quaternion CalculateMovementOfPendulum()
    {
        if(this.direction == pendulumDirectionType.X_axis)
        {
            return Quaternion.Lerp(Quaternion.Euler(Vector3.forward * this.angle),
                Quaternion.Euler(Vector3.back * this.angle), GetLerpTParam());
        }
        else
        {
            return Quaternion.Lerp(Quaternion.Euler(Vector3.right * this.angle),
                Quaternion.Euler(Vector3.left * this.angle), GetLerpTParam());
        }
    }

    private float GetLerpTParam()
    {
        return (Mathf.Sin(this.lerpTime) + 1) * 0.5f;
    }
}