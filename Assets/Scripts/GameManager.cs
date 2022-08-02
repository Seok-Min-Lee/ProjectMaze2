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

    DialogueCollection npcDialogues, npcDialogueOptions;
    int dialogueSituationNo, dialogueSequenceNo;
    public void UpdateUINormalToInteract(bool isInteract, string name = NameManager.NPC_NAME_HUMAN_OLD_MAN, int situationNo = 0)
    {
        if (isInteract)
        {
            // Ȱ��ȭ�� ī�޶� ��ü.
            followCamera.gameObject.SetActive(false);
            npcInteractionCamera.gameObject.SetActive(true);

            // �÷��̾� ���� ����.
            player._input.controlEnable = false;
            Cursor.lockState = CursorLockMode.Confined;

            // UI ���� ������ �ʱ�ȭ.
            dialogueSituationNo = situationNo;
            dialogueSequenceNo = 0;
            npcName.text = name;

            if (systemManager.GetNpcIndexByName(name: name, index: out int npcIndex) &&
                systemManager.GetDialoguesByNpcIndex(index: npcIndex, dialogueCollection: out DialogueCollection dialogues))
            {
                npcDialogues = new DialogueCollection();
                npcDialogues.AddRange(dialogues.Where(dialogue => dialogue.situationNo == dialogueSituationNo && dialogue.type != DialogueType.Option));

                npcDialogueOptions = new DialogueCollection();
                npcDialogueOptions.AddRange(dialogues.Where(dialogue => dialogue.situationNo == dialogueSituationNo && dialogue.type == DialogueType.Option));

                Dialogue dialogue = npcDialogues.ElementAt(dialogueSequenceNo++);
                npcDialogue.text = dialogue.text;
                interactChoicePanel.SetActive(dialogue.type == DialogueType.Question? true : false);
            }

            // Ȱ��ȭ�� UI ��ü.
            NormalPanel.SetActive(false);
            InteractPanel.SetActive(true);
        }
        else
        {
            // Ȱ��ȭ�� ī�޶� ��ü.
            followCamera.gameObject.SetActive(true);
            npcInteractionCamera.gameObject.SetActive(false);

            // �÷��̾� ���� ����.
            player._input.controlEnable = true;
            Cursor.lockState = CursorLockMode.Locked;

            // Ȱ��ȭ�� UI ��ü.
            NormalPanel.SetActive(true);
            InteractPanel.SetActive(false);
        }
    }

    public void UpdateInteractionUI(out bool isEnd)
    {
        Dialogue dialogue;
        if(dialogueSequenceNo < npcDialogues.Count)
        {
            dialogue = npcDialogues.FirstOrDefault(npcDialogue => npcDialogue.sequenceNo == dialogueSequenceNo);

            if (dialogue.type == DialogueType.Question)
            {
                interactChoicePanel.SetActive(true);

                List<Dialogue> options = new List<Dialogue>();
                options.AddRange(npcDialogueOptions.Where(option => option.sequenceNo == dialogueSequenceNo));

                for(int i = 0; i < options.Count; i++)
                {
                    npcChoiceButtons[i].SetActive(true);
                    npcChoiceTexts[i].text = options[i].text;
                }

                player._input.interactEnable = false;
            }
            else
            {
                interactChoicePanel.SetActive(false);
                for (int i = 0; i < npcChoiceButtons.Length; i++)
                {
                    npcChoiceButtons[i].SetActive(false);
                }

                player._input.interactEnable = true;
            }

            dialogueSequenceNo++;
            npcDialogue.text = dialogue.text;

            isEnd = false;
        }
        else
        {
            isEnd = true;
        }
    }

    public void InteractChooseOption(int choiceNo)
    {
        dialogueSequenceNo += choiceNo;
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
