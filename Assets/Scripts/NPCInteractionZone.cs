using UnityEngine;

public class NpcInteractionZone : InteractionZone
{
    public NpcObject npcObject;
        
    private void Start()
    {
        this.interactionType = InteractionType.Diaglogue;
    }

    public void ActivatePostPorcess()
    {
        switch (this.npcObject.type)
        {
            case NpcType.Goblin:
                this.npcObject.gameObject.SetActive(false);
                break;
        }
    }
}
