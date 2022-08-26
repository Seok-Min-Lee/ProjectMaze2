using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 카메라
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera, minimapCamera;

    // 인게임 UI
    public GameObject normalPanel, interactPanel, gameMenuPanel, guidePanel, gameOverPanel;   // 평상시, 상호작용, 게임메뉴, 가이드, 게임오버
    public GameObject interactableAlram;
    public GameObject interactChoicePanel, nextDialogueSignal;  // 선택지, 다음 표시
    public GameObject[] npcChoiceButtons;   // 선택지 각 버튼
    public Text[] npcChoiceTexts;   // 선택지 버튼 내 텍스트
    public Text npcName, npcDialogue;   // 다이얼로그에 표시되는 NPC 이름과 대사
    public Text guideHead, guideBody;   // 가이드 제목, 설명

    public GameObject minimap, minimapMarker;   // 지도, 플레이어 마커

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;   // HP, 혼란 게이지
    public Text playerLifeText;
    public GameObject magicFairyImage, magicGiantImage, magicHumanImage, poisonImage, confusionImage;
    public Text magicGiantStackText, poisonStackText;
    public GameObject[] playerBeadCovers;

    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;
    public Material[] skyboxMaterials;

    // NPC 상호작용 관련

    DialogueCollection dialogueCollection;
    public int dialogueSituationNo; // 테스트 끝나면 private 수정.
    int dialogueSequenceNo, dialogueSequenceSubNo;
    NPCInteractionZone interactNpc;

    // 미니맵 관련
    Vector3 minimapMarkerPoint, minimapCameraPoint; // 플레이어 마커, 카메라 위치
    bool minimapVisible;

    string currentSceneName;
    bool isPause, isDisplayGuide, isDisplayGameMenu, isDisplayGameOver;

    Dictionary<GuideType, GuideType> displayedGuideTypeDictionary;

    float skyboxRotation;
    //int userId;

    private void Awake()
    {
        displayedGuideTypeDictionary = new Dictionary<GuideType, GuideType>();

        SetIngameAttributes();
    }

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        if (TryGetMinimapAttributesBySceneName(sceneName: this.currentSceneName, index: out int minimapIndex))
        {
            minimapVisible = this.attributeIsActivePlayerMinimaps[minimapIndex];
        }

        ActivateMinimap(isActive: minimapVisible);
        UpdateUIActivedBeads(isActives: this.attributeIsActivePlayerBeads);
        ActivateSkyboxByPlayerBeads(isActiveBeads: this.attributeIsActivePlayerBeads);
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            DefaultUIUpadate();
            UpdateMinimap();

            skyboxRotation += Time.deltaTime;
            RenderSettings.skybox.SetFloat(ValueManager.PROPERY_SKYBOX_ROTATION, skyboxRotation);
        }
    }

    public void UpdateUINormalToInteraction(NPCInteractionZone npc)
    {
        interactNpc = npc;

        // UI 세팅.
        UpdateUIWhetherInteraction(isInteract: true);

        // 상호작용 관련 데이터 초기화.
        InitializeInteractionData(npc: interactNpc);
    }

    public void UpdateUIWhetherInteraction(bool isInteract)
    {
        // 활성화된 카메라 교체.
        followCamera.gameObject.SetActive(!isInteract);
        npcInteractionCamera.gameObject.SetActive(isInteract);

        // 플레이어 조작 설정.
        player._input.controlEnable = !isInteract;
        Cursor.lockState = isInteract ? CursorLockMode.Confined : CursorLockMode.Locked;

        // 활성화된 UI 교체.
        normalPanel.SetActive(!isInteract);
        interactPanel.SetActive(isInteract);
    }

    public void UpdateInteractionUI(out bool isContinuable)
    {
        Dialogue dialogue;
        bool isLast;

        if (TryGetNextDialogue(dialogue: out dialogue))
        {
            npcDialogue.text = dialogue.text;
            isLast = SystemManager.instance.lastDialogueIndexDictionary.ContainsKey(dialogue.id);

            if (dialogue.type == DialogueType.Question)
            {
                DialogueCollection options = new DialogueCollection(dialogueCollection.Where(x => x.type == DialogueType.Option && x.sequenceNo == this.dialogueSequenceNo));

                // 선택지 활성화
                for (int i = 0; i < options.Count; i++)
                {
                    npcChoiceButtons[i].SetActive(true);
                    npcChoiceTexts[i].text = options[i].text;
                }

                nextDialogueSignal.SetActive(false);

                // 상호작용 입력 비활성화
                player._input.interactEnable = false;
            }
            else
            {
                // 선택지 비활성화
                foreach (GameObject button in npcChoiceButtons)
                {
                    if (button.activeSelf)
                    {
                        button.SetActive(false);
                    }
                }

                // 다음 스크립트 여부 표시.
                nextDialogueSignal.SetActive(!isLast);

                // 이벤트 타입인 경우 마지막 다이얼로그에서 해당 이벤트 발생
                if(isLast)
                {
                    OccurDialogueEvent(dialogue: dialogue);
                }

                // 상호작용 입력 활성화
                player._input.interactEnable = true;
            }

            dialogueSequenceNo++;

            isContinuable = !isLast;
        }
        else
        {
            isContinuable = false;
        }
    }

    public void DisplayGameMenu()
    {
        if (!this.isDisplayGameMenu)
        {
            if (!this.isDisplayGuide)
            {
                player.StopPlayerMotion();
                SwitchPauseAndCursorLockEvent(panel: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
            }
            else
            {
                OnClickGuideOKButton();
            }
        }
    }

    public void DisplayGuideByGuideType(GuideType guideType)
    {
        Guide guide;
        if (!this.isDisplayGuide && 
            !displayedGuideTypeDictionary.ContainsKey(guideType) &&
            SystemManager.instance.guideTypeGuideDictionary.TryGetValue(key: guideType, value: out guide))
        {
            player.StopPlayerMotion();
            SwitchPauseAndCursorLockEvent(panel: ref this.guidePanel, isActive: ref this.isDisplayGuide);

            guideHead.text = guide.title;
            guideBody.text = guide.description;

            displayedGuideTypeDictionary.Add(key: guideType, value: guideType);
        }
    }

    public void OnClickInteractionOption(int choiceNo)
    {
        dialogueSequenceSubNo = choiceNo;

        // 선택지 선택이 상호작용 키 입력을 대체하고
        // Player 클래스에 작성된 상호작용에 대한 로직을 따라가기 위해 아래와 같이 처리함.
        player._input.interact = true;
        player.Interact();
    }

    public void OnClickGameMenuOKButton()
    {
        SwitchPauseAndCursorLockEvent(panel: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);

        // 설정 값 저장 - 추가할 것
    }

    public void OnClickGameMenuCancelButton()
    {
        SwitchPauseAndCursorLockEvent(panel: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
    }

    public void OnClickGameMenuLobbyButton()
    {
        SwitchPauseAndCursorLockEvent(panel: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        Cursor.lockState = CursorLockMode.Confined;

        SaveCurrentIngameAttributes();
        SystemManager.instance.ClearDataExclusiveUsers();

        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_LOBBY);
    }

    public void OnClickGameMenuQuitButton()
    {
        SwitchPauseAndCursorLockEvent(panel: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);

        // 설정 값 저장 - 추가할 것
        SaveCurrentIngameAttributes();

        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void OnClickGuideOKButton()
    {
        SwitchPauseAndCursorLockEvent(panel: ref this.guidePanel, isActive: ref this.isDisplayGuide);
    }

    public void OnClickGameOverReTryButton()
    {
        isDisplayGameOver = false;

        gameOverPanel.SetActive(isDisplayGameOver);

        player._input.controlEnable = !isDisplayGameOver;
        Cursor.lockState = isDisplayGameOver ? CursorLockMode.Confined : CursorLockMode.Locked;

        SystemManager.instance.ingameAttributes.Clear();
        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
    }

    public void ActivateSkyboxByPlayerBeads(bool[] isActiveBeads)
    {
        int materialIndex = 0;

        if(isActiveBeads.Length == 3 && isActiveBeads[0])
        {
            if (isActiveBeads[1])
            {
                if (isActiveBeads[2])
                {
                    materialIndex = 3;
                }
                else
                {
                    materialIndex = 2;
                }
            }
            else
            {
                materialIndex = 1;
            }
        }

        RenderSettings.skybox = skyboxMaterials[materialIndex];
    }

    public void ActivateMinimap(bool isActive)
    {
        if(minimapCamera != null)
        {
            minimapVisible = isActive;
            minimap.SetActive(minimapVisible);
            minimapCamera.SetActive(minimapVisible);

            switch (currentSceneName)
            {
                case NameManager.SCENE_STAGE_1:
                    attributeIsActivePlayerMinimaps[0] = true;
                    break;
                case NameManager.SCENE_STAGE_2:
                    attributeIsActivePlayerMinimaps[1] = true;
                    break;
                case NameManager.SCENE_STAGE_3:
                    attributeIsActivePlayerMinimaps[2] = true;
                    break;
            }
        }
    }

    public void ActivateTrafficLight(bool isActivate)
    {
        trafficLightPanel.SetActive(isActivate);
    }

    public void UpdateTrafficLightByType(TrapTrafficLightType type)
    {
        for (int i = 0; i < trafficLights.Length; i++)
        {
            if (i == (int)type)
            {
                trafficLights[i].SetActive(true);
            }
            else
            {
                trafficLights[i].SetActive(false);
            }
        }
    }

    public void UpdateUIActivedBeads(bool[] isActives)
    {
        for(int i=0; i< playerBeadCovers.Count(); i++)
        {
            playerBeadCovers[i].SetActive(!isActives[i]);
        }
    }

    public void LoadSceneBySceneType(SceneType sceneType)
    {
        string sceneName = ConvertManager.ConvertSceneTypeToString(sceneType: sceneType);

        // Player Attribute DB 데이터 업데이트 추가.
        SaveCurrentIngameAttributes();

        LoadingSceneManager.LoadScene(sceneName: sceneName);
    }

    public void MoveGameObject(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.position = vector;
    }

    private void InitializeInteractionData(NPCInteractionZone npc)
    {
        if (SystemManager.instance.GetNpcIndexByName(name: npc.npcName, index: out int npcIndex) &&
            SystemManager.instance.GetDialoguesByNpcIndex(index: npcIndex, dialogueCollection: out DialogueCollection dialogues))
        {
            this.dialogueSequenceNo = 0;
            npcName.text = npc.npcName;

            DialogueCollection _dialogueCollection = new DialogueCollection(dialogues.Where(dialogue => dialogue.situationNo == this.dialogueSituationNo));

            if (npc.type == NpcType.Goblin)
            {
                this.dialogueSequenceSubNo = GetGoblinInteracionDialogueSequenceSubNo(dialogues: _dialogueCollection);

                dialogueCollection = new DialogueCollection(_dialogueCollection.Where(dialogue => dialogue.sequenceSubNo == this.dialogueSequenceSubNo));
            }
            else
            {
                this.dialogueSequenceSubNo = 0;
                dialogueCollection = _dialogueCollection;
            }
        }
    }

    private int GetGoblinInteracionDialogueSequenceSubNo(IEnumerable<Dialogue> dialogues)
    {
        // 다이럴로그 타입을 결정한다.
        float randomTypeValue = UnityEngine.Random.Range(minInclusive: 0, maxExclusive:10);
        DialogueType dialogueType = randomTypeValue > 4 ? DialogueType.Normal : DialogueType.Event;

        // 다이얼로그 SequenceSubNo 의 최대 최소값 구한다.
        DialogueCollection _dialogueCollection;
        int sequenceSubNo;

        if (dialogueType == DialogueType.Event)
        {
            if (player.isActiveMagicHuman)
            {
                if (player.isActiveMagicFairy)
                {
                    sequenceSubNo = SystemManager.instance.dialogueMagicGiantSequenceSubNo;
                }
                else
                {
                    if (UnityEngine.Random.Range(minInclusive: 0, maxExclusive: 2) > 0)
                    {
                        sequenceSubNo = SystemManager.instance.dialogueMagicFairySequenceSubNo;
                    }
                    else
                    {
                        sequenceSubNo = SystemManager.instance.dialogueMagicGiantSequenceSubNo;
                    }
                }
            }
            else
            {
                sequenceSubNo = SystemManager.instance.dialogueMagicHumanSequenceSubNo;
            }
        }
        else
        {
            _dialogueCollection = new DialogueCollection(dialogues.Where(dialogue => dialogue.type == dialogueType).OrderBy(dialogue => dialogue.sequenceSubNo));

            int randomMinValue = _dialogueCollection.FirstOrDefault().sequenceSubNo;
            int randomMaxValue = _dialogueCollection.LastOrDefault().sequenceSubNo;

            sequenceSubNo = (int)UnityEngine.Random.Range(minInclusive: randomMinValue, maxExclusive: randomMaxValue + 1);
        }

        return sequenceSubNo;
    }

    private bool TryGetNextDialogue(out Dialogue dialogue)
    {
        DialogueCollection dialogues = new DialogueCollection(dialogueCollection.Where(dialogue => dialogue.sequenceNo == this.dialogueSequenceNo));

        if (dialogues.Count > 0)
        {
            dialogue = dialogues.Where(dialogue => dialogue.sequenceSubNo == dialogueSequenceSubNo).FirstOrDefault();

            if(!string.IsNullOrEmpty(dialogue.text))
            {
                return true;
            }

            return false;
        }
        else
        {
            dialogue = new Dialogue();

            return false;
        }
    }

    private void OccurDialogueEvent(Dialogue dialogue)
    {
        if(dialogue.type == DialogueType.Event)
        {
            if(dialogue.sequenceSubNo == SystemManager.instance.dialogueMagicFairySequenceSubNo)
            {
                player.ActivateMagicFairy(isActive: true);
            }
            else if(dialogue.sequenceSubNo == SystemManager.instance.dialogueMagicGiantSequenceSubNo)
            {
                player.ActivateMagicGiant(isActive: true);
            }
            else if(dialogue.sequenceSubNo == SystemManager.instance.dialogueMagicHumanSequenceSubNo)
            {
                player.ActivateMagicHuman(isActive: true);
            }
        }
    }

    private void DefaultUIUpadate()
    {
        // 체력 및 공포
        playerLifeText.text = ValueManager.PREFIX_PLAYER_LIFE + player.currentLife.ToString();
        playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
        playerConfusionBar.localScale = new Vector3((float)player.confusionStack / player.confusionStackMax, 1, 1);

        // 마법 효과
        bool isActiveMagicGiant = player.magicGiantStack > 0;
        bool isActivePoision = player.poisonStack > 0;

        magicFairyImage.SetActive(player.isActiveMagicFairy);
        magicGiantImage.SetActive(isActiveMagicGiant);
        magicHumanImage.SetActive(player.isActiveMagicHuman);
        poisonImage.SetActive(isActivePoision);
        confusionImage.SetActive(player.isConfusion);

        magicGiantStackText.text = ValueManager.PREFIX_PLAYER_EFFECT_STACK + player.magicGiantStack.ToString();
        poisonStackText.text = ValueManager.PREFIX_PLAYER_EFFECT_STACK + player.poisonStack.ToString();

        // 상호작용 가능한 경우
        interactableAlram.SetActive(player.isInteractPreprocessReady ? true : false);

        // 게임오버
        if (player.currentLife <= 0 && player.currentHp <= 0)
        {
            isDisplayGameOver = true;

            gameOverPanel.SetActive(isDisplayGameOver);

            player._input.controlEnable = !isDisplayGameOver;
            Cursor.lockState = isDisplayGameOver ? CursorLockMode.Confined : CursorLockMode.Locked;
        }
    }

    private void UpdateMinimap()
    {
        if (minimapVisible && minimapCamera != null)
        {
            minimapMarkerPoint = player.transform.position;
            minimapMarkerPoint.y = 0;

            minimapMarker.transform.position = minimapMarkerPoint;

            minimapCameraPoint = minimapMarkerPoint;
            minimapCameraPoint.y = minimapCamera.transform.position.y;
            minimapCamera.transform.position = minimapCameraPoint;
        }
    }

    private bool TryGetMinimapAttributesBySceneName(string sceneName, out int index)
    {
        // index 플레이어의 인게임 속성에서 미니맵에 대한 값이 배열이기 때문에 원하는 값을 찾기 위한 인덱스
        // isVisible 현재 씬에 미니맵의 존재 여부

        bool isVisible;

        switch (sceneName)
        {
            case NameManager.SCENE_STAGE_1:
                index = 0;
                isVisible = true;
                break;
            case NameManager.SCENE_STAGE_2:
                index = 1;
                isVisible = true;
                break;
            case NameManager.SCENE_STAGE_3:
                index = 2;
                isVisible = true;
                break;
            default:
                index = -1;
                isVisible = false;
                break;
        }

        return isVisible;
    }

    private void SwitchPauseAndCursorLockEvent(ref GameObject panel, ref bool isActive)
    {
        isActive = !isActive;

        SwitchPause();
        panel.SetActive(isActive);

        player._input.controlEnable = !isActive;
        Cursor.lockState = isActive ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    private void SwitchPause()
    {
        if (!isPause)
        {
            Time.timeScale = ValueManager.TIME_SCALE_PASUE;
            isPause = true;
        }
        else
        {
            Time.timeScale = ValueManager.TIME_SCALE_PLAY;
            isPause = false;
        }
    }

    // 아래 변수 및 함수들은 데이터를 읽고 저장할 때에만 사용하기를 권장.
    bool[] attributeIsActivePlayerBeads;
    bool[] attributeIsActivePlayerMinimaps;
    bool attributeIsDisplayGuide;
    bool attributeIsActivePlayerMagicFairy, attributeIsActivePlayerMagicHuman;
    int attributePlayerLife, attributePlayerCurrentHp, attributePlayerCurrentConfusion, attributePlayerMagicGiantStack, attributePlayerPoisonStack;

    public void SetPlayerIngameAttributes(
        out bool[] isActiveBeads,
        out bool isActiveMinimap,
        out bool isActiveMagicFairy,
        out bool isActiveMagicHuman,
        out int life,
        out int currentHp,
        out int currentConfusion,
        out int magicGiantStack,
        out int poisonStack
    )
    {
        switch (currentSceneName)
        {
            case NameManager.SCENE_STAGE_1:
                isActiveMinimap = this.attributeIsActivePlayerMinimaps[0];
                break;
            case NameManager.SCENE_STAGE_2:
                isActiveMinimap = this.attributeIsActivePlayerMinimaps[1];
                break;
            case NameManager.SCENE_STAGE_3:
                isActiveMinimap = this.attributeIsActivePlayerMinimaps[2];
                break;
            default:
                isActiveMinimap = false;
                break;
        }

        isActiveBeads = this.attributeIsActivePlayerBeads;
        isActiveMagicFairy = this.attributeIsActivePlayerMagicFairy;
        isActiveMagicHuman = this.attributeIsActivePlayerMagicHuman;
        life = this.attributePlayerLife;
        currentHp = this.attributePlayerCurrentHp;
        currentConfusion = this.attributePlayerCurrentConfusion;
        magicGiantStack = this.attributePlayerMagicGiantStack;
        poisonStack = this.attributePlayerPoisonStack;
    }

    public void UpdatePlayerIngameAttributes(
        bool[] isActiveBeads,
        bool isActiveMinimap,
        bool isActiveMagicFairy,
        bool isActiveMagicHuman,
        int life,
        int currentHp,
        int currentConfusion,
        int magicGiantStack,
        int poisonStack
    )
    {
        switch (currentSceneName)
        {
            case NameManager.SCENE_STAGE_1:
                this.attributeIsActivePlayerMinimaps[0] = isActiveMinimap;
                break;
            case NameManager.SCENE_STAGE_2:
                this.attributeIsActivePlayerMinimaps[1] = isActiveMinimap;
                break;
            case NameManager.SCENE_STAGE_3:
                this.attributeIsActivePlayerMinimaps[2] = isActiveMinimap;
                break;
        }

        this.attributeIsActivePlayerBeads = isActiveBeads;
        this.attributeIsActivePlayerMagicFairy = isActiveMagicFairy;
        this.attributeIsActivePlayerMagicHuman = isActiveMagicHuman;
        this.attributePlayerLife = life;
        this.attributePlayerCurrentHp = currentHp;
        this.attributePlayerCurrentConfusion = currentConfusion;
        this.attributePlayerMagicGiantStack = magicGiantStack;
        this.attributePlayerPoisonStack = poisonStack;
    }

    private void SaveCurrentIngameAttributes()
    {
        // 플레이어 속성 값 호출
        this.player.CallUpdatePlayerIngameAttributes();

        // 타입 변환 후 교체 및 저장.
        IngameAttributeCollection ingameAttributes = ConvertPropertyToIngameAttribute();
        SystemManager.instance.SaveIngameAttributes(ingameAttributes: ingameAttributes);
    }

    private void SetIngameAttributes()
    {
        attributeIsActivePlayerBeads = new bool[3];
        attributeIsActivePlayerMinimaps = new bool[3];

        SetIngameAttributesDefault();
        SetIngameAttributesBySavedData(ingameAttributes: SystemManager.instance.ingameAttributes);
    }

    private void SetIngameAttributesDefault()
    {
        for(int i = 0; i < attributeIsActivePlayerBeads.Length; i++)
        {
            attributeIsActivePlayerBeads[i] = false;
        }
        for (int i = 0; i < attributeIsActivePlayerMinimaps.Length; i++)
        {
            attributeIsActivePlayerMinimaps[i] = false;
        }

        this.attributeIsDisplayGuide = true;
        this.attributeIsActivePlayerMagicFairy = false;
        this.attributeIsActivePlayerMagicHuman = false;

        this.attributePlayerLife = 1;
        this.attributePlayerCurrentHp = 100;
        this.attributePlayerCurrentConfusion = 0;
        this.attributePlayerMagicGiantStack = 0;
        this.attributePlayerPoisonStack = 0;

    }
    private void SetIngameAttributesBySavedData(IEnumerable<IngameAttribute> ingameAttributes)
    {
        if(ingameAttributes.Count() > 0)
        {
            foreach (IngameAttribute attribute in ingameAttributes)
            {
                switch (attribute.attributeName)
                {
                    case NameManager.INGAME_ATTRIBUTE_NAME_BEAD_1:
                        attributeIsActivePlayerBeads[0] = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_BEAD_2:
                        attributeIsActivePlayerBeads[1] = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_BEAD_3:
                        attributeIsActivePlayerBeads[2] = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_LIFE:
                        attributePlayerLife = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_HP:
                        attributePlayerCurrentHp = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_CONFUSION:
                        attributePlayerCurrentConfusion = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_FAIRY:
                        attributeIsActivePlayerMagicFairy = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_GIANT:
                        attributePlayerMagicGiantStack = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_HUMAN:
                        attributeIsActivePlayerMagicHuman = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_POISON_STACK:
                        attributePlayerPoisonStack = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_1:
                        attributeIsActivePlayerMinimaps[0] = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_2:
                        attributeIsActivePlayerMinimaps[1] = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_3:
                        attributeIsActivePlayerMinimaps[2] = attribute.value == 0 ? false : true;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_DISPLAY_GUIDE:
                        attributeIsDisplayGuide = attribute.value == 0 ? false : true;
                        break;
                }
            }
        }
    }

    private IngameAttributeCollection ConvertPropertyToIngameAttribute()
    {
        IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();

        int index = 0;
        int userId = SystemManager.instance.logInedUser.id;

        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_BEAD_1,
            value: this.attributeIsActivePlayerBeads[0] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_BEAD_2,
            value: this.attributeIsActivePlayerBeads[1] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_BEAD_3,
            value: this.attributeIsActivePlayerBeads[2] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_1,
            value: this.attributeIsActivePlayerMinimaps[0] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_2,
            value: this.attributeIsActivePlayerMinimaps[1] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_3,
            value: this.attributeIsActivePlayerMinimaps[2] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_FAIRY,
            value: this.attributeIsActivePlayerMagicFairy == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_GIANT,
            value: this.attributePlayerMagicGiantStack
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_HUMAN,
            value: this.attributeIsActivePlayerMagicHuman == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_LIFE,
            value: this.attributePlayerLife
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_HP,
            value: this.attributePlayerCurrentHp
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_CONFUSION,
            value: this.attributePlayerCurrentConfusion
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_POISON_STACK,
            value: this.attributePlayerPoisonStack
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            userId: userId,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_DISPLAY_GUIDE,
            value: this.attributeIsDisplayGuide == true ? 1 : 0
        ));

        return ingameAttributes;
    }
}
