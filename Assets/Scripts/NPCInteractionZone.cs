using UnityEngine;

public class NPCInteractionZone : MonoBehaviour
{
    public NpcType type;
    public GameObject NPCObject;
    public GameObject effect;
    public Transform cameraPoint;
    
    public string npcName { get; private set; }
    
    private void Start()
    {
        if(ConvertManager.TryConvertNpcTypeToName(type: type, name: out string nameString))
        {
            this.npcName = nameString;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public void ActivatePostPorcess()
    {
        switch (this.type)
        {
            case NpcType.Goblin:
                this.NPCObject.SetActive(false);
                break;
        }
    }
}
