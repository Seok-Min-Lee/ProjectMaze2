using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcObject : MonoBehaviour
{ 
    public NpcType type;
    public Transform cameraPoint;
    public Transform respawnPoint;

    public Npc npc { get; private set; }
    public Transform playerPoint { get; private set; }
    public DialogueCollection dialogues { get; private set; }

    private void Start()
    {
        if(TryConvertNpcTypeToNPC(type: this.type, npcData: out Npc _npc))
        {
            this.npc = _npc;
        }

        int situationNo = SystemManager.instance.isClearGame ? 1 : 0;
        dialogues = GetDialoguesByClearOrNot(situationNo: situationNo);

        playerPoint = GetComponentInChildren<InteractionZone>().transform;

        SetTransformByClearOrNot();
    }

    public void InteractionPostProcess(Player player)
    {
        switch (type)
        {
            case NpcType.Goblin:
                this.gameObject.SetActive(false);
                player.OnTriggerExitFromInteractionZone();
                break;
            default:
                break;
        }
    }

    private void SetTransformByClearOrNot()
    {
        if(respawnPoint != null &&
           SystemManager.instance.isClearGame)
        {
            this.transform.position = respawnPoint.position;
            this.transform.rotation = respawnPoint.rotation;
        }
    }

    private DialogueCollection GetDialoguesByClearOrNot(int situationNo)
    {
        if (ConvertManager.TryConvertNpcTypeToName(type: this.type, name: out string _name) &&
            SystemManager.instance.TryGetNpcIndexByName(name: _name, out int _index) &&
            SystemManager.instance.TryGetDialoguesByNpcIndex(index: _index, dialogues: out DialogueCollection _dialolgues))
        {
            DialogueCollection _dialoguesBySituationNo = new DialogueCollection(_dialolgues.Where(dialogue => dialogue.situationNo == situationNo));
            
            if(_dialoguesBySituationNo.Count > 0)
            {
                return _dialoguesBySituationNo;
            }
            else
            {
                return _dialolgues;
            }
        }

        return new DialogueCollection();
    }

    private bool TryConvertNpcTypeToNPC(NpcType type, out Npc npcData)
    {
        int _index;
        string _name;

        if(ConvertManager.TryConvertNpcTypeToName(type: this.type, name: out _name) &&
           SystemManager.instance.TryGetNpcIndexByName(name: _name, out _index) &&
           SystemManager.instance.npcIndexNpcDictionary.TryGetValue(key: _index, value: out npcData))
        {
            return true;
        }

        npcData = new Npc();
        return false;
    }
}
