using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractionZone : MonoBehaviour
{
    public NpcType type;
    public GameObject NPCObject;
    public GameObject effect;
    public Transform cameraPoint;
    public bool isVolatility { get; private set; }
    public string npcName { get; private set; }
    
    private void Start()
    {
        if(ConvertManager.TryConvertNpcTypeToName(type: type, name: out string nameString))
        {
            npcName = nameString;
            SetVolatilityByNpcType();
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public void DisappearByVolatility()
    {
        if (isVolatility)
        {
            NPCObject.SetActive(false);
        }    
    }

    private void SetVolatilityByNpcType()
    {
        switch (this.type)
        {
            case NpcType.Goblin:
                this.isVolatility = true;
                break;
            default:
                this.isVolatility = false;
                break;
        }
    }
}
