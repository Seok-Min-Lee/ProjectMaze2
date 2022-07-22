using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public float angle = 0;

    public float lerpTime = 0;
    public float speed = 2f;

    float initX, initY, initZ;

    void Start()
    {
        initX = transform.localRotation.x;
        initY = transform.localRotation.y;
        initZ = transform.localRotation.z;
    }

    void Update()
    {
        lerpTime += Time.deltaTime * speed;
        transform.rotation = CalculateMovementOfPendulum();
    }

    private Quaternion CalculateMovementOfPendulum()
    {
        return Quaternion.Lerp(Quaternion.Euler(Vector3.forward * angle),
            Quaternion.Euler(Vector3.back * angle), GetLerpTParam());
    }

    private float GetLerpTParam()
    {
        return (Mathf.Sin(lerpTime) + 1) * 0.5f;
    }
}
