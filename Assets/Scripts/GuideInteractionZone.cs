using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideInteractionZone : InteractionZone
{
    public GuideType guideType;

    void Start()
    {
        this.interactionType = InteractionType.Guide;    
    }
}
