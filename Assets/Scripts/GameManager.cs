using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera;
    public GameObject NormalPanel, InteractPanel;
    public GameObject interactChoicePanel;
    public GameObject[] npcChoiceButtons;
    public Text[] npcChoiceTexts;
    public Text npcName, npcDialogue;

    SystemManager systemManager;
    private void Awake()
    {
        systemManager = new SystemManager();
    }

    const int PLAYER_POISON_STACK_MAX = 5, PLAYER_POISON_TIC_DAMAGE = 2;    // 독 최대 스택, 독 스택당 도트 데미지
    const float PLAYER_ACTIVATE_TIME_DETOX = 5.0f;     //'중독' 상태 해제를 위한 미입력 시간
    const float PLAYER_ACTIVATE_TIME_CONFUSION = 1.0f, PLAYER_DURATION_CONFUSION = 5.0f;  // '공포' 스택 업데이트 시간, '공포' 발현 시 지속 시간

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

    DialogueCollection dialogueCollection;
    int dialogueSituationNo, dialogueSequenceNo, dialogueLastSequenceNo, dialogueSequenceSubNo;
    NPC interactNpc;
    public void UpdateUINormalToInteract(bool isInteract, NPC npc)
    {
        if(!string.Equals(interactNpc.name, npc.name))
        {
            interactNpc = npc;
        }
            
        if (isInteract)
        {
            // UI 세팅.
            UpdateUISetting(isInteract: isInteract);

            // 상호작용 관련 데이터 초기화.
            InitializeInteractionData(npc: npc);

            // 상호작용 호출.
            UpdateInteractionUI(isEnd: out bool isEnd);
        }
        else
        {
            // UI 세팅.
            UpdateUISetting(isInteract: isInteract);
        }
    }

    private void UpdateUISetting(bool isInteract)
    {
        // 활성화된 카메라 교체.
        followCamera.gameObject.SetActive(!isInteract);
        npcInteractionCamera.gameObject.SetActive(isInteract);

        // 플레이어 조작 설정.
        player._input.controlEnable = !isInteract;
        Cursor.lockState = isInteract ? CursorLockMode.Confined : CursorLockMode.Locked;

        // 활성화된 UI 교체.
        NormalPanel.SetActive(!isInteract);
        InteractPanel.SetActive(isInteract);
    }

    private void InitializeInteractionData(NPC npc)
    {
        if (systemManager.GetNpcIndexByName(name: name, index: out int npcIndex) &&
            systemManager.GetDialoguesByNpcIndex(index: npcIndex, dialogueCollection: out dialogueCollection))
        {
            this.dialogueSituationNo = 0;
            this.dialogueSequenceNo = 0;
            this.dialogueSequenceSubNo = 0;
            npcName.text = npc.name;

            dialogueLastSequenceNo = dialogueCollection.LastOrDefault().sequenceNo;
        }
    }

    public void UpdateInteractionUI(out bool isEnd)
    {
        Dialogue dialogue;
        if (TryGetNextDialogue(dialogue: out dialogue))
        {
            npcDialogue.text = dialogue.text;

            if (dialogue.type == DialogueType.Question)
            {
                DialogueCollection options = new DialogueCollection(dialogueCollection.Where(x => x.type == DialogueType.Option && x.sequenceNo == this.dialogueSequenceNo));

                // 선택지 활성화
                for (int i = 0; i < options.Count; i++)
                {
                    npcChoiceButtons[i].SetActive(true);
                    npcChoiceTexts[i].text = options[i].text;
                }

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

                // 상호작용 입력 활성화
                player._input.interactEnable = true;
            }

            dialogueSequenceNo++;
        }

        isEnd = dialogueSequenceNo > dialogueLastSequenceNo ? true : false;
    }

    private bool TryGetNextDialogue(out Dialogue dialogue)
    {
        DialogueCollection dialogues = new DialogueCollection(dialogueCollection.Where(dialogue => dialogue.sequenceNo == this.dialogueSequenceNo));

        if(dialogues.Count > 0)
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

    public void InteractChooseOption(int choiceNo)
    {
        dialogueSequenceSubNo = choiceNo;
        UpdateInteractionUI(isEnd: out bool isEnd);
    }

    public void MoveGameObject(GameObject gameObject, Vector3 vector)
    {
        gameObject.transform.position = vector;
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            DefaultUIUpadate();
        }
    }

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;

    private void DefaultUIUpadate()
    {
        playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
        playerConfusionBar.localScale = new Vector3((float)player.currentConfusion / player.maxConfusion, 1, 1);
    }
}
