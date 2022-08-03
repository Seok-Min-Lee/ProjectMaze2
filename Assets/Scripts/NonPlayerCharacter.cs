using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    public NpcType type;
    public GameObject effect;
    public string name { get; private set; }
    
    private void Start()
    {
        if(NameManager.TryConvertNpcTypeToName(type: type, name: out string nameString))
        {
            name = nameString;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
