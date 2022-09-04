using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using LitJson;

public class SystemManager : MonoBehaviour
{
    public static SystemManager instance;

    public IngameAttributeCollection ingameAttributes { get; private set; }
    public IngamePreferenceCollection ingamePreferences { get; private set; }
    public Dictionary<int, int> lastDialogueIndexDictionary { get; private set; }
    public Dictionary<int, Npc> npcIndexNpcDictionary { get; private set; }
    public Dictionary<GuideType, Guide> guideTypeGuideDictionary { get; private set; }
    public int dialogueMagicHumanCaseNo { get; private set; }
    public int dialogueMagicFairyCaseNo { get; private set; }
    public int dialogueMagicGiantCaseNo { get; private set; }
    public int dialogueEntranceCaseNo { get; private set; }
    public bool isClearGame { get; private set; }

    public User logInedUser { get; private set; }

    private IngameAttributeCollection ingameAttributeExlusiveLoginedUsers;
    private Dictionary<string, User> userAccountUserDictionary;
    private Dictionary<int, string> npcIndexNameDictionary;
    private Dictionary<string, int> npcNameIndexDictionary;
    private Dictionary<int, DialogueCollection> npcIndexDialogueListDictionary;

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

        // 유저 데이터 로드
        this.userAccountUserDictionary = new Dictionary<string, User>();
        LoadUserData(path: Application.dataPath + ValueManager.JSON_PATH_TB_USER);

