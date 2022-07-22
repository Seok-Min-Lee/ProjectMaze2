using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapMisteryWall : Trap
{
    public override void ActivateEvent()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
