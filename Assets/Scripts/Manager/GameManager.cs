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
    public GameObject normalPanel, interactPanel;   // ����, ��ȣ�ۿ��
    public GameObject interactChoicePanel, nextDialogueSignal;  // ������, ���� ǥ��
    public GameObject[] npcChoiceButtons;   // ������ �� ��ư
    public Text[] npcChoiceTexts;   // ������ ��ư �� �ؽ�Ʈ
    public Text npcName, npcDialogue;   // ���̾�α׿� ǥ�õǴ� NPC �̸��� ���

    public GameObject minimap, minimapMarker;   // ����, �÷��̾� ��Ŀ

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;   // HP, ȥ�� ������

    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;

    SystemManager systemManager;

    // NPC ��ȣ�ۿ� ����
    DialogueCollection dialogueCollection;
    int dialogueSituationNo, dialogueSequenceNo, dialogueLastSequenceNo, dialogueSequenceSubNo;
    NPCInteractionZone interactNpc;

    // �̴ϸ� ����
    Vector3 minimapMarkerPoint, minimapCameraPoint; // �÷��̾� ��Ŀ, ī�޶� ��ġ
    bool minimapVisible;

    string currentSceneName;

    private void Awake()
    {
        systemManager = new SystemManager();
        // DB ������ �ε� �߰�
        // systemManager.GetDbData();

        currentSceneName = SceneManager.GetActiveScene().name;
        SetIngameAttributes();

        minimapVisible = GetMinimapVisibilityBySceneName(sceneName: this.currentSceneName);
        ActivateMinimap(isActive: minimapVisible);
    }

    private void Start()
    {
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            DefaultUIUpadate();
            UpdateMinimap();
        }
    }

    public void UpdateUINormalToInteraction(NPCInteractionZone npc)
    {
        interactNpc = npc;

        // UI ����.
        UpdateUIWhetherInteraction(isInteract: true);

        // ��ȣ�ۿ� ���� ������ �ʱ�ȭ.
        InitializeInteractionData(npc: interactNpc);

        // ��ȣ�ۿ� ȣ��.
        TryUpdateInteractionUI();
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

    public bool TryUpdateInteractionUI()
    {
        Dialogue dialogue;

        if (TryGetNextDialogue(dialogue: out dialogue))
        {
            npcDialogue.text = dialogue.text;

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
                if(dialogue.sequenceNo < dialogueLastSequenceNo)
                {
                    nextDialogueSignal.SetActive(true);
                }
                else
                {
                    nextDialogueSignal.SetActive(false);
                }

                // ��ȣ�ۿ� �Է� Ȱ��ȭ
                player._input.interactEnable = true;
            }

            dialogueSequenceNo++;
        }

        return dialogueSequenceNo > dialogueLastSequenceNo ? true : false;
    }

    public void InteractChooseOption(int choiceNo)
    {
        dialogueSequenceSubNo = choiceNo;
        TryUpdateInteractionUI();
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
                    isActivePlayerMinimaps[0] = true;
                    break;
                case NameManager.SCENE_STAGE_2:
                    isActivePlayerMinimaps[1] = true;
                    break;
                case NameManager.SCENE_STAGE_3:
                    isActivePlayerMinimaps[2] = true;
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

    public void UpdateScene(SceneType sceneType)
    {
        string sceneName = ConvertManager.ConvertSceneTypeToString(sceneType: sceneType);

        // Player Attribute DB ������ ������Ʈ �߰�.
        IngameAttributeCollection ingameAttributes = ConvertPropertyToIngameAttribute();
        systemManager.WriteIngameAttributeToJsonData(ingameAttributes);
        //

        SceneManager.LoadScene(sceneName: sceneName);
    }

    public void MoveGameObject(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.position = vector;
    }

    private void InitializeInteractionData(NPCInteractionZone npc)
    {
        if (systemManager.GetNpcIndexByName(name: npc.npcName, index: out int npcIndex) &&
            systemManager.GetDialoguesByNpcIndex(index: npcIndex, dialogueCollection: out DialogueCollection dialogues))
        {
            this.dialogueSituationNo = 0;
            this.dialogueSequenceNo = 0;
            this.dialogueSequenceSubNo = 0;

            dialogueCollection = new DialogueCollection(dialogues.Where(dialogue => dialogue.situationNo == this.dialogueSituationNo));
            this.dialogueLastSequenceNo = dialogueCollection.OrderBy(dialogue => dialogue.sequenceNo).LastOrDefault().sequenceNo;

            npcName.text = npc.npcName;
        }
    }

    private bool TryGetNextDialogue(out Dialogue dialogue)
    {
        DialogueCollection dialogues = new DialogueCollection(dialogueCollection.Where(dialogue => dialogue.sequenceNo == this.dialogueSequenceNo));

        if (dialogues.Count > 0)
        {
            if (dialogues.Count > 1)
            {
                dialogue = dialogues.Where(dialogue => dialogue.sequenceSubNo == dialogueSequenceSubNo).FirstOrDefault();
            }
            else
            {
                dialogue = dialogues.FirstOrDefault();
            }

            return true;
        }
        else
        {
            dialogue = new Dialogue();

            return false;
        }
    }

    private void DefaultUIUpadate()
    {
        playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
        playerConfusionBar.localScale = new Vector3((float)player.currentConfusion / player.maxConfusion, 1, 1);
    }

    private void UpdateMinimap()
    {
        if (minimapCamera != null && minimapVisible)
        {
            minimapMarkerPoint = player.transform.position;
            minimapMarkerPoint.y = 0;

            minimapMarker.transform.position = minimapMarkerPoint;

            minimapCameraPoint = minimapMarkerPoint;
            minimapCameraPoint.y = minimapCamera.transform.position.y;
            minimapCamera.transform.position = minimapCameraPoint;
        }
    }

    private bool GetMinimapVisibilityBySceneName(string sceneName)
    {
        bool result;

        switch (sceneName)
        {
            case NameManager.SCENE_STAGE_1:
                result = true;
                break;
            case NameManager.SCENE_STAGE_2:
                result = true;
                break;
            case NameManager.SCENE_STAGE_3:
                result  = true;
                break;
            default:
                result = false;
                break;
        }

        return result;
    }

    // ���� �� ���� ��

    // �Ʒ� ���� �� �Լ����� �����͸� �а� ������ ������ ����ϱ⸦ ����.
    bool[] isActivePlayerBeads;
    bool[] isActivePlayerMinimaps;
    bool isActivePlayerMagicFairy, isActivePlayerMagicGiant, isActivePlayerMagicHuman;
    int playerLife, playerCurrentHp, playerCurrentConfusion, playerPoisonStack;

    private void SetIngameAttributes()
    {
        isActivePlayerBeads = new bool[3];
        isActivePlayerMinimaps = new bool[3];

        IngameAttributeCollection ingameAttributes = systemManager.ingameAttributeCollection;

        foreach (IngameAttribute attribute in ingameAttributes)
        {
            switch (attribute.attributeName)
            {
                case NameManager.INGAME_ATTRIBUTE_NAME_BEAD_1:
                    isActivePlayerBeads[0] = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_BEAD_2:
                    isActivePlayerBeads[1] = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_BEAD_3:
                    isActivePlayerBeads[2] = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_LIFE:
                    playerLife = attribute.value;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_HP:
                    playerCurrentHp = attribute.value;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_CONFUSION:
                    playerCurrentConfusion = attribute.value;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_FAIRY:
                    isActivePlayerMagicFairy = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_GIANT:
                    isActivePlayerMagicGiant = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_HUMAN:
                    isActivePlayerMagicHuman = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_POISON_STACK:
                    playerPoisonStack = attribute.value;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_1:
                    isActivePlayerMinimaps[0] = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_2:
                    isActivePlayerMinimaps[1] = attribute.value == 0 ? false : true;
                    break;
                case NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_3:
                    isActivePlayerMinimaps[2] = attribute.value == 0 ? false : true;
                    break;
            }
        }
    }

    const int PLAYER_POISON_STACK_MAX = 5, PLAYER_POISON_TIC_DAMAGE = 2;    // �� �ִ� ����, �� ���ô� ��Ʈ ������
    const float PLAYER_ACTIVATE_TIME_DETOX = 5.0f;     //'�ߵ�' ���� ������ ���� ���Է� �ð�
    const float PLAYER_ACTIVATE_TIME_CONFUSION = 1.0f, PLAYER_DURATION_CONFUSION = 5.0f;  // '����' ���� ������Ʈ �ð�, '����' ���� �� ���� �ð�

    public void InitializePlayer(
        out int playerPoisonStackMax,
        out int playerPoisonTicDamage,
        out float playerActivateTimeDetox,
        out float playerActivateTimeConfusion,
        out float playerDurationConfusion
    )
    {
        playerPoisonStackMax = PLAYER_POISON_STACK_MAX;
        playerPoisonTicDamage = PLAYER_POISON_TIC_DAMAGE;
        playerActivateTimeDetox = PLAYER_ACTIVATE_TIME_DETOX;
        playerActivateTimeConfusion = PLAYER_ACTIVATE_TIME_CONFUSION;
        playerDurationConfusion = PLAYER_DURATION_CONFUSION;
    }

    public void SetPlayerIngameAttributes(
        out bool[] isActiveBeads,
        out bool isActiveMinimap,
        out bool isActiveMagicFairy,
        out bool isActiveMagicGiant,
        out bool isActiveMagicHuman,
        out int life,
        out int currentHp,
        out int currentConfusion,
        out int poisonStack
    )
    {
        switch (currentSceneName)
        {
            case NameManager.SCENE_STAGE_1:
                isActiveMinimap = this.isActivePlayerMinimaps[0];
                break;
            case NameManager.SCENE_STAGE_2:
                isActiveMinimap = this.isActivePlayerMinimaps[1];
                break;
            case NameManager.SCENE_STAGE_3:
                isActiveMinimap = this.isActivePlayerMinimaps[2];
                break;
            default:
                isActiveMinimap = false;
                break;
        }

        isActiveBeads = this.isActivePlayerBeads;
        isActiveMagicFairy = this.isActivePlayerMagicFairy;
        isActiveMagicGiant = this.isActivePlayerMagicGiant;
        isActiveMagicHuman = this.isActivePlayerMagicHuman;
        life = this.playerLife;
        currentHp = this.playerCurrentHp;
        currentConfusion = this.playerCurrentConfusion;
        poisonStack = this.playerPoisonStack;
    }

    public void UpdatePlayerIngameAttributes(
        bool[] isActiveBeads,
        bool isActiveMinimap,
        bool isActiveMagicFairy,
        bool isActiveMagicGiant,
        bool isActiveMagicHuman,
        int life,
        int currentHp,
        int currentConfusion,
        int poisonStack
    )
    {
        switch (currentSceneName)
        {
            case NameManager.SCENE_STAGE_1:
                this.isActivePlayerMinimaps[0] = isActiveMinimap;
                break;
            case NameManager.SCENE_STAGE_2:
                this.isActivePlayerMinimaps[1] = isActiveMinimap;
                break;
            case NameManager.SCENE_STAGE_3:
                this.isActivePlayerMinimaps[2] = isActiveMinimap;
                break;
        }

        this.isActivePlayerBeads = isActiveBeads;
        this.isActivePlayerMagicFairy = isActiveMagicFairy;
        this.isActivePlayerMagicGiant = isActiveMagicGiant;
        this.isActivePlayerMagicHuman = isActiveMagicHuman;
        this.playerLife = life;
        this.playerCurrentHp = currentHp;
        this.playerCurrentConfusion = currentConfusion;
        this.playerPoisonStack = poisonStack;
    }

    private IngameAttributeCollection ConvertPropertyToIngameAttribute()
    {
        IngameAttributeCollection ingameAttributes = new IngameAttributeCollection();
        int index = 0;

        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_BEAD_1,
            value: this.isActivePlayerBeads[0] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_BEAD_2,
            value: this.isActivePlayerBeads[1] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_BEAD_3,
            value: this.isActivePlayerBeads[2] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_1,
            value: this.isActivePlayerMinimaps[0] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_2,
            value: this.isActivePlayerMinimaps[1] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MINIMAP_3,
            value: this.isActivePlayerMinimaps[2] == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_FAIRY,
            value: this.isActivePlayerMagicFairy == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_GIANT,
            value: this.isActivePlayerMagicGiant == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_MAGIC_HUMAN,
            value: this.isActivePlayerMagicHuman == true ? 1 : 0
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_LIFE,
            value: this.playerLife
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_HP,
            value: this.playerCurrentHp
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_CURRENT_CONFUSION,
            value: this.playerCurrentConfusion
        ));
        ingameAttributes.Add(new IngameAttribute(
            id: index++,
            attributeName: NameManager.INGAME_ATTRIBUTE_NAME_POISON_STACK,
            value: this.playerPoisonStack
        ));

        return ingameAttributes;
    }
}
