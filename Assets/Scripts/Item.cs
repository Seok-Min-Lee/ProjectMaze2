using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType type;
    public int value;

    float rotateSpeed = 0.3f;

    void Update()
    {
        transform.Rotate(axis: Vector3.up, angle: rotateSpeed);
    }
}
