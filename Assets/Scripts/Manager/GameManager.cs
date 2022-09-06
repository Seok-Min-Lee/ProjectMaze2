using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ī�޶�
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera, minimapCamera;

    // UI
    public GameObject normalPanel, interactPanel, gameMenuPanel, guidePanel, gameOverPanel;   // UI �гε�

    // Normal Panel
    public GameObject minimap, minimapMarker;   // ����, �÷��̾� ��Ŀ
    public GameObject backMirror;               // �� �̷�
    public GameObject interactableAlram;        // ��ȣ�ۿ� ���� ǥ��
    public Text interactableAlramText;
    public Text confirmMessageText;

    public GameObject[] playerBeadCovers;
    public GameObject magicFairyImage, magicGiantImage, magicHumanImage, poisonImage, confusionImage;   // Ư�� ȿ�� Ȱ��ȭ ǥ��
    public RectTransform playerHpBar, playerConfusionBar;   // �÷��̾� HP, ȥ�� ������
    public Text playerLifeText;     // �÷��̾� ���� ������ ǥ��
    public Text playerKeyText;
    public Text magicGiantStackText, poisonStackText;   // Ư��ȿ�� ���� ǥ��
    
    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;

    // Interact Panel   
    public GameObject interactChoicePanel, nextDialogueSignal;  // ������, ���� ǥ��
    public GameObject[] npcChoiceButtons;   // ������ �� ��ư
    public Text[] npcChoiceTexts;   // ������ ��ư �� �ؽ�Ʈ
    public Text npcName, npcDialogue;   // ���̾�α׿� ǥ�õǴ� NPC �̸��� ���

    // Game Menu Panel
    public Toggle displayGuideToggle, displayBackMirrorToggle;
    public Slider bgmSlider, seSlider;

    // Guide Panel
    public Text guideHead, guideBody;   // ���̵� ����, ����

    public Material[] skyboxMaterials;
    public AudioMixer masterMixer;
    
    public string currentSceneName { get; private set; }
    
    private Player player;
    private bool isPause, isDisplayNormal, isDisplayGuide, isDisplayGameMenu, isDisplayGameOver, isDisplayInteract;

    // �̴ϸ� ����
    private Vector3 minimapMarkerPoint, minimapCameraPoint; // �÷��̾� ��Ŀ, ī�޶� ��ġ
    private bool minimapVisible;

    // NPC ��ȣ�ۿ� ����
    private DialogueCollection dialogues;
    private VillageObjectManager villageObjectManager;
    private NpcObject interactNpcObject;
    public int situationNo { get; private set; }
    private int dialogueSequenceNo, dialogueCaseNo;

    private float skyboxRotation;

    private bool isLatestPlayerInteractable, isLatestPlayerInteractWithNpc, isLatestPlayerInputInteract, isLatestPlayerInputEscape;

    private void Awake()
    {
        // Attribute, Preference �ʱ�ȭ
        SetIngameAttributeProperties();
        SetIngamePreferenceProperties();
    }

    private void Start()
    {
        // ������Ƽ �ʱ�ȭ
        this.player = GameObject.FindGameObjectWithTag(tag: NameManager.TAG_PLAYER).GetComponent<Player>();

        this.currentSceneName = SceneManager.GetActiveScene().name;
        this.situationNo = SystemManager.instance.isClearGame ? 1 : 0;
        this.isDisplayNormal = normalPanel.activeSelf;

        this.confirmMessageText.text = string.Empty;

        // preferences �� ����
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

        // ������ ��Ȱ��ȭ
        foreach (GameObject button in this.npcChoiceButtons)
        {
            if (button.activeSelf)
            {
                button.SetActive(false);
            }
        }

        // ��Ȱ��ȭ �Ǿ��ִ� interactEnable Ȱ��ȭ ��Ų��.
        // Ű �Է��� ���� ������ ó���Ѵ�. 
        player._input.interactEnable = true;
        player._input.interact = false;
        TryContinueInteraction();
    }

    public void OnClickGameMenuOKButton()
    {
        // UI �� �ΰ����� ���� ������Ʈ
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // ������ Preference �� ���� ������ ������Ʈ.
        this.preferenceGuideVisible = displayGuideToggle.isOn;
        this.preferenceBackMirrorVisible = displayBackMirrorToggle.isOn;
        this.preferenceBgmVolume = bgmSlider.value;
        this.preferenceSeVolume = seSlider.value;

        // Preference ����
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );

        // Ÿ�� ��ȯ �� ��ü �� ����.
        IngamePreferenceCollection preferences = ConvertPropertyToIngamePreferences();
        SystemManager.instance.SaveIngamePreferences(preferences: preferences);
    }

    public void OnClickGameMenuCancelButton()
    {
        // UI �� �ΰ����� ���� ������Ʈ
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // ���� Preference �� �ѹ�
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );
    }

    public void OnClickGameMenuLobbyButton()
    {
        // UI �� �ΰ����� ���� ������Ʈ
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // ���� ȯ�� ������ �ѹ�
        SetIngamePreferences(
            backMirrorVisible: this.preferenceBackMirrorVisible,
            bgmVolume: this.preferenceBgmVolume,
            seVolume: this.preferenceSeVolume
        );

        // Attribute ����
        SaveCurrentIngameAttributes(isSavePosition: true);
        SystemManager.instance.DeleteDataExceptUsers();

        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_LOBBY);
    }

    public void OnClickGameMenuQuitButton()
    {
        // UI �� �ΰ����� ���� ������Ʈ
        ReverseObjectSetActive(obj: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);
        UpdateExtraSettingAll();

        // ���� ȯ�� ������ �ѹ�
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
        // UI �� �ΰ����� ���� ������Ʈ
        ReverseObjectSetActive(obj: ref this.guidePanel, isActive: ref this.isDisplayGuide);
        UpdateExtraSettingAll();
    }

    public void OnClickGameOverReTryButton()
    {
        // UI �� �ΰ����� ���� ������Ʈ
        ReverseObjectSetActive(obj: ref this.gameOverPanel, isActive: ref this.isDisplayGameOver);
        UpdateExtraSettingAll();

        // ������ �ִ� Attribute �ʱ�ȭ.
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
            // Ʃ�丮���� ��� Ʃ�丮�� Ŭ���� ���θ� �����Ѵ�.
            IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();
            ingameAttributes.Add(new IngameAttribute(
                id: 0,
                userId: SystemManager.instance.logInedUser.id,
                attributeName: NameManager.INGAME_ATTRIBUTE_NAME_TUTORIAL_CLEAR,
                value: 1
            ));
            SystemManager.instance.SaveIngameAttributes(ingameAttributes: ingameAttributes);
            
            // �κ�� �������� SystemManager �����͸� �����.
            SystemManager.instance.DeleteDataExceptUsers();
        }
        else
        {
            // Player Attribute DB ������ ������Ʈ �߰�.
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
        // ObjectManager �� �����ϰ� ���� ��.
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

    #region ##### ���� ȯ�� ���� #####
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
        // Bead UI�� ���� �̹��� ���� Ŀ�� �̹����� ���� �ִ�.
        // ȹ���� ������ ������ ���� �ִ� Ŀ�� �̹����� ��Ȱ��ȭ ��Ų��.
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
        // index : �÷��̾��� �ΰ��� �Ӽ����� �̴ϸʿ� ���� ���� �迭�̱� ������ ���ϴ� ���� ã�� ���� �ε���
        // isVisible : ���� ���� �̴ϸ��� ���� ����

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
        // ���̵� ǥ�ô� ObjectManager Ŭ�������� �����Ѵ�.

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
            // ü�� �� ����
            playerLifeText.text = ValueManager.PREFIX_PLAYER_LIFE + player.currentLife.ToString();
            playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
            playerConfusionBar.localScale = new Vector3((float)player.confusionStack / player.confusionStackMax, 1, 1);

            playerKeyText.text = ValueManager.PREFIX_PLAYER_KEY + player.currentKey.ToString();

            // ���� ȿ��
            bool isActiveMagicGiant = player.magicGiantStack > 0;
            bool isActivePoision = player.poisonStack > 0;

            magicFairyImage.SetActive(player.isActiveMagicFairy);
            magicGiantImage.SetActive(isActiveMagicGiant);
            magicHumanImage.SetActive(player.isActiveMagicHuman);
            poisonImage.SetActive(isActivePoision);
            confusionImage.SetActive(player.isConfusion);

            magicGiantStackText.text = ValueManager.PREFIX_PLAYER_EFFECT_STACK + player.magicGiantStack.ToString();
            poisonStackText.text = ValueManager.PREFIX_PLAYER_EFFECT_STACK + player.poisonStack.ToString();

            // ��ȣ�ۿ� ������ ���
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
        // Ű �Է� Ȯ��
        if (!this.isLatestPlayerInputInteract && player._input.interact)
        {
            // ��ȣ�ۿ� ���� ���� Ȯ��
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
                    // ���̵� �г� Ȱ��ȭ�Ǿ� �ִ� ��� OK ó��
                    OnClickGuideOKButton();
                }
            }
            else
            {
                // ���� �޴� �г� Ȱ��ȭ �Ǿ� �ִ� ��� Cancel ó��
                OnClickGameMenuCancelButton();
            }
        }

        this.isLatestPlayerInputEscape = player._input.escape;
    }

    private void DisplayInteractionWithNpc()
    {
        if (this.isLatestPlayerInteractWithNpc && player.isInteract)
        {
            // ���� ���̾�αװ� ���� ��� ��ȣ�ۿ��� ��ģ��.
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

            ReverseInteractionSetting();        // UI ����
            InitInteractionWithNpcSettings();   // Dialogue ����
            TryContinueInteraction();           // ��ȣ�ۿ� ����

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

                nextDialogueSignal.SetActive(false);    // ���� ���̾�α� ǥ�� off
                player._input.interactEnable = false;   // �ɼ� ������ ���� ��ȣ�ۿ� Ű�Է� off
            }
            else
            {
                OccurDialogueEvent(dialogue: dialogue, isLast: isLast);

                this.nextDialogueSignal.SetActive(!isLast); // ���� ���̾�α� ǥ�� ������Ʈ
            }

            this.dialogueSequenceNo++;

            return true;
        }

        return false;
    }

    private void InitInteractionWithNpcSettings()
    {
        // ���̾�α� ���� �ʱ�ȭ.
        this.dialogues = interactNpcObject.dialogues;

        this.dialogueCaseNo = GetDialougeCaseNo();
        this.dialogueSequenceNo = 0;

        // �÷��̾� ��ġ �� ���� �ʱ�ȭ.
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

        // ������ Ȱ��ȭ
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
        // ���̷��α� Ÿ���� �����Ѵ�.
        float randomTypeValue = Random.Range(minInclusive: 0, maxExclusive: 10);
        DialogueType dialogueType = randomTypeValue > 4 ? DialogueType.Normal : DialogueType.Event;

        // ���̾�α� SequenceSubNo �� �ִ� �ּҰ� ���Ѵ�.
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
        // Ȱ��ȭ�� ī�޶� ��ü.
        bool isActiveFollowCamera = followCamera.activeSelf;
        bool isActiveInteractionCamera = npcInteractionCamera.activeSelf;
        ReverseObjectSetActive(obj: ref this.followCamera, isActive: ref isActiveFollowCamera);
        ReverseObjectSetActive(obj: ref this.npcInteractionCamera, isActive: ref isActiveInteractionCamera);

        if (this.npcInteractionCamera.activeSelf)
        {
            MoveGameObject(obj: this.npcInteractionCamera, vector: player.interactableNpcObject.cameraPoint.position);
            this.npcInteractionCamera.transform.rotation = player.interactableNpcObject.cameraPoint.rotation;
        }

        // Ȱ��ȭ�� UI ��ü.
        ReverseObjectSetActive(obj: ref this.normalPanel, isActive: ref this.isDisplayNormal);
        ReverseObjectSetActive(obj: ref this.interactPanel, isActive: ref this.isDisplayInteract);

        // �ΰ� ����.
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
        // ��������� Ŀ���� �ʿ��� ��� �����Ӱ� �þ��� ������ �����Ѵ�.
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

    #region ##### ������ ó�� ����#####
    // �Ʒ� ���� �� �Լ����� �����͸� �а� ������ ������ ����ϱ⸦ ����.
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

        // �÷��̾� �Ӽ� �� ȣ��
        this.player.CallUpdatePlayerIngameAttributes();

        // Ÿ�� ��ȯ �� ��ü �� ����.
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
