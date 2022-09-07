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
        this.dialogues = GetDialoguesByClearOrNot(situationNo: situationNo);
        this.playerPoint = GetComponentInChildren<InteractionZone>().transform;

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
        // 게임 클리어 했을 때 위치가 바뀌는 경우 respawnPoint로 이동한다.
        if(respawnPoint != null &&
           SystemManager.instance.isClearGame)
        {
            this.transform.position = respawnPoint.position;
            this.transform.rotation = respawnPoint.rotation;
        }
    }

    private DialogueCollection GetDialoguesByClearOrNot(int situationNo)
    {
        // npc 타입을 통해 npc 이름을 가져온다.
        // npc 이름을 통해 npc Index 를 가져온다.
        // npc Index 를 통해 다이얼로그들을 가져온다.
        if (ConvertManager.TryConvertNpcTypeToName(type: this.type, name: out string _name) &&
            SystemManager.instance.TryGetNpcIndexByName(name: _name, out int _index) &&
            SystemManager.instance.TryGetDialoguesByNpcIndex(index: _index, dialogues: out DialogueCollection _dialolgues))
        {
            // situationNo 에 맞는 다이얼로그를 찾는다. 없다면 모든 다이얼로그를 반환한다.
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
