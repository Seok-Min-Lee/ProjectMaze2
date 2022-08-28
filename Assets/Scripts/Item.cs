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
        if(this.type == ItemType.Bead)
        {
            Material material = mesh.GetComponentInChildren<Renderer>().material;
            float alpha = material.color.a;

            Color color = Color.white;

            switch (value)
            {
                case 0:
                    color = new Color(1, 0, 0, alpha);
                    break;
                case 1:
                    color = new Color(0, 1, 0, alpha);
                    break;
                case 2:
                    color = new Color(0, 0, 1, alpha);
                    break;
            }

            material.color = color;
        }
    }

    private void Update()
    {
        mesh.Rotate(axis: Vector3.up, angle: ValueManager.ITEM_ROTATION_SPEED * Time.deltaTime);
    }
}
