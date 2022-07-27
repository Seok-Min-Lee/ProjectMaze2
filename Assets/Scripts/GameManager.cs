using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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

    public Player player;
    public RectTransform playerHpBar, playerConfusionBar;

    private void LateUpdate()
    {
        if (player != null)
        {
            playerHpBar.localScale = new Vector3((float)player.currentHp / player.maxHp, 1, 1);
            playerConfusionBar.localScale = new Vector3((float)player.currentConfusion / player.maxConfusion, 1, 1);
        }
    }
}
