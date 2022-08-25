using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LitJson;
using System.IO;
using System;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance;

    const string USER_JSON_PATH = "/Resources/tb_user_test.json";
    const string NPC_JSON_PATH = "/Resources/tb_npc_test.json";
    const string DIALOGUE_JSON_PATH = "/Resources/tb_dialogue_test.json";
    const string INGAME_ATTRIBUTE_PATH = "/Resources/tb_ingame_attribute_test.json";

    public IngameAttributeCollection ingameAttributes { get; private set; }
    public Dictionary<int, int> lastDialogueIndexDictionary { get; private set; }
    public int dialogueMagicHumanSequenceSubNo { get; private set; }
    public int dialogueMagicFairySequenceSubNo { get; private set; }
    public int dialogueMagicGiantSequenceSubNo { get; private set; }

    Dictionary<string, User> userAccountUserDictionary;
    Dictionary<int, string> npcIndexNameDictionary;
    Dictionary<string, int> npcNameIndexDictionary;
    Dictionary<int, DialogueCollection> npcIndexDialogueListDictionary;

    public User logInedUser { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        userAccountUserDictionary = new Dictionary<string, User>();
        LoadUserData(path: Application.dataPath + USER_JSON_PATH);

        // 딕셔너리 생성
        npcIndexNameDictionary = new Dictionary<int, string>();
        npcNameIndexDictionary = new Dictionary<string, int>();
        npcIndexDialogueListDictionary = new Dictionary<int, DialogueCollection>();

        lastDialogueIndexDictionary = new Dictionary<int, int>();
    }

    public bool TryLogIn(string account, string password)
    {
        if(userAccountUserDictionary.TryGetValue(key: account, value: out User _user) &&
           string.Equals(a: password, b: _user.password))
        {
            logInedUser = _user;
            LoadDataAll();

            return true;
        }

        return false;
    }

    public void ClearDataAll()
    {
        IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();
        Dictionary<int, int> lastDialogueIndexDictionary = new Dictionary<int, int>();
        dialogueMagicHumanSequenceSubNo = 0;
        dialogueMagicFairySequenceSubNo = 0;
        dialogueMagicGiantSequenceSubNo = 0;

        userAccountUserDictionary = new Dictionary<string, User>();
        npcIndexNameDictionary = new Dictionary<int, string>();
        npcNameIndexDictionary = new Dictionary<string, int>();
        npcIndexDialogueListDictionary = new Dictionary<int, DialogueCollection>();

        logInedUser = new User(
            id: -1,
            account: String.Empty,
            password: String.Empty
        );
    }

    public void SaveIngameAttributes(IEnumerable<IngameAttribute> ingameAttributes)
    {
        this.ingameAttributes.Clear();
        this.ingameAttributes.AddRange(ingameAttributes);

        SaveIngameAttributeToJsonData(ingameAttributes: ingameAttributes);
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

    private void LoadDataAll()
    {
        // Json 형식 데이터 로드
        JsonData npcRaws, dialogueRaws, ingameAttributeRaws;
        LoadJsonRawData(
            npcDataPath: Application.dataPath + NPC_JSON_PATH,
            dialogueDataPath: Application.dataPath + DIALOGUE_JSON_PATH,
            ingameAttributePath: Application.dataPath + INGAME_ATTRIBUTE_PATH,
            npcRaws: out npcRaws,
            dialogueRaws: out dialogueRaws,
            ingameAttributeRaws: out ingameAttributeRaws
        );

        // 구조체 형식에 맞게 변환 및 객체 생성
        NpcCollection npcs = ConvertJsonDataToNPC(data: npcRaws);
        foreach (NPC raw in npcs)
        {
            npcNameIndexDictionary.Add(key: raw.name, value: raw.id);
        }

        DialogueCollection dialogues = ConvertJsonDataToDialogue(data: dialogueRaws);
        foreach (IGrouping<int, Dialogue> dialogueGroup in dialogues.GroupBy(dialogue => dialogue.npcId))
        {
            foreach (IGrouping<int, Dialogue> dialogueSituationNoGroup in dialogueGroup.GroupBy(dialogue => dialogue.situationNo))
            {
                foreach (IGrouping<int, Dialogue> dialogueSubNoGroup in dialogueSituationNoGroup.Where(dialogue => dialogue.type != DialogueType.Option).GroupBy(dialogue => dialogue.sequenceSubNo))
                {
                    int lastIndex = dialogueSubNoGroup.OrderByDescending(dialogue => dialogue.sequenceNo).FirstOrDefault().id;

                    if (!lastDialogueIndexDictionary.ContainsKey(lastIndex))
                    {
                        lastDialogueIndexDictionary.Add(lastIndex, lastIndex);
                    }
                }
            }


            npcIndexDialogueListDictionary.Add(key: dialogueGroup.Key, value: new DialogueCollection(dialogueGroup));
        }

        ingameAttributes = ConvertJsonDataToIngameAttribute(data: ingameAttributeRaws, userId: logInedUser.id);
    }

    private void LoadUserData(string path)
    {
        JsonData data = LoadJsonData(path: path);

        User user;
        int _id;
        string _account, _password;

        foreach (JsonData datum in data)
        {
            _id = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLUMN_ID].ToString());
            _account = datum[NameManager.JSON_COLUMN_ACCOUNT].ToString();
            _password = datum[NameManager.JSON_COLUMN_PASSWORD].ToString();

            user = new User(
                id: _id,
                account: _account,
                password: _password
            );

            userAccountUserDictionary.Add(key: _account, value: user);
        }
    }

    private void LoadJsonRawData(
        string npcDataPath, 
        string dialogueDataPath,
        string ingameAttributePath,
        out JsonData npcRaws, 
        out JsonData dialogueRaws,
        out JsonData ingameAttributeRaws
    )
    {
        npcRaws = LoadJsonData(path: npcDataPath);
        dialogueRaws = LoadJsonData(path: dialogueDataPath);
        ingameAttributeRaws = LoadJsonData(path: ingameAttributePath);
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
                id: ConvertManager.ConvertStringToInt(input: _id),
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

            int sequenceSubNo;
            DialogueType dialogueType;
            if(this.npcNameIndexDictionary.TryGetValue(key: _npcName, value: out int _npcId))
            {
                sequenceSubNo = ConvertManager.ConvertStringToInt(input: _sequenceSubNo);
                dialogueType = (DialogueType)ConvertManager.ConvertStringToInt(input: _dialogueType);

                raws.Add(new Dialogue(
                    id: ConvertManager.ConvertStringToInt(input: _id),
                    npcId: _npcId,
                    situationNo: ConvertManager.ConvertStringToInt(input: _situationNo),
                    sequenceNo: ConvertManager.ConvertStringToInt(input: _sequenceNo),
                    sequenceSubNo: sequenceSubNo,
                    type: dialogueType,
                    text: _text
                ));

                // 특정 다이얼로그의 id 를 저장하기 위함.
                if (dialogueType == DialogueType.Event)
                {
                    if (_text.Contains(NameManager.DIALOGUE_KEYWORD_MAGIC_FAIRY))
                    {
                        dialogueMagicFairySequenceSubNo = sequenceSubNo;
                    }
                    else if (_text.Contains(NameManager.DIALOGUE_KEYWORD_MAGIC_GIANT))
                    {
                        dialogueMagicGiantSequenceSubNo = sequenceSubNo;
                    }
                    else if (_text.Contains(NameManager.DIALOGUE_KEYWORD_MAGIC_HUMAN))
                    {
                        dialogueMagicHumanSequenceSubNo = sequenceSubNo;
                    }
                }
            }
        }

        return raws;
    }

    private IngameAttributeCollection ConvertJsonDataToIngameAttribute(JsonData data, int userId)
    {
        IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();

        int index = 0;
        foreach(JsonData datum in data)
        {
            int _userId = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLIMN_USER_ID].ToString());

            if(userId == _userId)
            {
                string _attributeName = datum[NameManager.JSON_COLUMN_ATTRIBUTE_NAME].ToString();
                int _value = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLUMN_VALUE].ToString());

                ingameAttributes.Add(new IngameAttribute(
                    id: index++,
                    userId: _userId,
                    attributeName: _attributeName,
                    value: _value
                ));
            }
        }

        return ingameAttributes;
    }
    
    public void SaveIngameAttributeToJsonData(IEnumerable<IngameAttribute> ingameAttributes)
    {
        JsonData data = JsonMapper.ToJson(ingameAttributes);

        File.WriteAllText(path: Application.dataPath + INGAME_ATTRIBUTE_PATH, contents: data.ToString());
    }
}
