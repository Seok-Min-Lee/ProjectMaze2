using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject followCamera, backMirrorCamera, npcInteractionCamera;
    public GameObject NormalPanel, InteractPanel;

    private void Awake()
    {
        SystemManager systemManager = new SystemManager();
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

    public void UpdateUI(bool isInteract)
    {
        if (isInteract)
        {
            followCamera.gameObject.SetActive(false);
            npcInteractionCamera.gameObject.SetActive(true);

            NormalPanel.SetActive(false);
            InteractPanel.SetActive(true);
        }
        else
        {
            followCamera.gameObject.SetActive(true);
            npcInteractionCamera.gameObject.SetActive(false);

            NormalPanel.SetActive(true);
            InteractPanel.SetActive(false);
        }
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
