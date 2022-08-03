using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SystemManager
{
    List<string> npcNames;
    Dictionary<int, string> npcIndexNameDictionary;
    Dictionary<string, int> npcNameIndexDictionary;
    Dictionary<int, DialogueCollection> npcIndexDialogueListDictionary;

    public SystemManager()
    {
        InitializeNpc(
            names: out npcNames, 
            indexNameDictionary: out npcIndexNameDictionary, 
            nameIndexDictionary: out npcNameIndexDictionary
        );

        InitializeDialogue(indexDialogueListDictionary: out npcIndexDialogueListDictionary);
    }

    public bool GetNpcIndexByName(string name, out int index)
    {
        if (npcNameIndexDictionary.TryGetValue(key: name, value: out index))
        {
            return true;
        }

        return false;
    }

    public bool GetNpcNameByIndex(int index, out string name)
    {
        if (npcIndexNameDictionary.TryGetValue(key: index, out name))
        {
            return true;
        }

        return false;
    }

    public bool GetDialoguesByNpcIndex(int index, out DialogueCollection dialogueCollection)
    {
        if (npcIndexDialogueListDictionary.TryGetValue(key: index, out dialogueCollection))
        {
            return true;
        }

        return false;
    }

    private void InitializeNpc(out List<string> names, out Dictionary<int, string> indexNameDictionary, out Dictionary<string, int> nameIndexDictionary)
    {
        string[] nameList = {
            NameManager.NPC_NAME_FAIRY_CHILD,
            NameManager.NPC_NAME_FAIRY_OLD_MAN,
            NameManager.NPC_NAME_GIANT_STONE_STATUE,
            NameManager.NPC_NAME_GIANT_TWIN_A,
            NameManager.NPC_NAME_GIANT_TWIN_B,
            NameManager.NPC_NAME_HUMAN_GATEKEEPER,
            NameManager.NPC_NAME_HUMAN_OLD_MAN,
            NameManager.NPC_NAME_HUMAN_YOUNG_MAN,
            NameManager.NPC_NAME_NONAME
        };

        names = new List<string>();
        names.AddRange(nameList);


        indexNameDictionary = new Dictionary<int, string>();
        nameIndexDictionary = new Dictionary<string, int>();
        for (int i = 0; i < names.Count; i++)
        {
            indexNameDictionary.Add(i, npcNames[i]);
            nameIndexDictionary.Add(npcNames[i], i);
        }
    }

    private void InitializeDialogue(out Dictionary<int, DialogueCollection> indexDialogueListDictionary)
    {
        indexDialogueListDictionary = new Dictionary<int, DialogueCollection>();
        int dialogueIndex = 0;
        int npcIndex;
        Dialogue dialogue;

        if (GetNpcIndexByName(name: NameManager.NPC_NAME_HUMAN_OLD_MAN, index: out npcIndex))
        {
            DialogueCollection dialogueRaws = new DialogueCollection();
            int dialogueSequenceNo = 0;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "����� �𸣰����� �ϴ��� ���� ���� ���� ���� ���� �ƴϾ�.."
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "�̰��� �׽�Ʈ"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Question,
                text: "����"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Option,
                text: "������ A"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 1,
                type: DialogueType.Option,
                text: "������ B"
            ));

            dialogueSequenceNo++;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Reaction,
                text: "A�� �����߱���"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 1,
                type: DialogueType.Reaction,
                text: "B�� �����߱�"
            ));

            dialogueSequenceNo++;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Reaction,
                text: "�̾߱�� ���̳�."
            ));

            dialogueSequenceNo = 0;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 1,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "�ᱹ �ϴ��� ���Ⱦ�.."
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 1,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "(����� �ʾ� ���δ�.)"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 1,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "���ε��� ���� �г�� ������ �ϴ��� ��������.. ���� �г밡 ������ٰ� �� �� ���� ������.."
            ));


            DialogueCollection dialogueList = new DialogueCollection(dialogueRaws.OrderBy(dialogue => dialogue.situationNo).ThenBy(dialogue => dialogue.sequenceNo).ThenBy(dialogue => dialogue.sequenceSubNo));
            indexDialogueListDictionary.Add(key: npcIndex, value: dialogueList);
        }

    }
}
