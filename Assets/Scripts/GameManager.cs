using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    const int PLAYER_POISON_STACK_MAX = 5, PLAYER_POISON_TIC_DAMAGE = 2;    // �� �ִ� ����, �� ���ô� ��Ʈ ������
    const float PLAYER_ACTIVATE_TIME_DETOX = 5.0f;     //'�ߵ�' ���� ������ ���� ���Է� �ð�
    const float PLAYER_ACTIVATE_TIME_CONFUSION = 1.0f, PLAYER_DURATION_CONFUSION = 5.0f;  // '����' ���� ������Ʈ �ð�, '����' ���� �� ���� �ð�

    public void Initialize(
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
