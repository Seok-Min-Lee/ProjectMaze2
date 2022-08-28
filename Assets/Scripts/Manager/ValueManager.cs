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

    public const float MONSTER_TURN_BACK_ANGLE = 180f; // 몬스터 뒤돌기 각도
    public const float MONSTER_DESTORY_DELAY = 1f;   // 몬스터 소멸까지 걸리는 시간

    public const float PLAYER_MOVE_SPEED_DEFAULT = 5f;  // 플레이어 이동 속도 기본값
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 11f;   // 플레이어 달리기 속도 기본값
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'중독' 상태 해제를 위한 미입력 시간
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 0.5f;   //'공포' 스택 업데이트 시간
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '공포' 발현 시 지속 시간
    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // 플레이어 이동속도 증가 비율
    public const float PLAYER_CALIBRATION_RESPAWN_Y = 10f;  // 플레이어 리스폰 Y 좌표 보정값.

    public const float TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND = 20.0f; // 1초간 매쉬 업데이트 횟수, 이것의 역수가 업데이트 인터벌 시간
    public const float TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY = 3f;  // Fade 방향 전환 대기 시간.
    public const float TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y = -0.1f;   // Mach Pair 생성시 바닥 충돌로 인한 랜더링 오류 방지를 위한 보정값
    
    public const float TRAP_LIFT_CALIBRATION_START_POSITION_Y = -0.1f;   // Lift 생성시 바닥 충돌로 인한 랜더링 오류 방지를 위한 보정값
    public const float TRAP_LIFT_DERECTION_CHANGE_DELAY = 5f;  // 리프트 방향 전환 대기 시간.

    public const float TIME_SCALE_PASUE = 0f;   // 게임 일시정지 설정을 위한 타임 스케일 값
    public const float TIME_SCALE_PLAY = 1.0f;  // 일시정지 해제 후 설정할 타임 스케일값

    public const float INGAME_PREFERENCE_BGM_VOLUME_MAX = 0f;    // 출력가능한 소리의 범위는 -80~20 이지만 음질을 고려하여 -40~0 내에서만 제어한다.
    public const float INGAME_PREFERENCE_BGM_VOLUME_MIN = -40f;  
    public const float INGAME_PREFERNECE_BGM_VOLUME_MUTE = -80f; // 설정한 값이 최소값이면 뮤트 처리를 하기 위해 -80으로 변환한다.
    public const float INGAME_PREFERENCE_SE_VOLUME_MAX = 0f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MIN = -40f;
    public const float INGAME_PREFERNECE_SE_VOLUME_MUTE = -80f;

    public const string PREFIX_PLAYER_LIFE = "X ";  // UI 에서 플레이어 라이프 옆에 오는 표시
    public const string PREFIX_PLAYER_EFFECT_STACK = "X ";  // UI 에서 스택을 표현하기 위한 표시
    
    public const string PROPERY_SKYBOX_ROTATION = "_Rotation";  // 스카이박스 세팅을 위한 스카이박스 프로퍼티
    public const string PROPERY_AUDIO_MIXER_BGM = "BGM";    // 오디오 세팅을 위한 오디오 믹서 프로퍼티 (추가한 그룹명)
    public const string PROPERY_AUDIO_MIXER_EFFECT = "Effect";  // 오디오 세팅을 위한 오디오 믹서 프로퍼티 (추가한 그룹명)

    public const string JSON_PATH_TB_USER = "/Resources/tb_user.json";
    public const string JSON_PATH_TB_NPC = "/Resources/tb_npc.json";
    public const string JSON_PATH_TB_DIALOGUE = "/Resources/tb_dialogue.json";
    public const string JSON_PATH_TB_INGAME_ATTRIBUTE = "/Resources/tb_ingame_attribute.json";
    public const string JSON_PATH_TB_INGAME_PREFERENCE = "/Resources/tb_ingame_preference.json";
    public const string JSON_PATH_TB_GUIDE = "/Resources/tb_guide.json";

    public const string ERROR_MESSAGE_LOGIN_FAIL = "로그인 실패. 로그인 정보를 확인해주시길 바랍니다.";
    public const string ERROR_MESSAGE_MODE_SELECT_FAIL = "이어하기 실패. 저장된 게임 정보가 없습니다.";
    public const string ERROR_MESSAGE_SIGN_UP_OVERLAP = "회원가입 실패. 중복되는 계정이 존재합니다.";
    public const string ERROR_MESSAGE_SIGN_UP_NOT_INPUT = "회원가입 실패. 입력창에 데이터를 입력해주세요.";

    public const string CONFIRM_MESSAGE_SIGN_UP_SUCCESS = "회원가입 완료. 로그인 해주시길 바랍니다.";
}
