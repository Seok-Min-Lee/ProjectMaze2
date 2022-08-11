using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRotation : MonoBehaviour
{
    public float rotateSpeed;

    void Update()
    {
        transform.Rotate(axis: Vector3.up, angle: rotateSpeed * Time.deltaTime);
    }
}
