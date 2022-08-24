using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValueManager
{
    public const float PLAYER_MOVE_SPEED_DEFAULT = 4f;  // 플레이어 이동 속도 기본값
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 10f;   // 플레이어 달리기 속도 기본값
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'중독' 상태 해제를 위한 미입력 시간
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 1.0f;   //'공포' 스택 업데이트 시간
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '공포' 발현 시 지속 시간

    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // 플레이어 이동속도 증가 비율

    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 5;  // 플레이어 방어 최대 스택
    public const int PLAYER_POISON_STACK_MAX = 5;   // 독 최대 스택
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // 독 스택당 도트 데미지
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '공포' 스택 최대값
}
