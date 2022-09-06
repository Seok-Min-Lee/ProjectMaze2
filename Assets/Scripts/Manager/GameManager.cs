using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // 카메라
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera, minimapCamera;

    // UI
    public GameObject normalPanel, interactPanel, gameMenuPanel, guidePanel, gameOverPanel;   // UI 패널들

    // Normal Panel
    public GameObject minimap, minimapMarker;   // 지도, 플레이어 마커
    public GameObject backMirror;               // 백 미러
    public GameObject interactableAlram;        // 상호작용 가능 표시
    public Text interactableAlramText;
    public Text confirmMessageText;

    public GameObject[] playerBeadCovers;
    public GameObject magicFairyImage, magicGiantImage, magicHumanImage, poisonImage, confusionImage;   // 특수 효과 활성화 표시
    public RectTransform playerHpBar, playerConfusionBar;   // 플레이어 HP, 혼란 게이지
    public Text playerLifeText;     // 플레이어 남은 라이프 표시
    public Text playerKeyText;
    public Text magicGiantStackText, poisonStackText;   // 특수효과 스택 표시
    
    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;

    // Interact Panel   
    public GameObject interactChoicePanel, nextDialogueSignal;  // 선택지, 다음 표시
    public GameObject[] npcChoiceButtons;   // 선택지 각 버튼
    public Text[] npcChoiceTexts;   // 선택지 버튼 내 텍스트
    public Text npcName, npcDialogue;   // 다이얼로그에 표시되는 NPC 이름과 대사

    // Game Menu Panel
    public Toggle displayGuideToggle, displayBackMirrorToggle;
    public Slider bgmSlider, seSlider;

    // Guide Panel
    public Text guideHead, guideBody;   // 가이드 제목, 설명

    public Material[] skyboxMaterials;
    public AudioMixer masterMixer;
    
    public string currentSceneName { get; private set; }
    
    private Player player;
    private bool isPause, isDisplayNormal, isDisplayGuide, isDisplayGameMenu, isDisplayGameOver, isDisplayInteract;

    // 미니맵 관련
    private Vector3 minimapMarkerPoint, minimapCameraPoint; // 플레이어 마커, 카메라 위치
    private bool minimapVisible;

    // NPC 상호작용 관련
    private DialogueCollection dialogues;
    private VillageObjectManager villageObjectManager;
    private NpcObject interactNpcObject;
    public int situationNo { get; private set; }
    private int dialogueSequenceNo, dialogueCaseNo;

    private float skyboxRotation;

    private bool isLatestPlayerInteractable, isLatestPlayerInteractWithNpc, isLatestPlayerInputInteract, isLatestPlayerInputEscape;

    private void Awake()
    {
        // Attribute, Preference 초기화
        SetIngameAttributeProperties();
        SetIngamePreferenceProperties();
    }

    private void Start()
    {
        // 프로퍼티 초기화
        this.player = GameObject.FindGameObjectWithTag(tag: NameManager.TAG_PLAYER).GetComponent<Player>();

        this.currentSceneName = SceneManager.GetActiveScene().name;
        this.situationNo = SystemManager.instance.isClearGame ? 1 : 0;
        this.isDisplayNormal = normalPanel.activeSelf;

        this.confirmMessageText.text = string.Empty;

        // preferences 값 적용
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );

        SetSceneObjectManagerBySceneName(sceneName: this.currentSceneName, manager: ref villageObjectManager);

        UpdateByPlayerActiveBeads(isActives: this.attributeIsActivePlayerBeads);
        SetMinimapVisibleByMinimapAttributes(attributes: this.attributeIsActivePlayerMinimaps);
        ActivateMinimap(isActive: minimapVisible);
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            UpdateSkyBox();

            UpdateNormalPanel();
            UpdateMinimap();

            DisplayInteraction();
            DisplayGameOver();
            DisplayGameMenu();
        }
    }

    public void DisplayConfirmMessage(string text, EventMessageType type)
    {
        StopAllCoroutines();
        StartCoroutine(AlertMessage(text: text, type: type, target: this.confirmMessageText));
    }

    IEnumerator AlertMessage(
        string text,
        EventMessageType type,
        Text target = null
    )
    {
        if (target != null)
        {
            target.color = GetColorByEventMessageType(type: type);
            target.text = text;
            
            yield return new WaitForSeconds(ValueManager.MESSAGE_DISPLAY_DURATION);

            target.text = string.Empty;
        }
    }

    private Color GetColorByEventMessageType(EventMessageType type)
    {
        Color color;

        switch (type)
        {
            case EventMessageType.Item:
                color = Color.yellow;
                break;
            case EventMessageType.TrapSuccess:
                color = new Color(0.2f, 1, 0, 1);   // Green
                break;
            case EventMessageType.TrapFailure:
                color = new Color(1, 0.2f, 0, 1);   // Orange
                break;
            case EventMessageType.Debuff:
                color = new Color(1, 0, 1, 1);  // Magenta
                break;
            case EventMessageType.Recovery:
                color = Color.green;
                break;
            case EventMessageType.Error:
                color = Color.red;
                break;
            default:
                color = Color.red;
                break;
        }

        return color;
    }

    public void OnClickInteractionOption(int choiceNo)
    {
        this.dialogueCaseNo = choiceNo;

        // 선택지 비활성화
        foreach (GameObject button in this.npcChoiceButtons)
        {
            if (button.activeSelf)
            {
                button.SetActive(false);
            }
        }

        // 비활성화 되어있는 interactEnable 활성화 시킨다.
        // 키 입력이 들어온 것으로 처리한다. 
        player._input.interactEnable = true;
        player._input.interact = false;
        TryContinueInteraction();
    }

    public void OnClickGameMenuOKButton()
    {
        // UI 및 부가적인 세팅 업데이트
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // 설정한 Preference 를 현재 변수에 업데이트.
        this.preferenceGuideVisible = displayGuideToggle.isOn;
        this.preferenceBackMirrorVisible = displayBackMirrorToggle.isOn;
        this.preferenceBgmVolume = bgmSlider.value;
        this.preferenceSeVolume = seSlider.value;

        // Preference 적용
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );

        // 타입 변환 후 교체 및 저장.
        IngamePreferenceCollection preferences = ConvertPropertyToIngamePreferences();
        SystemManager.instance.SaveIngamePreferences(preferences: preferences);
    }

    public void OnClickGameMenuCancelButton()
    {
        // UI 및 부가적인 세팅 업데이트
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // 기존 Preference 로 롤백
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );
    }

    public void OnClickGameMenuLobbyButton()
    {
        // UI 및 부가적인 세팅 업데이트
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // 기존 환경 값으로 롤백
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );

        // Attribute 저장
        SaveCurrentIngameAttributes(isSavePosition: true);
        SystemManager.instance.DeleteDataExceptUsers();

        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_LOBBY);
    }

    public void OnClickGameMenuQuitButton()
    {
        // UI 및 부가적인 세팅 업데이트
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // 기존 환경 값으로 롤백
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );

        SaveCurrentIngameAttributes(isSavePosition: true);

        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void OnClickGuideOKButton()
    {
        // UI 및 부가적인 세팅 업데이트
        ReverseObjectSetActive(obj: ref this.guidePanel, isActive: ref this.isDisplayGuide);
        UpdateExtraSettingAll();
    }

    public void OnClickGameOverReTryButton()
    {
        // UI 및 부가적인 세팅 업데이트
        ReverseObjectSetActive(obj: ref this.gameOverPanel, isActive: ref this.isDisplayGameOver);
        UpdateExtraSettingAll();

        // 가지고 있던 Attribute 초기화.
        SystemManager.instance.ingameAttributes.Clear();
        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
    }

    public void OnValueChangedBgmSlider()
    {
        float volume = bgmSlider.value;

        if(volume <= ValueManager.INGAME_PREFERENCE_BGM_VOLUME_MIN)
        {
            volume = ValueManager.INGAME_PREFERENCE_BGM_VOLUME_MUTE;
        }

        masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_BGM, volume);
    }

    public void OnValueChangedSeSlider()
    {
        float volume = seSlider.value;

        if (volume <= ValueManager.INGAME_PREFERENCE_SE_VOLUME_MIN)
        {
            volume = ValueManager.INGAME_PREFERENCE_SE_VOLUME_MUTE;
        }

        masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_EFFECT, volume);
    }


    public void ActivateMinimap(bool isActive)
    {
        minimapVisible = isActive;
        minimap.SetActive(minimapVisible);

        if (minimapVisible &&
           minimapCamera != null)
        {
            minimapCamera.SetActive(minimapVisible);

            switch (this.currentSceneName)
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

    public void UpdateByPlayerActiveBeads(bool[] isActives)
    {
        UpdateBeadCoversByPlayerActivedBeads(isActives: isActives);
        ChangeSkyboxByPlayerBeads(isActives: isActives);
    }

    public void LoadSceneBySceneType(SceneType sceneType)
    {
        string sceneName = ConvertManager.ConvertSceneTypeToString(sceneType: sceneType);

        if(this.currentSceneName == NameManager.SCENE_TUTORIAL)
        {
            // 튜토리얼인 경우 튜토리얼 클리어 여부만 저장한다.
            IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();
            ingameAttributes.Add(new IngameAttribute(
                id: 0,
                userId: SystemManager.instance.logInedUser.id,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_TUTORIAL_CLEAR,
                value: 1
            ));
            SystemManager.instance.SaveIngameAttributes(ingameAttributes: ingameAttributes);
            
            // 로비로 가기전에 SystemManager 데이터를 지운다.
            SystemManager.instance.DeleteDataExceptUsers();
        }
        else
        {
            // Player Attribute DB 데이터 업데이트 추가.
            SaveCurrentIngameAttributes(isSavePosition: false);
        }
        LoadingSceneManager.LoadScene(sceneName: sceneName);
    }

    public void MoveGameObject(GameObject obj, Vector3 vector)
    {
        obj.transform.position = vector;
    }

    private void SetSceneObjectManagerBySceneName(string sceneName, ref VillageObjectManager manager)
    {
        // ObjectManager 로 통일하고 지울 것.
        switch (sceneName)
        {
            case NameManager.SCENE_VILLAGE:
                manager = GameObject.Find(NameManager.NAME_VILLAGE_OBJECT_MANAGER).GetComponent<VillageObjectManager>();
                break;
            case NameManager.SCENE_STAGE_1:
                break;
            case NameManager.SCENE_STAGE_2:
                break;
            case NameManager.SCENE_STAGE_3:
                break;
            default:
                break;
        }
    }

    #region ##### 게임 환경 관련 #####
    private void ChangeSkyboxByPlayerBeads(bool[] isActives)
    {
        int materialIndex = 0;

        if (isActives.Length == 3 && isActives[0])
        {
            if (isActives[1])
            {
                if (isActives[2])
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
    #endregion

    private void UpdateBeadCoversByPlayerActivedBeads(bool[] isActives)
    {
        // Bead UI는 구슬 이미지 위에 커버 이미지가 덮고 있다.
        // 획득한 구슬이 있으면 덮고 있는 커버 이미지를 비활성화 시킨다.
        for (int i = 0; i < playerBeadCovers.Count(); i++)
        {
            playerBeadCovers[i].SetActive(!isActives[i]);
        }
    }

    private void UpdateInteractableAlram()
    {
        if (this.isLatestPlayerInteractable != player.isInteractable &&
            !this.isDisplayGameMenu &&
            !this.isDisplayGuide &&
            !this.isDisplayGameOver &&
            !player.isInteract)
        {
            this.isLatestPlayerInteractable = player.isInteractable;
            this.interactableAlram.SetActive(this.isLatestPlayerInteractable);

            UpdateInteractableAlramTextByInteractionType(type: player.interactableInteractionType);
        }
    }

    private void UpdateInteractableAlramTextByInteractionType(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.Diaglogue:
                this.interactableAlramText.text = ValueManager.INTERACTABLE_ALRAM_TEXT_DIALOGUE;
                break;
            case InteractionType.Guide:
                this.interactableAlramText.text = ValueManager.INTERACTABLE_ALRAM_TEXT_GUIDE;
                break;
        }
    }

    private void SetMinimapVisibleByMinimapAttributes(bool[] attributes)
    {
        if (TryGetMinimapAttributeIndexBySceneName(sceneName: this.currentSceneName, index: out int minimapIndex))
        {
            this.minimapVisible = attributes[minimapIndex];
        }
    }

    private bool TryGetMinimapAttributeIndexBySceneName(string sceneName, out int index)
    {
        // index : 플레이어의 인게임 속성에서 미니맵에 대한 값이 배열이기 때문에 원하는 값을 찾기 위한 인덱스
        // isVisible : 현재 씬에 미니맵의 존재 여부

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

    private void SetIngamePreferences(
        bool backMirrorVisible,
        float bgmVolume,
        float seVolume
    )
    {
        // 가이드 표시는 ObjectManager 클래스에서 관리한다.

        this.masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_BGM, bgmVolume);
        this.masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_EFFECT, seVolume);
        this.backMirror.SetActive(backMirrorVisible);
    }

    private void UpdateSkyBox()
    {
        this.skyboxRotation += Time.deltaTime;
        RenderSettings.skybox.SetFloat(ValueManager.PROPERY_SKYBOX_ROTATION, skyboxRotation);
    }

    private void UpdateNormalPanel()
    {
        if (this.normalPanel.activeSelf)
        {
            // 체력 및 공포
            playerLifeText.text = ValueManager.PREFIX_PLAYER_LIFE + player.currentLife.ToString();
            playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
            playerConfusionBar.localScale = new Vector3((float)player.confusionStack / player.confusionStackMax, 1, 1);

            playerKeyText.text = ValueManager.PREFIX_PLAYER_KEY + player.currentKey.ToString();

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
            //interactableAlram.SetActive(player.isInteractPreprocessReady ? true : false);
            UpdateInteractableAlram();
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

    private void DisplayGameOver()
    {
        if (!isDisplayGameOver && 
            !player.isTutorial &&
            player.currentLife <= 0 && 
            player.currentHp <= 0)
        {
            isDisplayGameOver = true;

            gameOverPanel.SetActive(isDisplayGameOver);

            UpdatePlayerMovable();
            UpdateCursorVisibility();
        }
    }

    private void DisplayInteraction()
    {
        // 키 입력 확인
        if (!this.isLatestPlayerInputInteract && player._input.interact)
        {
            // 상호작용 가능 여부 확인
            if (player.isInteractable)
            {
                switch (player.interactableInteractionType)
                {
                    case InteractionType.Diaglogue:
                        DisplayInteractionWithNpc();
                        break;
                    case InteractionType.Guide:
                        DisplayGuideByGuideType(player.interactableGuideType);
                        break;
                }
            }

            player._input.interact = false;
        }

        this.isLatestPlayerInputInteract = player._input.interact;
    }

    private void DisplayGameMenu()
    {
        if (!this.isLatestPlayerInputEscape && player._input.escape)
        {
            if (!this.isDisplayGameMenu)
            {
                if (!this.isDisplayGuide)
                {
                    this.displayGuideToggle.isOn = this.preferenceGuideVisible;
                    this.displayBackMirrorToggle.isOn = this.preferenceBackMirrorVisible;
                    this.bgmSlider.value = this.preferenceBgmVolume;
                    this.seSlider.value = this.preferenceSeVolume;

                    player.InputStop();


                    ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
                    UpdateExtraSettingAll();
                }
                else
                {
                    // 가이드 패널 활성화되어 있는 경우 OK 처리
                    OnClickGuideOKButton();
                }
            }
            else
            {
                // 게임 메뉴 패널 활성화 되어 있는 경우 Cancel 처리
                OnClickGameMenuCancelButton();
            }
        }

        this.isLatestPlayerInputEscape = player._input.escape;
    }

    private void DisplayInteractionWithNpc()
    {
        if (this.isLatestPlayerInteractWithNpc && player.isInteract)
        {
            // 다음 다이얼로그가 없는 경우 상호작용을 마친다.
            if (!TryContinueInteraction())
            {
                ReverseInteractionSetting();
                this.interactNpcObject.InteractionPostProcess(player: player);

                this.isLatestPlayerInteractWithNpc = false;
            }
        }
        else if(!this.isLatestPlayerInteractWithNpc && player.isInteract)
        {
            this.interactNpcObject = player.interactableNpcObject;

            ReverseInteractionSetting();        // UI 세팅
            InitInteractionWithNpcSettings();   // Dialogue 세팅
            TryContinueInteraction();           // 상호작용 시작

            this.npcName.text = interactNpcObject.npc.name;

            this.isLatestPlayerInteractWithNpc = player.isInteract;
        }
    }

    public void DisplayGuideByGuideType(GuideType guideType)
    {
        Guide guide;

        if (!this.isDisplayGuide)
        {
            if(SystemManager.instance.guideTypeGuideDictionary.TryGetValue(key: guideType, value: out guide))
            {
                player.InputStop();

                ReverseObjectSetActive(obj: ref this.guidePanel, isActive: ref this.isDisplayGuide);
                UpdateExtraSettingAll();

                this.guideHead.text = guide.title;
                this.guideBody.text = guide.description;
            }
        }
        else
        {
            ReverseObjectSetActive(obj: ref this.guidePanel, isActive: ref this.isDisplayGuide);
            UpdateExtraSettingAll();
        }
    }

    private bool TryContinueInteraction()
    {
        Dialogue dialogue;
        bool isLast;

        if(this.interactNpcObject != null &&
           TryGetNextDialogue(dialogue: out dialogue))
        {
            npcDialogue.text = dialogue.text;
            isLast = SystemManager.instance.lastDialogueIndexDictionary.ContainsKey(dialogue.id);

            if(dialogue.type == DialogueType.Question)
            {
                UpdateDialogueOptions(sequenceNo: this.dialogueSequenceNo);

                nextDialogueSignal.SetActive(false);    // 다음 다이얼로그 표시 off
                player._input.interactEnable = false;   // 옵션 선택을 위해 상호작용 키입력 off
            }
            else
            {
                OccurDialogueEvent(dialogue: dialogue, isLast: isLast);

                this.nextDialogueSignal.SetActive(!isLast); // 다음 다이얼로그 표시 업데이트
            }

            this.dialogueSequenceNo++;

            return true;
        }

        return false;
    }

    private void InitInteractionWithNpcSettings()
    {
        // 다이얼로그 관련 초기화.
        this.dialogues = interactNpcObject.dialogues;

        this.dialogueCaseNo = GetDialougeCaseNo();
        this.dialogueSequenceNo = 0;

        // 플레이어 위치 및 조작 초기화.
        player.ForceToMove(point: interactNpcObject.playerPoint.position);
        player.InputStop();
    }

    private int GetDialougeCaseNo()
    {
        int caseNo;

        if (interactNpcObject.type == NpcType.Goblin)
        {
            caseNo = GetDialogueSampleCaseNoForGoblin();
        }
        else
        {
            caseNo = 0;
        }

        return caseNo;
    }

    private bool TryGetNextDialogue(out Dialogue dialogue)
    {
        DialogueCollection _dialogues = GetNextDialogueSamples();

        if (_dialogues.Count() > 0)
        {
            dialogue = _dialogues.FirstOrDefault();

            return !string.IsNullOrEmpty(dialogue.text);
        }

        dialogue = new Dialogue();
        return false;
    }

    private DialogueCollection GetNextDialogueSamples()
    {
        DialogueCollection _dialogues = new DialogueCollection(
            dialogues.Where(x => x.type != DialogueType.Option &&
                                 x.caseNo == this.dialogueCaseNo &&
                                 x.sequenceNo == this.dialogueSequenceNo));

        return _dialogues;
    }

    private void UpdateDialogueOptions(int sequenceNo)
    {
        DialogueCollection options = new DialogueCollection(
            dialogues.Where(x => x.type == DialogueType.Option && x.sequenceNo == sequenceNo)
        );

        // 선택지 활성화
        if (options.Count <= npcChoiceButtons.Length)
        {
            for (int i = 0; i < options.Count; i++)
            {
                npcChoiceButtons[i].SetActive(true);
                npcChoiceTexts[i].text = options[i].text;
            }
        }
    }

    private void OccurDialogueEvent(Dialogue dialogue, bool isLast)
    {
        if (isLast && dialogue.type == DialogueType.Event)
        {
            if (dialogue.caseNo == SystemManager.instance.dialogueMagicFairyCaseNo)
            {
                player.ActivateMagicFairy(isActive: true);
            }
            else if (dialogue.caseNo == SystemManager.instance.dialogueMagicGiantCaseNo)
            {
                player.ActivateMagicGiant(isActive: true);
            }
            else if (dialogue.caseNo == SystemManager.instance.dialogueMagicHumanCaseNo)
            {
                player.ActivateMagicHuman(isActive: true);
            }
            else if (dialogue.caseNo == SystemManager.instance.dialogueEntranceCaseNo)
            {
                if (villageObjectManager != null)
                {
                    villageObjectManager.OpenPortal();
                }
            }
        }
    }

    private int GetDialogueSampleCaseNoForGoblin()
    {
        // 다이럴로그 타입을 결정한다.
        float randomTypeValue = Random.Range(minInclusive: 0, maxExclusive: 10);
        DialogueType dialogueType = randomTypeValue > 4 ? DialogueType.Normal : DialogueType.Event;

        // 다이얼로그 SequenceSubNo 의 최대 최소값 구한다.
        DialogueCollection _dialogues;
        int caseNo;

        if (dialogueType == DialogueType.Event)
        {
            if (this.player.isActiveMagicHuman)
            {
                if (this.player.isActiveMagicFairy)
                {
                    caseNo = SystemManager.instance.dialogueMagicGiantCaseNo;
                }
                else
                {
                    if (Random.Range(minInclusive: 0, maxExclusive: 2) > 0)
                    {
                        caseNo = SystemManager.instance.dialogueMagicFairyCaseNo;
                    }
                    else
                    {
                        caseNo = SystemManager.instance.dialogueMagicGiantCaseNo;
                    }
                }
            }
            else
            {
                caseNo = SystemManager.instance.dialogueMagicHumanCaseNo;
            }
        }
        else
        {
            _dialogues = new DialogueCollection(dialogues.Where(dialogue => dialogue.type == dialogueType).OrderBy(dialogue => dialogue.caseNo));

            int randomMinValue = _dialogues.FirstOrDefault().caseNo;
            int randomMaxValue = _dialogues.LastOrDefault().caseNo;

            caseNo = (int)Random.Range(minInclusive: randomMinValue, maxExclusive: randomMaxValue + 1);
        }

        return caseNo;
    }

    private void ReverseInteractionSetting()
    {
        // 활성화된 카메라 교체.
        bool isActiveFollowCamera = followCamera.activeSelf;
        bool isActiveInteractionCamera = npcInteractionCamera.activeSelf;
        ReverseObjectSetActive(obj: ref this.followCamera, isActive: ref isActiveFollowCamera);
        ReverseObjectSetActive(obj: ref this.npcInteractionCamera, isActive: ref isActiveInteractionCamera);

        if (this.npcInteractionCamera.activeSelf)
        {
            MoveGameObject(obj: this.npcInteractionCamera, vector: player.interactableNpcObject.cameraPoint.position);
            this.npcInteractionCamera.transform.rotation = player.interactableNpcObject.cameraPoint.rotation;
        }

        // 활성화된 UI 교체.
        ReverseObjectSetActive(obj: ref this.normalPanel, isActive: ref this.isDisplayNormal);
        ReverseObjectSetActive(obj: ref this.interactPanel, isActive: ref this.isDisplayInteract);

        // 부가 설정.
        UpdatePlayerMovable();
        UpdateCursorVisibility();
    }

    private void UpdateExtraSettingAll()
    {
        UpdateTimeScale();
        UpdatePlayerMovable();
        UpdateCursorVisibility();
    }

    private void ReverseObjectSetActive(ref GameObject obj, ref bool isActive)
    {
        isActive = !isActive;
        obj.SetActive(isActive);
    }

    private void UpdateTimeScale()
    {
        if (!this.isPause)
        {
            if (IsNeedPause())
            {
                Time.timeScale = ValueManager.TIME_SCALE_PASUE;
                this.isPause = true;
            }
        }
        else
        {
            Time.timeScale = ValueManager.TIME_SCALE_PLAY;
            this.isPause = false;
        }
    }

    private void UpdateCursorVisibility()
    {
        bool isNeedCursor = IsNeedCursor();

        if(Cursor.lockState == CursorLockMode.Locked)
        {
            if (isNeedCursor)
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
        else if(Cursor.lockState == CursorLockMode.Confined)
        {
            if (!isNeedCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void UpdatePlayerMovable()
    {
        // 현재까지는 커서가 필요한 경우 움직임과 시야의 조작을 제한한다.
        player._input.controlEnable = !IsNeedCursor();
        player._input.cursorInputForLook = !IsNeedCursor();

        player.InputStop();
    }

    private bool IshadBeadAll()
    {
        for (int i = 0; i < this.attributeIsActivePlayerBeads.Length; i++)
        {
            if (this.attributeIsActivePlayerBeads[i] == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsNeedCursor()
    {
        return this.isDisplayGameMenu || this.isDisplayGameOver || this.isDisplayGuide || this.isDisplayInteract;
    }

    private bool IsNeedPause()
    {
        return this.isDisplayGameMenu || this.isDisplayGuide;
    }

    #region ##### 데이터 처리 관련#####
    // 아래 변수 및 함수들은 데이터를 읽고 저장할 때에만 사용하기를 권장.
    private bool[] attributeIsActivePlayerBeads;
    private bool[] attributeIsActivePlayerMinimaps;
    private bool attributeSavedPositionEnabled/*, attributeTutorialClear*/;
    private bool attributeIsActivePlayerMagicFairy, attributeIsActivePlayerMagicHuman;
    private int attributePlayerLife, attributePlayerKey, attributePlayerCurrentHp, attributePlayerCurrentConfusion, attributePlayerMagicGiantStack, attributePlayerPoisonStack;
    private int attributeSavedSceneNumber;
    private int attributeSavedPositionX, attributeSavedPositionY, attributeSavedPositionZ;

    private bool preferenceBackMirrorVisible;
    private float preferenceBgmVolume, preferenceSeVolume;

    public bool preferenceGuideVisible { get; private set; }

    public void SetPlayerIngameAttributes(
        out bool[] isActiveBeads,
        out bool isActiveMinimap,
        out bool isActiveMagicFairy,
        out bool isActiveMagicHuman,
        out bool isTutorialClear,
        out int life,
        out int key,
        out int currentHp,
        out int currentConfusion,
        out int magicGiantStack,
        out int poisonStack,
        out bool savedPositionEnabled,
        out int savedPositionX,
        out int savedPositionY,
        out int savedPositionZ
    )
    {
        isTutorialClear = SystemManager.instance.isClearTutorial;
        if (!isTutorialClear)
        {
            isActiveBeads = new bool[3];
            isActiveMinimap = false;
            isActiveMagicFairy = false;
            isActiveMagicHuman = false;
            life = ValueManager.TUTORIAL_PLAYER_SETTING_CURRENT_LIFE;
            key = 0;
            currentHp = ValueManager.TUTORIAL_PLAYER_SETTING_CURRENT_HP;
            currentConfusion = 0;
            magicGiantStack = 0;
            poisonStack = 0;
            savedPositionEnabled = false;
            savedPositionX = 0;
            savedPositionY = 0;
            savedPositionZ = 0;
        }
        else
        {
            switch (this.currentSceneName)
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
            key = this.attributePlayerKey;
            currentHp = this.attributePlayerCurrentHp;
            currentConfusion = this.attributePlayerCurrentConfusion;
            magicGiantStack = this.attributePlayerMagicGiantStack;
            poisonStack = this.attributePlayerPoisonStack;
            savedPositionEnabled = this.attributeSavedPositionEnabled;
            savedPositionX = this.attributeSavedPositionX;
            savedPositionY = this.attributeSavedPositionY;
            savedPositionZ = this.attributeSavedPositionZ;
        }
    }

    public void UpdatePlayerIngameAttributes(
        bool[] isActiveBeads,
        bool isActiveMinimap,
        bool isActiveMagicFairy,
        bool isActiveMagicHuman,
        int life,
        int key,
        int currentHp,
        int currentConfusion,
        int magicGiantStack,
        int poisonStack,
        int attributeSavedPositionX,
        int attributeSavedPositionY,
        int attributeSavedPositionZ
    )
    {
        switch (this.currentSceneName)
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
        this.attributePlayerKey = key;
        this.attributePlayerCurrentHp = currentHp;
        this.attributePlayerCurrentConfusion = currentConfusion;
        this.attributePlayerMagicGiantStack = magicGiantStack;
        this.attributePlayerPoisonStack = poisonStack;
        this.attributeSavedPositionX = attributeSavedPositionX;
        this.attributeSavedPositionY = attributeSavedPositionY;
        this.attributeSavedPositionZ = attributeSavedPositionZ;
    }

    private void SaveCurrentIngameAttributes(bool isSavePosition)
    {
        this.attributeSavedPositionEnabled = isSavePosition;

        // 플레이어 속성 값 호출
        this.player.CallUpdatePlayerIngameAttributes();

        // 타입 변환 후 교체 및 저장.
        IngameAttributeCollection ingameAttributes = ConvertPropertyToIngameAttribute();
        SystemManager.instance.SaveIngameAttributes(ingameAttributes: ingameAttributes);
    }

    private void SetIngameAttributeProperties()
    {
        attributeIsActivePlayerBeads = new bool[3];
        attributeIsActivePlayerMinimaps = new bool[3];

        SetIngameAttributesDefault();
        SetIngameAttributesBySavedData(ingameAttributes: SystemManager.instance.ingameAttributes);
    }

    private void SetIngamePreferenceProperties()
    {
        SetIngamePreferenceDefault();
        SetIngamePreferenceBySavedData(preferences: SystemManager.instance.ingamePreferences);
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

        this.attributeIsActivePlayerMagicFairy = false;
        this.attributeIsActivePlayerMagicHuman = false;

        this.attributePlayerLife = 1;
        this.attributePlayerKey = 0;
        this.attributePlayerCurrentHp = 100;
        this.attributePlayerCurrentConfusion = 0;
        this.attributePlayerMagicGiantStack = 0;
        this.attributePlayerPoisonStack = 0;

        this.attributeSavedPositionEnabled = false;
        this.attributeSavedSceneNumber = 0;
        this.attributeSavedPositionX = 0;
        this.attributeSavedPositionY = 0;
        this.attributeSavedPositionZ = 0;
    }

    private void SetIngamePreferenceDefault()
    {
        this.preferenceGuideVisible = this.currentSceneName == NameManager.SCENE_TUTORIAL ? false : true;
        this.preferenceBackMirrorVisible = false;

        this.preferenceBgmVolume = ValueManager.INGAME_PREFERENCE_BGM_VOLUME_MAX;
        this.preferenceSeVolume = ValueManager.INGAME_PREFERENCE_SE_VOLUME_MAX;
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
                    case NameManager.INGAME_ATTRIBUTE_NAME_KEY:
                        attributePlayerKey = attribute.value;
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
                    case NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_ENABLED:
                        attributeSavedPositionEnabled = attribute.value == 0 ? false : true;
                        break;
                    //case NameManager.INGAME_ATTRIBUTE_NAME_TUTORIAL_CLEAR:
                    //    attributeTutorialClear = attribute.value == 0 ? false : true;
                    //    break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_SAVED_SCENE_NUMBER:
                        attributeSavedSceneNumber = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_X:
                        attributeSavedPositionX = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_Y:
                        attributeSavedPositionY = attribute.value;
                        break;
                    case NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_Z:
                        attributeSavedPositionZ = attribute.value;
                        break;
                }
            }
        }
    }

    private void SetIngamePreferenceBySavedData(IEnumerable<IngamePreference> preferences)
    {
        foreach (IngamePreference preference in preferences)
        {
            switch (preference.name)
            {
                case NameManager.INGAME_PREFERENCE_NAME_GUIDE_VISIBLE:
                    this.preferenceGuideVisible = ConvertManager.ConvertStringToInt(preference.value) == 0 ? false : true;
                    break;

                case NameManager.INGAME_PREFERENCE_NAME_BACK_MIRROR_VISIBLE:
                    this.preferenceBackMirrorVisible = ConvertManager.ConvertStringToInt(preference.value) == 0 ? false : true;
                    break;

                case NameManager.INGAME_PREFERENCE_NAME_BGM_VOLUME:
                    this.preferenceBgmVolume = ConvertManager.ConvertStringToFloat(preference.value);
                    break;

                case NameManager.INGAME_PREFERENCE_NAME_SE_VOLUME:
                    this.preferenceSeVolume = ConvertManager.ConvertStringToFloat(preference.value);
                    break;
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
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_TUTORIAL_CLEAR,
            value: SystemManager.instance.isClearTutorial == true ? 1 : 0
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
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_KEY,
            value: this.attributePlayerKey
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

        if (this.attributeSavedPositionEnabled)
        {
            ingameAttributes.Add(new IngameAttribute(
                id: index++,
                userId: userId,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_ENABLED,
                value: this.attributeSavedPositionEnabled == true ? 1 : 0
            ));
            ingameAttributes.Add(new IngameAttribute(
                id: index++,
                userId: userId,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_SCENE_NUMBER,
                value: SceneManager.GetSceneByName(this.currentSceneName).buildIndex
            ));
            ingameAttributes.Add(new IngameAttribute(
                id: index++,
                userId: userId,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_X,
                value: this.attributeSavedPositionX
            ));
            ingameAttributes.Add(new IngameAttribute(
                id: index++,
                userId: userId,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_Y,
                value: this.attributeSavedPositionY
            ));
            ingameAttributes.Add(new IngameAttribute(
                id: index++,
                userId: userId,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_Z,
                value: this.attributeSavedPositionZ
            ));
        }


        return ingameAttributes;
    }

    private IngamePreferenceCollection ConvertPropertyToIngamePreferences()
    {
        IngamePreferenceCollection preferences = new IngamePreferenceCollection();

        int index = 0;

        preferences.Add(new IngamePreference(
            id: index++,
            name: NameManager.INGAME_PREFERENCE_NAME_GUIDE_VISIBLE,
            value: this.preferenceGuideVisible ? 1.ToString() : 0.ToString()
        ));
        preferences.Add(new IngamePreference(
            id: index++,
            name: NameManager.INGAME_PREFERENCE_NAME_BACK_MIRROR_VISIBLE,
            value: this.preferenceBackMirrorVisible ? 1.ToString() : 0.ToString()
        ));
        preferences.Add(new IngamePreference(
            id: index++,
            name: NameManager.INGAME_PREFERENCE_NAME_BGM_VOLUME,
            value: this.preferenceBgmVolume.ToString()
        ));
        preferences.Add(new IngamePreference(
            id: index++,
            name: NameManager.INGAME_PREFERENCE_NAME_SE_VOLUME,
            value: this.preferenceSeVolume.ToString()
        ));

        return preferences;
    }
    #endregion
}
