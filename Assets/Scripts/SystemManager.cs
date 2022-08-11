using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LitJson;
using System.IO;
using System;

public class SystemManager
{
    const string NPC_JSON_PATH = "/Resources/tb_npc_test.json";
    const string DIALOGUE_JSON_PATH = "/Resources/tb_dialogue_test.json";

    Dictionary<int, string> npcIndexNameDictionary;
    Dictionary<string, int> npcNameIndexDictionary;
    Dictionary<int, DialogueCollection> npcIndexDialogueListDictionary;
    
    public SystemManager()
    {
        // 딕셔너리 생성
        npcIndexNameDictionary = new Dictionary<int, string>();
        npcNameIndexDictionary = new Dictionary<string, int>();
        npcIndexDialogueListDictionary = new Dictionary<int, DialogueCollection>();

        // Json 형식 데이터 로드
        JsonData npcRaws, dialogueRaws;
        LoadJsonRawData(
            npcDataPath: Application.dataPath + NPC_JSON_PATH, 
            dialogueDataPath: Application.dataPath + DIALOGUE_JSON_PATH,
            npcRaws: out npcRaws,
            dialogueRaws: out dialogueRaws
        );

        // 구조체 형식에 맞게 변환 및 객체 생성
        NpcCollection npcs = ConvertJsonDataToNPC(data: npcRaws);
        foreach (NPC raw in npcs)
        {
            npcNameIndexDictionary.Add(key: raw.name, value: raw.id);
        }

        DialogueCollection dialogues = ConvertJsonDataToDialogue(data: dialogueRaws);
        foreach(IGrouping<int, Dialogue> dialogueGroup in dialogues.GroupBy(dialogue => dialogue.npcId))
        {
            npcIndexDialogueListDictionary.Add(key: dialogueGroup.Key, value: new DialogueCollection(dialogueGroup));
        }
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

    private void LoadJsonRawData(
        string npcDataPath, 
        string dialogueDataPath,
        out JsonData npcRaws, 
        out JsonData dialogueRaws
    )
    {
        npcRaws = LoadJsonData(path: npcDataPath);
        dialogueRaws = LoadJsonData(path: dialogueDataPath);
    }

    private JsonData LoadJsonData(string path)
    {
        string json = File.ReadAllText(path);

        return JsonMapper.ToObject(json);
    }

    private NpcCollection ConvertJsonDataToNPC(JsonData data)
    {
        NpcCollection raws = new NpcCollection();

        foreach (JsonData datum in data)
        {
            string _id = datum[NameManager.JSON_COLUMN_ID].ToString();
            string _name = datum[NameManager.JSON_COLUMN_NAME].ToString();

            raws.Add(new NPC(
                id: Int32.Parse(_id),
                name: _name
            ));
        }

        return raws;
    }

    private DialogueCollection ConvertJsonDataToDialogue(JsonData data)
    {
        DialogueCollection raws = new DialogueCollection();

        foreach (JsonData datum in data)
        {
            string _id = datum[NameManager.JSON_COLUMN_ID].ToString();
            string _npcName = datum[NameManager.JSON_COLUMN_NPC_NAME].ToString();
            string _situationNo = datum[NameManager.JSON_COLUMN_SITUATION_NO].ToString();
            string _sequenceNo = datum[NameManager.JSON_COLUMN_SEQUENCE_NO].ToString();
            string _sequenceSubNo = datum[NameManager.JSON_COLUMN_SEQUENCE_SUB_NO].ToString();
            string _dialogueType = datum[NameManager.JSON_COLUMN_DIALOGUE_TYPE].ToString();
            string _text = datum[NameManager.JSON_COLUMN_TEXT].ToString();

            if(this.npcNameIndexDictionary.TryGetValue(key: _npcName, value: out int _npcId))
            {
                raws.Add(new Dialogue(
                    id: Int32.Parse(_id),
                    npcId: _npcId,
                    situationNo: Int32.Parse(_situationNo),
                    sequenceNo: Int32.Parse(_sequenceNo),
                    sequenceSubNo: Int32.Parse(_sequenceSubNo),
                    type: (DialogueType)Int32.Parse(_dialogueType),
                    text: _text
                ));
            }
        }

        return raws;
    }
}
