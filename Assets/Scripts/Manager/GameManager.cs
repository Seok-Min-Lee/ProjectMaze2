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
    const string PREFIX_PLAYER_LIFE = "X ";

    // ī�޶�
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera, minimapCamera;

    // �ΰ��� UI
    public GameObject normalPanel, interactPanel, deathPanel;   // ����, ��ȣ�ۿ��
    public GameObject interactableAlram;
    public GameObject interactChoicePanel, nextDialogueSignal;  // ������, ���� ǥ��
    public GameObject[] npcChoiceButtons;   // ������ �� ��ư
    public Text[] npcChoiceTexts;   // ������ ��ư �� �ؽ�Ʈ
    public Text npcName, npcDialogue;   // ���̾�α׿� ǥ�õǴ� NPC �̸��� ���

    public GameObject minimap, minimapMarker;   // ����, �÷��̾� ��Ŀ

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;   // HP, ȥ�� ������
    public Text playerLifeText;
    public GameObject[] playerBeadCovers;

    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;

    SystemManager systemManager;

    // NPC ��ȣ�ۿ� ����

    DialogueCollection dialogueCollection;
    public int dialogueSituationNo; // �׽�Ʈ ������ private ����.
    int dialogueSequenceNo, dialogueSequenceSubNo;
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

        if(TryGetMinimapAttributesBySceneName(sceneName: this.currentSceneName, index: out int minimapIndex))
        {
            minimapVisible = this.isActivePlayerMinimaps[minimapIndex];
        }

        ActivateMinimap(isActive: minimapVisible);
        UpdateUIActivedBeads(isActives: this.isActivePlayerBeads);
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
        //TryUpdateInteractionUI();
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
            isLast = systemManager.lastDialogueIndexDictionary.ContainsKey(dialogue.id);

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

                // �̺�Ʈ Ÿ���� ��� �ش� �̺�Ʈ �߻�
                OccurDialogueEvent(dialogue: dialogue);

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

    public void InteractChooseOption(int choiceNo)
    {
        dialogueSequenceSubNo = choiceNo;

        // ������ ������ ��ȣ�ۿ� Ű �Է��� ��ü�ϰ�
        // Player Ŭ������ �ۼ��� ��ȣ�ۿ뿡 ���� ������ ���󰡱� ���� �Ʒ��� ���� ó����.
        player._input.interact = true;
        player.Interact();
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

    public void UpdateUIActivedBeads(bool[] isActives)
    {
        for(int i=0; i< playerBeadCovers.Count(); i++)
        {
            playerBeadCovers[i].SetActive(!isActives[i]);
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
                    sequenceSubNo = systemManager.dialogueMagicGiantSequenceSubNo;
                }
                else
                {
                    if (UnityEngine.Random.Range(minInclusive: 0, maxExclusive: 2) > 1)
                    {
                        sequenceSubNo = systemManager.dialogueMagicFairySequenceSubNo;
                    }
                    else
                    {
                        sequenceSubNo = systemManager.dialogueMagicGiantSequenceSubNo;
                    }
                }
            }
            else
            {
                sequenceSubNo = systemManager.dialogueMagicHumanSequenceSubNo;
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
            if(!player.isActiveMagicFairy && dialogue.sequenceSubNo == systemManager.dialogueMagicFairySequenceSubNo)
            {
                player.isActiveMagicFairy = true;
            }
            else if(player.magicGiantStack <= ValueManager.PLAYER_MAGIC_GIANT_STACK_MAX && dialogue.sequenceSubNo == systemManager.dialogueMagicGiantSequenceSubNo)
            {
                player.magicGiantStack++;
            }
            else if(!player.isActiveMagicHuman && dialogue.sequenceSubNo == systemManager.dialogueMagicHumanSequenceSubNo)
            {
                player.isActiveMagicHuman = true;
            }
        }
    }

    private void DefaultUIUpadate()
    {
        playerLifeText.text = PREFIX_PLAYER_LIFE + player.currentLife.ToString();
        playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
        playerConfusionBar.localScale = new Vector3((float)player.confusionStack / player.confusionStackMax, 1, 1);

        interactableAlram.SetActive(player.isInteractPreprocessReady ? true : false);

        if (player.currentLife <= 0 && player.currentHp <= 0)
        {
            deathPanel.SetActive(true);
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

    // ���� �� ���� ��

    // �Ʒ� ���� �� �Լ����� �����͸� �а� ������ ������ ����ϱ⸦ ����.
    bool[] isActivePlayerBeads;
    bool[] isActivePlayerMinimaps;
    bool isActivePlayerMagicFairy, isActivePlayerMagicHuman;
    int playerLife, playerCurrentHp, playerCurrentConfusion, playerMagicGiantStack, playerPoisonStack;

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
                    playerMagicGiantStack = attribute.value;
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
        isActiveMagicHuman = this.isActivePlayerMagicHuman;
        life = this.playerLife;
        currentHp = this.playerCurrentHp;
        currentConfusion = this.playerCurrentConfusion;
        magicGiantStack = this.playerMagicGiantStack;
        poisonStack = this.playerPoisonStack;
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
        this.isActivePlayerMagicHuman = isActiveMagicHuman;
        this.playerLife = life;
        this.playerCurrentHp = currentHp;
        this.playerCurrentConfusion = currentConfusion;
        this.playerMagicGiantStack = magicGiantStack;
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
            value: this.playerMagicGiantStack
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
