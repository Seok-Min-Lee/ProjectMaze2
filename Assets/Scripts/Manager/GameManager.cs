using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    private void Awake()
    {
        systemManager = new SystemManager();
        // DB ������ �ε� �߰�
        // systemManager.GetDbData();
        ActivateMinimap(isActive: minimapVisible);
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
        minimapVisible = isActive;
        minimap.SetActive(minimapVisible);
        minimapCamera.SetActive(minimapVisible);
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
        if (minimapVisible)
        {
            minimapMarkerPoint = player.transform.position;
            minimapMarkerPoint.y = 0;

            minimapMarker.transform.position = minimapMarkerPoint;

            minimapCameraPoint = minimapMarkerPoint;
            minimapCameraPoint.y = minimapCamera.transform.position.y;
            minimapCamera.transform.position = minimapCameraPoint;
        }
    }

    // ���� �� ���� ��
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
}
