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
    // ī�޶�
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera, minimapCamera;

    // �ΰ��� UI
    public GameObject normalPanel, interactPanel, gameMenuPanel, guidePanel, gameOverPanel;   // ����, ��ȣ�ۿ�, ���Ӹ޴�, ���̵�, ���ӿ���
    public GameObject interactableAlram;
    public GameObject interactChoicePanel, nextDialogueSignal;  // ������, ���� ǥ��
    public GameObject[] npcChoiceButtons;   // ������ �� ��ư
    public Text[] npcChoiceTexts;   // ������ ��ư �� �ؽ�Ʈ
    public Text npcName, npcDialogue;   // ���̾�α׿� ǥ�õǴ� NPC �̸��� ���
    public Text guideHead, guideBody;   // ���̵� ����, ����

    public GameObject minimap, minimapMarker;   // ����, �÷��̾� ��Ŀ

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;   // HP, ȥ�� ������
    public Text playerLifeText;
    public GameObject magicFairyImage, magicGiantImage, magicHumanImage, poisonImage, confusionImage;
    public Text magicGiantStackText, poisonStackText;
    public GameObject[] playerBeadCovers;

    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;
    public Material[] skyboxMaterials;

    // NPC ��ȣ�ۿ� ����

    DialogueCollection dialogueCollection;
    public int dialogueSituationNo; // �׽�Ʈ ������ private ����.
    int dialogueSequenceNo, dialogueSequenceSubNo;
    NPCInteractionZone interactNpc;

    // �̴ϸ� ����
    Vector3 minimapMarkerPoint, minimapCameraPoint; // �÷��̾� ��Ŀ, ī�޶� ��ġ
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

        // UI ����.
        UpdateUIWhetherInteraction(isInteract: true);

        // ��ȣ�ۿ� ���� ������ �ʱ�ȭ.
        InitializeInteractionData(npc: interactNpc);
    }

    public void UpdateUIWhetherInteraction(bool isInteract)
    {
        // Ȱ��ȭ�� ī�޶� ��ü.
        followCamera.gameObject.SetActive(!isInteract);
        npcInteractionCamera.gameObject.SetActive(isInteract);

        // �÷��̾� ���� ����.
        player._input.controlEnable = !isInteract;
        Cursor.lockState = isInteract ? CursorLockMode.Confined : CursorLockMode.Locked;

        // Ȱ��ȭ�� UI ��ü.
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

                // ������ Ȱ��ȭ
                for (int i = 0; i < options.Count; i++)
                {
                    npcChoiceButtons[i].SetActive(true);
                    npcChoiceTexts[i].text = options[i].text;
                }

                nextDialogueSignal.SetActive(false);

                // ��ȣ�ۿ� �Է� ��Ȱ��ȭ
                player._input.interactEnable = false;
            }
            else
            {
                // ������ ��Ȱ��ȭ
                foreach (GameObject button in npcChoiceButtons)
                {
                    if (button.activeSelf)
                    {
                        button.SetActive(false);
                    }
                }

                // ���� ��ũ��Ʈ ���� ǥ��.
                nextDialogueSignal.SetActive(!isLast);

                // �̺�Ʈ Ÿ���� ��� ������ ���̾�α׿��� �ش� �̺�Ʈ �߻�
                if(isLast)
                {
                    OccurDialogueEvent(dialogue: dialogue);
                }

                // ��ȣ�ۿ� �Է� Ȱ��ȭ
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

        // ������ ������ ��ȣ�ۿ� Ű �Է��� ��ü�ϰ�
        // Player Ŭ������ �ۼ��� ��ȣ�ۿ뿡 ���� ������ ���󰡱� ���� �Ʒ��� ���� ó����.
        player._input.interact = true;
        player.Interact();
    }

    public void OnClickGameMenuOKButton()
    {
        SwitchPauseAndCursorLockEvent(panel: ref this.gameMenuPanel, isActive: ref this.isDisplayGameMenu);

        // ���� �� ���� - �߰��� ��
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

        // ���� �� ���� - �߰��� ��
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

        // Player Attribute DB ������ ������Ʈ �߰�.
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
        // ���̷��α� Ÿ���� �����Ѵ�.
        float randomTypeValue = UnityEngine.Random.Range(minInclusive: 0, maxExclusive:10);
        DialogueType dialogueType = randomTypeValue > 4 ? DialogueType.Normal : DialogueType.Event;

        // ���̾�α� SequenceSubNo �� �ִ� �ּҰ� ���Ѵ�.
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
        // ü�� �� ����
        playerLifeText.text = ValueManager.PREFIX_PLAYER_LIFE + player.currentLife.ToString();
        playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
        playerConfusionBar.localScale = new Vector3((float)player.confusionStack / player.confusionStackMax, 1, 1);

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
        interactableAlram.SetActive(player.isInteractPreprocessReady ? true : false);

        // ���ӿ���
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
        // index �÷��̾��� �ΰ��� �Ӽ����� �̴ϸʿ� ���� ���� �迭�̱� ������ ���ϴ� ���� ã�� ���� �ε���
        // isVisible ���� ���� �̴ϸ��� ���� ����

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

    // �Ʒ� ���� �� �Լ����� �����͸� �а� ������ ������ ����ϱ⸦ ����.
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
        // �÷��̾� �Ӽ� �� ȣ��
        this.player.CallUpdatePlayerIngameAttributes();

        // Ÿ�� ��ȯ �� ��ü �� ����.
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
