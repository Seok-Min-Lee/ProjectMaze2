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

    // 카메라
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera, minimapCamera;

    // 인게임 UI
    public GameObject normalPanel, interactPanel, deathPanel;   // 평상시, 상호작용시
    public GameObject interactableAlram;
    public GameObject interactChoicePanel, nextDialogueSignal;  // 선택지, 다음 표시
    public GameObject[] npcChoiceButtons;   // 선택지 각 버튼
    public Text[] npcChoiceTexts;   // 선택지 버튼 내 텍스트
    public Text npcName, npcDialogue;   // 다이얼로그에 표시되는 NPC 이름과 대사

    public GameObject minimap, minimapMarker;   // 지도, 플레이어 마커

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;   // HP, 혼란 게이지
    public Text playerLifeText;
    public GameObject[] playerBeadCovers;

    public GameObject trafficLightPanel;
    public GameObject[] trafficLights;

    SystemManager systemManager;

    // NPC 상호작용 관련

    DialogueCollection dialogueCollection;
    public int dialogueSituationNo; // 테스트 끝나면 private 수정.
    int dialogueSequenceNo, dialogueSequenceSubNo;
    NPCInteractionZone interactNpc;

    // 미니맵 관련
    Vector3 minimapMarkerPoint, minimapCameraPoint; // 플레이어 마커, 카메라 위치
    bool minimapVisible;

    string currentSceneName;

    private void Awake()
    {
        systemManager = new SystemManager();
        // DB 데이터 로드 추가
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

        // UI 세팅.
        UpdateUIWhetherInteraction(isInteract: true);

        // 상호작용 관련 데이터 초기화.
        InitializeInteractionData(npc: interactNpc);

        // 상호작용 호출.
        //TryUpdateInteractionUI();
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
            isLast = systemManager.lastDialogueIndexDictionary.ContainsKey(dialogue.id);

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

                // 이벤트 타입인 경우 해당 이벤트 발생
                OccurDialogueEvent(dialogue: dialogue);

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

    public void InteractChooseOption(int choiceNo)
    {
        dialogueSequenceSubNo = choiceNo;

        // 선택지 선택이 상호작용 키 입력을 대체하고
        // Player 클래스에 작성된 상호작용에 대한 로직을 따라가기 위해 아래와 같이 처리함.
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

        // Player Attribute DB 데이터 업데이트 추가.
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

    // 게임 내 설정 값

    // 아래 변수 및 함수들은 데이터를 읽고 저장할 때에만 사용하기를 권장.
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
