using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValueManager
{
    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 5;  // 플레이어 방어 최대 스택
    public const int PLAYER_POISON_STACK_MAX = 5;   // 독 최대 스택
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // 독 스택당 도트 데미지
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '공포' 스택 최대값

    public const float PLAYER_MOVE_SPEED_DEFAULT = 4f;  // 플레이어 이동 속도 기본값
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 30f;   // 플레이어 달리기 속도 기본값
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'중독' 상태 해제를 위한 미입력 시간
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 0.5f;   //'공포' 스택 업데이트 시간
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '공포' 발현 시 지속 시간
    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // 플레이어 이동속도 증가 비율

    public const float TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND = 20.0f; // 1초간 매쉬 업데이트 횟수, 이것의 역수가 업데이트 인터벌 시간
    public const float TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY = 3f;  // Fade In 과 Out 완료 후 대기 시간.

    public const float TIME_SCALE_PASUE = 0f;
    public const float TIME_SCALE_PLAY = 1.0f;

    public const string PREFIX_PLAYER_LIFE = "X ";
    public const string PREFIX_PLAYER_EFFECT_STACK = "X ";
    public const string PROPERY_SKYBOX_ROTATION = "_Rotation";

    public const string JSON_PATH_TB_USER = "/Resources/tb_user.json";
    public const string JSON_PATH_TB_NPC = "/Resources/tb_npc.json";
    public const string JSON_PATH_TB_DIALOGUE = "/Resources/tb_dialogue.json";
    public const string JSON_PATH_TB_INGAME_ATTRIBUTE = "/Resources/tb_ingame_attribute.json";
    public const string JSON_PATH_TB_GUIDE = "/Resources/tb_guide.json";
}
