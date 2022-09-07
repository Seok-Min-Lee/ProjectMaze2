public static class ValueManager
{
    public const int PLAYER_HP_MAX = 100;   // 최대 HP
    public const int PLAYER_LIFE_MAX = 9;   // 최대 Life 개수
    public const int PLAYER_KEY_MAX = 9;    // 최대 보유 열쇠 개수
    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 3;  // 플레이어 방어 최대 스택
    public const int PLAYER_POISON_STACK_MAX = 5;   // 독 최대 스택
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // 독 스택당 도트 데미지
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '공포' 스택 최대값
    
    public const int TRAP_MACH_PAIR_FAIL_DAMAGE = 33;   // 함정 짝 맞추기 실패시 데미지

    public const int TUTORIAL_PLAYER_SETTING_CURRENT_LIFE = 0;  // 튜토리얼 시작시 플레이어 라이프
    public const int TUTORIAL_PLAYER_SETTING_CURRENT_HP = 50;   // 튜토리얼 시작시 플레이어 HP

    public const float ITEM_ROTATION_SPEED = 60f;   // 아이템 공전 속도

    public const float MONSTER_TURN_BACK_ANGLE = 180f; // 몬스터 뒤돌기 각도
    public const float MONSTER_DESTORY_DELAY = 0.5f;   // 몬스터 소멸까지 걸리는 시간

    public const float MONSTER_INSECT_ATTACK_AREA_ACTIVATE_DELAY = 0.2f;        // 공격 영역 활성화까지 대기시간
    public const float MONSTER_INSECT_ATTACK_AREA_DEACTIVATE_DELAY = 0.2f;      // 이후 공격 영역 비활성화까지 대기시간
    public const float MONSTER_INSECT_ATTACK_AREA_DEACTIVATE_AFTER_DELAY = 0.6f;// 이후 다음 공격까지 대기시간
    public const float MONSTER_INSECT_SUICIDE_ANIMATION_BEFORE_DELAY = 1.3f;    // Suicide 시작 후 애니메이션 발생까지 대기시간
    public const float MONSTER_INSECT_SUICIDE_ANIMATION_AFTER_DELAY = 1.2f;     // 이후 폭발까지 대기시간

    // 몬스터 캐터펄트
    public const float MONSTER_CATAPULT_PROJECTILE_GAIN_POWER_TIME = 2f;    // 투사체 성장시간
    public const float MONSTER_CATAPULT_PROJECTILE_ANGULAR_POWER = 1f;      // 투사체 운동량 초기값
    public const float MONSTER_CATAPULT_PROJECTILE_SCALE_VALUE = 0.05f;     // 투사체 스케일 초기값
    public const float MONSTER_CATAPULT_PROJECTILE_ANGULAR_POWER_INCREMENT_VALUE = 0.1f;    // 투사체 운동량 성장값
    public const float MONSTER_CATAPULT_PROJECTILE_SCALE_VALUE_INCREMENT_VALUE = 0.02f;     // 투사체 스케일 성장값

    public const float PLAYER_MOVE_SPEED_DEFAULT = 11f;  // 플레이어 이동 속도 기본값
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 33f;   // 플레이어 달리기 속도 기본값
    public const float PLAYER_POISON_TIC_DELAY = 1f;       // 플레이어 중독 데미지 딜레이
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
    public const float INGAME_PREFERENCE_BGM_VOLUME_MUTE = -80f; // 설정한 값이 최소값이면 뮤트 처리를 하기 위해 -80으로 변환한다.
    public const float INGAME_PREFERENCE_BGM_VOLUME_DEFAULT = -10f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MAX = 0f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MIN = -40f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MUTE = -80f;
    public const float INGAME_PREFERENCE_SE_VOLUME_DEFAULT = -10f;

    public const float MESSAGE_DISPLAY_DURATION = 2f;   // 시스템 메세지 지속시간

    // 시스템 메세지
    public const string MESSAGE_PLAYER_ADDICT = "중독";
    public const string MESSAGE_PLAYER_DETOX = "해독";
    public const string MESSAGE_PLAYER_CONFUSE = "공포";
    public const string MESSAGE_PLAYER_CALM_DOWN = "공포 해제";
    public const string MESSAGE_PREFIX_ITEM = "아이템 [ ";
    public const string MESSAGE_SUFFIX_ITEM = " ] 획득";
    public const string MESSAGE_TRAP_MACH_PAIR_SUCCESS = "짝 맞추기 성공!";
    public const string MESSAGE_TRAP_MACH_PAIR_FAILURE = "짝 맞추기 실패!";
    //public const string MESSAGE_TRAP_TRAFFIC_LIGHT_SUCCESS = "신호등 통과!";
    public const string MESSAGE_TRAP_TRAFFIC_LIGHT_FAILURE = "신호를 준수 합시다!";
    public const string MESSAGE_PORTAL_FAIL = "구슬을 획득해야 다음 맵으로 갈 수 있습니다.";

    public const string PREFIX_PLAYER_LIFE = "X ";  // UI 에서 플레이어 라이프 옆에 오는 표시
    public const string PREFIX_PLAYER_KEY = "x ";  // UI 에서 플레이어 보유 Key 옆에 오는 표시
    public const string PREFIX_PLAYER_EFFECT_STACK = "X ";  // UI 에서 스택을 표현하기 위한 표시

    public const string SUFFIX_VERSION = "ver ";
    
    public const string PROPERY_SKYBOX_ROTATION = "_Rotation";  // 스카이박스 세팅을 위한 스카이박스 프로퍼티
    public const string PROPERY_AUDIO_MIXER_BGM = "BGM";    // 오디오 세팅을 위한 오디오 믹서 프로퍼티 (추가한 그룹명)
    public const string PROPERY_AUDIO_MIXER_EFFECT = "Effect";  // 오디오 세팅을 위한 오디오 믹서 프로퍼티 (추가한 그룹명)

    // JSON 파일 경로
    public const string JSON_PATH_TB_USER = "/Resources/tb_user.json";
    public const string JSON_PATH_TB_NPC = "/Resources/tb_npc.json";
    public const string JSON_PATH_TB_DIALOGUE = "/Resources/tb_dialogue.json";
    public const string JSON_PATH_TB_INGAME_ATTRIBUTE = "/Resources/tb_ingame_attribute.json";
    public const string JSON_PATH_TB_INGAME_PREFERENCE = "/Resources/tb_ingame_preference.json";
    public const string JSON_PATH_TB_GUIDE = "/Resources/tb_guide.json";

    // 로그인 화면 시스템 메세지
    public const string ERROR_MESSAGE_LOGIN_FAIL = "로그인 실패. 로그인 정보를 확인해주시길 바랍니다.";
    public const string ERROR_MESSAGE_NEW_GAME_FAIL = "게임 시작 실패. 튜토리얼을 먼저 진행해주세요.";
    public const string ERROR_MESSAGE_MODE_CONTINUE_GAME_FAIL = "이어하기 실패. 저장된 게임 정보가 없습니다.";
    public const string ERROR_MESSAGE_SIGN_UP_OVERLAP = "회원가입 실패. 중복되는 계정이 존재합니다.";
    public const string ERROR_MESSAGE_SIGN_UP_NOT_INPUT = "회원가입 실패. 입력창에 데이터를 입력해주세요.";

    public const string CONFIRM_MESSAGE_SIGN_UP_SUCCESS = "회원가입 완료. 로그인 해주시길 바랍니다.";

    // 인게임 상호작용 가능 메세지
    public const string INTERACTABLE_ALRAM_TEXT_DIALOGUE = "대화하기";
    public const string INTERACTABLE_ALRAM_TEXT_GUIDE = "가이드 보기";
}
