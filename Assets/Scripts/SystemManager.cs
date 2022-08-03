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
                text: "당신은 모르겠지만 하늘이 닿힌 것이 마냥 나쁜 것은 아니야.."
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "이것은 테스트"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Question,
                text: "골라라"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Option,
                text: "선택지 A"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 1,
                type: DialogueType.Option,
                text: "선택지 B"
            ));

            dialogueSequenceNo++;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Reaction,
                text: "A를 선택했구만"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 1,
                type: DialogueType.Reaction,
                text: "B를 선택했군"
            ));

            dialogueSequenceNo++;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 0,
                sequenceNo: dialogueSequenceNo,
                sequenceSubNo: 0,
                type: DialogueType.Reaction,
                text: "이야기는 끝이네."
            ));

            dialogueSequenceNo = 0;

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 1,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "결국 하늘이 열렸어.."
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 1,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "(기쁘지 않아 보인다.)"
            ));

            dialogueRaws.Add(new Dialogue(
                id: dialogueIndex++,
                npcId: npcIndex,
                situationNo: 1,
                sequenceNo: dialogueSequenceNo++,
                sequenceSubNo: 0,
                type: DialogueType.Normal,
                text: "거인들의 왕의 분노로 닿혔던 하늘이 열렸지만.. 그의 분노가 사라졌다고 할 수 없지 않은가.."
            ));


            DialogueCollection dialogueList = new DialogueCollection(dialogueRaws.OrderBy(dialogue => dialogue.situationNo).ThenBy(dialogue => dialogue.sequenceNo).ThenBy(dialogue => dialogue.sequenceSubNo));
            indexDialogueListDictionary.Add(key: npcIndex, value: dialogueList);
        }

    }
}