        // 환경 설정 데이터 로드
        ingamePreferences = new IngamePreferenceCollection();
        LoadIngamePreference(path: Application.dataPath + ValueManager.JSON_PATH_TB_INGAME_PREFERENCE);
    }

    private void Start()
    {
        this.ingameAttributeExlusiveLoginedUsers = new IngameAttributeCollection();

        this.npcIndexNameDictionary = new Dictionary<int, string>();
        this.npcNameIndexDictionary = new Dictionary<string, int>();
        this.npcIndexNpcDictionary = new Dictionary<int, Npc>();

        this.lastDialogueIndexDictionary = new Dictionary<int, int>();
        this.npcIndexDialogueListDictionary = new Dictionary<int, DialogueCollection>();

        this.guideTypeGuideDictionary = new Dictionary<GuideType, Guide>();
    }

    public void SignUpUser(string account, string password)
    {
        UserCollection users = new UserCollection();
        users.AddRange(this.userAccountUserDictionary.Values);

        int index = 0;
        foreach(User user in users)
        {
            if(user.id > index)
            {
                index = user.id;
            }
        }
        index++;

        User _user = new User(
            id: index,
            account: account,
            password: password
        );

        users.Add(_user);
        this.userAccountUserDictionary.Add(key: _user.account, value: _user);

        SaveUserToJsonData(users: users);
    }

    public bool IsExistAccount(string account)
    {
        return this.userAccountUserDictionary.ContainsKey(account);
    }

    public bool TryLogIn(string account, string password)
    {
        if(this.userAccountUserDictionary.TryGetValue(key: account, value: out User _user) &&
           string.Equals(a: password, b: _user.password))
        {
            this.logInedUser = _user;
            LoadDataAll();

            return true;
        }

        return false;
    }

    public void DeleteIngameIngameAttributeData()
    {
        this.isClearGame = false;
        this.ingameAttributes.Clear();
    }

    public void DeleteDataExclusiveUsers()
    {
        this.ingameAttributes = new IngameAttributeCollection();
        this.lastDialogueIndexDictionary = new Dictionary<int, int>();
        this.dialogueMagicHumanCaseNo = 0;
        this.dialogueMagicFairyCaseNo = 0;
        this.dialogueMagicGiantCaseNo = 0;
        this.dialogueEntranceCaseNo = 0;

        this.npcIndexNameDictionary = new Dictionary<int, string>();
        this.npcNameIndexDictionary = new Dictionary<string, int>();
        this.npcIndexNpcDictionary = new Dictionary<int, Npc>();
        this.npcIndexDialogueListDictionary = new Dictionary<int, DialogueCollection>();
        this.guideTypeGuideDictionary = new Dictionary<GuideType, Guide>();

        this.logInedUser = new User(
            id: 0,
            account: String.Empty,
            password: String.Empty
        );
    }

    public void SaveIngameAttributes(IEnumerable<IngameAttribute> ingameAttributes)
    {
        this.ingameAttributes.Clear();
        this.ingameAttributes.AddRange(ingameAttributes);
        this.isClearGame = DetectClearGameByIngameAttributes(this.ingameAttributes);

        // JSON 데이터를 일부 수정 없이 일괄 처리하기 때문에 다른 유저들의 데이터를 한번에 넘긴다.
        IngameAttributeCollection ingameAttributeAll = new IngameAttributeCollection();
        ingameAttributeAll.AddRange(this.ingameAttributes);
        ingameAttributeAll.AddRange(this.ingameAttributeExlusiveLoginedUsers);

        SaveIngameAttributeToJsonData(ingameAttributes: ingameAttributeAll);
    }

    public void SaveIngamePreferences(IEnumerable<IngamePreference> preferences)
    {
        this.ingamePreferences.Clear();
        this.ingamePreferences.AddRange(preferences);

        SaveIngamePreferenceToJsonData(preferences: this.ingamePreferences);
    }

    public bool TryGetNpcIndexByName(string name, out int index)
    {
        if (this.npcNameIndexDictionary.TryGetValue(key: name, value: out index))
        {
            return true;
        }

        return false;
    }

    public bool TryGetNpcNameByIndex(int index, out string name)
    {
        if (this.npcIndexNameDictionary.TryGetValue(key: index, out name))
        {
            return true;
        }

        return false;
    }

    public bool TryGetDialoguesByNpcIndex(int index, out DialogueCollection dialogues)
    {
        if (this.npcIndexDialogueListDictionary.TryGetValue(key: index, out dialogues))
        {
            return true;
        }

        return false;
    }

    private void LoadDataAll()
    {
        // Json 형식 데이터 로드
        JsonData npcRaws, dialogueRaws, ingameAttributeRaws, guideRaws;
        LoadJsonRawData(
            npcDataPath: Application.dataPath + ValueManager.JSON_PATH_TB_NPC,
            dialogueDataPath: Application.dataPath + ValueManager.JSON_PATH_TB_DIALOGUE,
            ingameAttributePath: Application.dataPath + ValueManager.JSON_PATH_TB_INGAME_ATTRIBUTE,
            guidePath: Application.dataPath + ValueManager.JSON_PATH_TB_GUIDE,
            npcRaws: out npcRaws,
            dialogueRaws: out dialogueRaws,
            ingameAttributeRaws: out ingameAttributeRaws,
            guideRaws: out guideRaws
        );

        // 구조체 형식에 맞게 변환 및 객체 생성
        NpcDataCollection npcs = ConvertJsonDataToNPC(data: npcRaws);
        foreach (Npc npc in npcs)
        {
            string npcName = npc.name;
            if(!this.npcNameIndexDictionary.ContainsKey(key: npcName))
            {
                this.npcNameIndexDictionary.Add(key: npcName, value: npc.id);
            }

            if(!this.npcIndexNpcDictionary.ContainsKey(key: npc.id))
            {
                this.npcIndexNpcDictionary.Add(key: npc.id, value: npc);
            }
        }

        DialogueCollection dialogues = ConvertJsonDataToDialogue(data: dialogueRaws);
        foreach (IGrouping<int, Dialogue> dialogueGroup in dialogues.GroupBy(dialogue => dialogue.npcId))
        {
            foreach (IGrouping<int, Dialogue> dialogueSituationNoGroup in dialogueGroup.GroupBy(dialogue => dialogue.situationNo))
            {
                foreach (IGrouping<int, Dialogue> dialogueSubNoGroup in dialogueSituationNoGroup.Where(dialogue => dialogue.type != DialogueType.Option).GroupBy(dialogue => dialogue.caseNo))
                {
                    int lastIndex = dialogueSubNoGroup.OrderByDescending(dialogue => dialogue.sequenceNo).FirstOrDefault().id;

                    if (!this.lastDialogueIndexDictionary.ContainsKey(lastIndex))
                    {
                        this.lastDialogueIndexDictionary.Add(lastIndex, lastIndex);
                    }
                }
            }


            this.npcIndexDialogueListDictionary.Add(key: dialogueGroup.Key, value: new DialogueCollection(dialogueGroup));
        }

        this.ingameAttributeExlusiveLoginedUsers.Clear();
        this.ingameAttributes = ConvertJsonDataToIngameAttribute(data: ingameAttributeRaws, userId: logInedUser.id);
        this.isClearGame = DetectClearGameByIngameAttributes(attributes: this.ingameAttributes);

        GuideCollection guides = ConvertJsonDataToGuide(data: guideRaws);
        foreach(Guide guide in guides)
        {
            GuideType guideType = guide.type;

            if(!this.guideTypeGuideDictionary.ContainsKey(key: guideType))
            {
                this.guideTypeGuideDictionary.Add(key: guideType, value: guide);
            }
        }
    }

    private bool DetectClearGameByIngameAttributes(IEnumerable<IngameAttribute> attributes)
    {
        foreach(IngameAttribute attribute in attributes.Where(attribute => attribute.attributeName.Contains(NameManager.INGAME_ATTRIBUTE_NAME_BEAD)))
        {
            if(attribute.value != 1)
            {
                return false;
            }
        }

        return true;
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

            this.userAccountUserDictionary.Add(key: _account, value: user);
        }
    }

    private void LoadIngamePreference(string path)
    {
        JsonData data = LoadJsonData(path: path);

        IngamePreference preference;
        int _id;
        string _name, _value;

        foreach(JsonData datum in data)
        {
            _id = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLUMN_ID].ToString());
            _name = datum[NameManager.JSON_COLUMN_NAME].ToString();
            _value = datum[NameManager.JSON_COLUMN_VALUE].ToString();

            ingamePreferences.Add(new IngamePreference(
                id: _id,
                name: _name,
                value: _value
            ));
        }
    }

    private void LoadJsonRawData(
        string npcDataPath, 
        string dialogueDataPath,
        string ingameAttributePath,
        string guidePath,
        out JsonData npcRaws, 
        out JsonData dialogueRaws,
        out JsonData ingameAttributeRaws,
        out JsonData guideRaws
    )
    {
        npcRaws = LoadJsonData(path: npcDataPath);
        dialogueRaws = LoadJsonData(path: dialogueDataPath);
        ingameAttributeRaws = LoadJsonData(path: ingameAttributePath);
        guideRaws = LoadJsonData(path: guidePath);
    }

    private JsonData LoadJsonData(string path)
    {
        string json = File.ReadAllText(path);

        return JsonMapper.ToObject(json);
    }

    private NpcDataCollection ConvertJsonDataToNPC(JsonData data)
    {
        NpcDataCollection raws = new NpcDataCollection();

        foreach (JsonData datum in data)
        {
            string _id = datum[NameManager.JSON_COLUMN_ID].ToString();
            string _name = datum[NameManager.JSON_COLUMN_NAME].ToString();

            raws.Add(new Npc(
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
            string _caseNo = datum[NameManager.JSON_COLUMN_CASE_NO].ToString();
            string _sequenceNo = datum[NameManager.JSON_COLUMN_SEQUENCE_NO].ToString();
            string _dialogueType = datum[NameManager.JSON_COLUMN_DIALOGUE_TYPE].ToString();
            string _text = datum[NameManager.JSON_COLUMN_TEXT].ToString();

            int caseNo;
            DialogueType dialogueType;
            if(this.npcNameIndexDictionary.TryGetValue(key: _npcName, value: out int _npcId))
            {
                caseNo = ConvertManager.ConvertStringToInt(input: _caseNo);
                dialogueType = (DialogueType)ConvertManager.ConvertStringToInt(input: _dialogueType);

                raws.Add(new Dialogue(
                    id: ConvertManager.ConvertStringToInt(input: _id),
                    npcId: _npcId,
                    situationNo: ConvertManager.ConvertStringToInt(input: _situationNo),
                    caseNo: caseNo,
                    sequenceNo: ConvertManager.ConvertStringToInt(input: _sequenceNo),
                    type: dialogueType,
                    text: _text
                ));

                // 특정 다이얼로그의 id 를 저장하기 위함.
                switch (_npcName)
                {
                    case NameManager.NPC_NAME_GOBLIN:
                        if (dialogueType == DialogueType.Event)
                        {
                            if (_text.Contains(NameManager.DIALOGUE_KEYWORD_MAGIC_FAIRY))
                            {
                                this.dialogueMagicFairyCaseNo = caseNo;
                            }
                            else if (_text.Contains(NameManager.DIALOGUE_KEYWORD_MAGIC_GIANT))
                            {
                                this.dialogueMagicGiantCaseNo = caseNo;
                            }
                            else if (_text.Contains(NameManager.DIALOGUE_KEYWORD_MAGIC_HUMAN))
                            {
                                this.dialogueMagicHumanCaseNo = caseNo;
                            }
                        }
                        break;

                    case NameManager.NPC_NAME_HUMAN_GATEKEEPER:
                        if (_text.Contains(NameManager.DIALOGUE_KEYWORD_ENTRANCE))
                        {
                            this.dialogueEntranceCaseNo = caseNo;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        return raws;
    }

    private IngameAttributeCollection ConvertJsonDataToIngameAttribute(JsonData data, int userId)
    {
        IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();

        foreach(JsonData datum in data)
        {
            int _userId = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLIMN_USER_ID].ToString());
            int _id = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLUMN_ID].ToString());
            int _value = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLUMN_VALUE].ToString());
            string _attributeName = datum[NameManager.JSON_COLUMN_ATTRIBUTE_NAME].ToString();

            IngameAttribute attribute = new IngameAttribute(
                id: _id,
                userId: _userId,
                attributeName: _attributeName,
                value: _value
            );

            if (userId == _userId)
            {
                ingameAttributes.Add(attribute);
            }
            else
            {
                this.ingameAttributeExlusiveLoginedUsers.Add(attribute);
            }
        }

        return ingameAttributes;
    }

    private GuideCollection ConvertJsonDataToGuide(JsonData data)
    {
        GuideCollection guides = new GuideCollection();

        foreach (JsonData datum in data)
        {
            int _id = ConvertManager.ConvertStringToInt(datum[NameManager.JSON_COLUMN_ID].ToString());
            GuideType _type = ConvertManager.ConvertStringToGuideType(datum[NameManager.JSON_COLUMN_GUIDE_TYPE].ToString());
            string _title = datum[NameManager.JSON_COLUMN_TITLE].ToString();
            string _description = datum[NameManager.JSON_COLUMN_DESCRIPTION].ToString();

            guides.Add(new Guide(
                id: _id,
                type: _type,
                title: _title,
                description: _description
            ));
        }

        return guides;
    }

    private void SaveUserToJsonData(IEnumerable<User> users)
    {
        JsonData data = JsonMapper.ToJson(users);

        File.WriteAllText(path: Application.dataPath + ValueManager.JSON_PATH_TB_USER, contents: data.ToString());
    }
    
    private void SaveIngameAttributeToJsonData(IEnumerable<IngameAttribute> ingameAttributes)
    {
        JsonData data = JsonMapper.ToJson(ingameAttributes);

        File.WriteAllText(path: Application.dataPath + ValueManager.JSON_PATH_TB_INGAME_ATTRIBUTE, contents: data.ToString());
    }

    private void SaveIngamePreferenceToJsonData(IEnumerable<IngamePreference> preferences)
    {
        JsonData data = JsonMapper.ToJson(preferences);

        File.WriteAllText(path: Application.dataPath + ValueManager.JSON_PATH_TB_INGAME_PREFERENCE, contents: data.ToString());
    }
}
