using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType type;
    public Transform mesh;
    public int value;
    
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
        mesh.Rotate(axis: Vector3.up, angle: ValueManager.ITEM_ROTATION_SPEED * Time.deltaTime);
    }
}
