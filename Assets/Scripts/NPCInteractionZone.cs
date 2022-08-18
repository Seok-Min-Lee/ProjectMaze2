using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractionZone : MonoBehaviour
{
    public NpcType type;
    public GameObject effect;
    public string npcName { get; private set; }
    
    private void Start()
    {
        if(ConvertManager.TryConvertNpcTypeToName(type: type, name: out string nameString))
        {
            npcName = nameString;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
