public static class ValueManager
{
    public const int PLAYER_HP_MAX = 100;   // 최대 HP
    public const int PLAYER_LIFE_MAX = 5;   // 최대 Life 개수
    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 3;  // 플레이어 방어 최대 스택
    public const int PLAYER_POISON_STACK_MAX = 5;   // 독 최대 스택
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // 독 스택당 도트 데미지
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '공포' 스택 최대값
    
    public const int TRAP_MACH_PAIR_FAIL_DAMAGE = 33;

    public const float ITEM_ROTATION_SPEED = 60f;   // 아이템 공전 속도

    public const float MONSTSER_TURN_BACK_ANGLE = 180f; // 몬스터 뒤돌기 각도

    public const float PLAYER_MOVE_SPEED_DEFAULT = 4f;  // 플레이어 이동 속도 기본값
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 30f;   // 플레이어 달리기 속도 기본값
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'중독' 상태 해제를 위한 미입력 시간
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 0.5f;   //'공포' 스택 업데이트 시간
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '공포' 발현 시 지속 시간
    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // 플레이어 이동속도 증가 비율

    public const float TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND = 20.0f; // 1초간 매쉬 업데이트 횟수, 이것의 역수가 업데이트 인터벌 시간
    public const float TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY = 3f;  // Fade 방향 전환 대기 시간.
    public const float TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y = -0.1f;   // Mach Pair 생성시 바닥 충돌로 인한 랜더링 오류 방지를 위한 보정값
    
    public const float TRAP_LIFT_CALIBRATION_START_POSITION_Y = -0.1f;   // Lift 생성시 바닥 충돌로 인한 랜더링 오류 방지를 위한 보정값
    public const float TRAP_LIFT_DERECTION_CHANGE_DELAY = 5f;  // 리프트 방향 전환 대기 시간.

    public const float TIME_SCALE_PASUE = 0f;   // 게임 일시정지 설정을 위한 타임 스케일 값
    public const float TIME_SCALE_PLAY = 1.0f;  // 일시정지 해제 후 설정할 타임 스케일값

    public const string PREFIX_PLAYER_LIFE = "X ";
    public const string PREFIX_PLAYER_EFFECT_STACK = "X ";
    public const string PROPERY_SKYBOX_ROTATION = "_Rotation";

    public const string JSON_PATH_TB_USER = "/Resources/tb_user.json";
    public const string JSON_PATH_TB_NPC = "/Resources/tb_npc.json";
    public const string JSON_PATH_TB_DIALOGUE = "/Resources/tb_dialogue.json";
    public const string JSON_PATH_TB_INGAME_ATTRIBUTE = "/Resources/tb_ingame_attribute.json";
    public const string JSON_PATH_TB_GUIDE = "/Resources/tb_guide.json";

    public const string ERROR_MESSAGE_LOGIN_FAIL = "로그인 실패. 로그인 정보를 확인해주시길 바랍니다.";
    public const string ERROR_MESSAGE_MODE_SELECT_FAIL = "이어하기 실패. 저장된 게임 정보가 없습니다.";
}
