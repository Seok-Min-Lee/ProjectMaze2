using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType type;
    public int value;

    public Transform mesh;

    float rotateSpeed = 60f;

    private void Start()
    {
        if(type == ItemType.Bead)
        {
            Color color = Color.white;

            switch (value)
            {
                case 0:
                    color = Color.red;
                    break;
                case 1:
                    color = Color.green;
                    break;
                case 2:
                    color = Color.blue;
                    break;
            }

            mesh.GetComponentInChildren<Renderer>().material.color = color;
        }
    }

    private void Update()
    {
        mesh.Rotate(axis: Vector3.up, angle: rotateSpeed * Time.deltaTime);
    }
}
