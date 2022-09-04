using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType type;
    public Transform mesh;
    public int value;

    public string name { get; private set; }
    
    private void Start()
    {
        SetMaterial();
        SetNameByItemType(type: this.type);
    }

    private void Update()
    {
        this.mesh.Rotate(axis: Vector3.up, angle: ValueManager.ITEM_ROTATION_SPEED * Time.deltaTime);
    }

    private void SetMaterial()
    {
        if (this.type == ItemType.Bead)
        {
            Material material = this.mesh.GetComponentInChildren<Renderer>().material;
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

    private void SetNameByItemType(ItemType type)
    {
        string _name;

        switch (type)
        {
            case ItemType.BackMirror:
                _name = NameManager.ITEM_NAME_BACK_MIRROR;
                break;
            case ItemType.Bead:
                _name = NameManager.ITEM_NAME_BEAD;
                break;
            case ItemType.Heal:
                _name = NameManager.ITEM_NAME_HEAL;
                break;
            case ItemType.Life:
                _name = NameManager.ITEM_NAME_LIFE;
                break;
            case ItemType.Map:
                _name = NameManager.ITEM_NAME_MAP;
                break;
            case ItemType.Key:
                _name = NameManager.ITEM_NAME_KEY;
                break;
            default:
                _name = string.Empty;
                break;
        }

        this.name = _name;
    }
}
