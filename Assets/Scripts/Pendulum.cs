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
        lerpTime += Time.deltaTime * speed;
        transform.rotation = CalculateMovementOfPendulum();
    }

    private Quaternion CalculateMovementOfPendulum()
    {
        if(direction == pendulumDirectionType.X_axis)
        {
            return Quaternion.Lerp(Quaternion.Euler(Vector3.forward * angle),
                Quaternion.Euler(Vector3.back * angle), GetLerpTParam());
        }
        else
        {
            return Quaternion.Lerp(Quaternion.Euler(Vector3.right * angle),
                Quaternion.Euler(Vector3.left * angle), GetLerpTParam());
        }
    }

    private float GetLerpTParam()
    {
        return (Mathf.Sin(lerpTime) + 1) * 0.5f;
    }
}