public static class ValueManager
{
    public const int PLAYER_HP_MAX = 100;   // �ִ� HP
    public const int PLAYER_LIFE_MAX = 9;   // �ִ� Life ����
    public const int PLAYER_KEY_MAX = 9;    // �ִ� ���� ���� ����
    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 3;  // �÷��̾� ��� �ִ� ����
    public const int PLAYER_POISON_STACK_MAX = 5;   // �� �ִ� ����
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // �� ���ô� ��Ʈ ������
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '����' ���� �ִ밪
    
    public const int TRAP_MACH_PAIR_FAIL_DAMAGE = 33;   // ���� ¦ ���߱� ���н� ������

    public const int TUTORIAL_PLAYER_SETTING_CURRENT_LIFE = 0;  // Ʃ�丮�� ���۽� �÷��̾� ������
    public const int TUTORIAL_PLAYER_SETTING_CURRENT_HP = 50;   // Ʃ�丮�� ���۽� �÷��̾� HP

    public const float ITEM_ROTATION_SPEED = 60f;   // ������ ���� �ӵ�

    public const float MONSTER_TURN_BACK_ANGLE = 180f; // ���� �ڵ��� ����
    public const float MONSTER_DESTORY_DELAY = 0.5f;   // ���� �Ҹ���� �ɸ��� �ð�

    public const float MONSTER_INSECT_ATTACK_AREA_ACTIVATE_DELAY = 0.2f;        // ���� ���� Ȱ��ȭ���� ���ð�
    public const float MONSTER_INSECT_ATTACK_AREA_DEACTIVATE_DELAY = 0.2f;      // ���� ���� ���� ��Ȱ��ȭ���� ���ð�
    public const float MONSTER_INSECT_ATTACK_AREA_DEACTIVATE_AFTER_DELAY = 0.6f;// ���� ���� ���ݱ��� ���ð�
    public const float MONSTER_INSECT_SUICIDE_ANIMATION_BEFORE_DELAY = 1.3f;    // Suicide ���� �� �ִϸ��̼� �߻����� ���ð�
    public const float MONSTER_INSECT_SUICIDE_ANIMATION_AFTER_DELAY = 1.2f;     // ���� ���߱��� ���ð�

    // ���� ĳ����Ʈ
    public const float MONSTER_CATAPULT_PROJECTILE_GAIN_POWER_TIME = 2f;    // ����ü ����ð�
    public const float MONSTER_CATAPULT_PROJECTILE_ANGULAR_POWER = 1f;      // ����ü ��� �ʱⰪ
    public const float MONSTER_CATAPULT_PROJECTILE_SCALE_VALUE = 0.05f;     // ����ü ������ �ʱⰪ
    public const float MONSTER_CATAPULT_PROJECTILE_ANGULAR_POWER_INCREMENT_VALUE = 0.1f;    // ����ü ��� ���尪
    public const float MONSTER_CATAPULT_PROJECTILE_SCALE_VALUE_INCREMENT_VALUE = 0.02f;     // ����ü ������ ���尪

    public const float PLAYER_MOVE_SPEED_DEFAULT = 11f;  // �÷��̾� �̵� �ӵ� �⺻��
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 33f;   // �÷��̾� �޸��� �ӵ� �⺻��
    public const float PLAYER_POISON_TIC_DELAY = 1f;       // �÷��̾� �ߵ� ������ ������
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'�ߵ�' ���� ������ ���� ���Է� �ð�
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 0.5f;   //'����' ���� ������Ʈ �ð�
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '����' ���� �� ���� �ð�
    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // �÷��̾� �̵��ӵ� ���� ����
    public const float PLAYER_CALIBRATION_RESPAWN_Y = 10f;  // �÷��̾� ������ Y ��ǥ ������.

    public const float TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND = 20.0f; // 1�ʰ� �Ž� ������Ʈ Ƚ��, �̰��� ������ ������Ʈ ���͹� �ð�
    public const float TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY = 3f;  // Fade ���� ��ȯ ��� �ð�.
    public const float TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y = -0.1f;   // Mach Pair ������ �ٴ� �浹�� ���� ������ ���� ������ ���� ������
    
    public const float TRAP_LIFT_CALIBRATION_START_POSITION_Y = -0.1f;   // Lift ������ �ٴ� �浹�� ���� ������ ���� ������ ���� ������
    public const float TRAP_LIFT_DERECTION_CHANGE_DELAY = 5f;  // ����Ʈ ���� ��ȯ ��� �ð�.

    public const float TIME_SCALE_PASUE = 0f;   // ���� �Ͻ����� ������ ���� Ÿ�� ������ ��
    public const float TIME_SCALE_PLAY = 1.0f;  // �Ͻ����� ���� �� ������ Ÿ�� �����ϰ�

    public const float INGAME_PREFERENCE_BGM_VOLUME_MAX = 0f;    // ��°����� �Ҹ��� ������ -80~20 ������ ������ ����Ͽ� -40~0 �������� �����Ѵ�.
    public const float INGAME_PREFERENCE_BGM_VOLUME_MIN = -40f;  
    public const float INGAME_PREFERENCE_BGM_VOLUME_MUTE = -80f; // ������ ���� �ּҰ��̸� ��Ʈ ó���� �ϱ� ���� -80���� ��ȯ�Ѵ�.
    public const float INGAME_PREFERENCE_BGM_VOLUME_DEFAULT = -10f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MAX = 0f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MIN = -40f;
    public const float INGAME_PREFERENCE_SE_VOLUME_MUTE = -80f;
    public const float INGAME_PREFERENCE_SE_VOLUME_DEFAULT = -10f;

    public const float MESSAGE_DISPLAY_DURATION = 2f;   // �ý��� �޼��� ���ӽð�

    // �ý��� �޼���
    public const string MESSAGE_PLAYER_ADDICT = "�ߵ�";
    public const string MESSAGE_PLAYER_DETOX = "�ص�";
    public const string MESSAGE_PLAYER_CONFUSE = "����";
    public const string MESSAGE_PLAYER_CALM_DOWN = "���� ����";
    public const string MESSAGE_PREFIX_ITEM = "������ [ ";
    public const string MESSAGE_SUFFIX_ITEM = " ] ȹ��";
    public const string MESSAGE_TRAP_MACH_PAIR_SUCCESS = "¦ ���߱� ����!";
    public const string MESSAGE_TRAP_MACH_PAIR_FAILURE = "¦ ���߱� ����!";
    //public const string MESSAGE_TRAP_TRAFFIC_LIGHT_SUCCESS = "��ȣ�� ���!";
    public const string MESSAGE_TRAP_TRAFFIC_LIGHT_FAILURE = "��ȣ�� �ؼ� �սô�!";
    public const string MESSAGE_PORTAL_FAIL = "������ ȹ���ؾ� ���� ������ �� �� �ֽ��ϴ�.";

    public const string PREFIX_PLAYER_LIFE = "X ";  // UI ���� �÷��̾� ������ ���� ���� ǥ��
    public const string PREFIX_PLAYER_KEY = "x ";  // UI ���� �÷��̾� ���� Key ���� ���� ǥ��
    public const string PREFIX_PLAYER_EFFECT_STACK = "X ";  // UI ���� ������ ǥ���ϱ� ���� ǥ��

    public const string SUFFIX_VERSION = "ver ";
    
    public const string PROPERY_SKYBOX_ROTATION = "_Rotation";  // ��ī�̹ڽ� ������ ���� ��ī�̹ڽ� ������Ƽ
    public const string PROPERY_AUDIO_MIXER_BGM = "BGM";    // ����� ������ ���� ����� �ͼ� ������Ƽ (�߰��� �׷��)
    public const string PROPERY_AUDIO_MIXER_EFFECT = "Effect";  // ����� ������ ���� ����� �ͼ� ������Ƽ (�߰��� �׷��)

    // JSON ���� ���
    public const string JSON_PATH_TB_USER = "/Resources/tb_user.json";
    public const string JSON_PATH_TB_NPC = "/Resources/tb_npc.json";
    public const string JSON_PATH_TB_DIALOGUE = "/Resources/tb_dialogue.json";
    public const string JSON_PATH_TB_INGAME_ATTRIBUTE = "/Resources/tb_ingame_attribute.json";
    public const string JSON_PATH_TB_INGAME_PREFERENCE = "/Resources/tb_ingame_preference.json";
    public const string JSON_PATH_TB_GUIDE = "/Resources/tb_guide.json";

    // �α��� ȭ�� �ý��� �޼���
    public const string ERROR_MESSAGE_LOGIN_FAIL = "�α��� ����. �α��� ������ Ȯ�����ֽñ� �ٶ��ϴ�.";
    public const string ERROR_MESSAGE_NEW_GAME_FAIL = "���� ���� ����. Ʃ�丮���� ���� �������ּ���.";
    public const string ERROR_MESSAGE_MODE_CONTINUE_GAME_FAIL = "�̾��ϱ� ����. ����� ���� ������ �����ϴ�.";
    public const string ERROR_MESSAGE_SIGN_UP_OVERLAP = "ȸ������ ����. �ߺ��Ǵ� ������ �����մϴ�.";
    public const string ERROR_MESSAGE_SIGN_UP_NOT_INPUT = "ȸ������ ����. �Է�â�� �����͸� �Է����ּ���.";

    public const string CONFIRM_MESSAGE_SIGN_UP_SUCCESS = "ȸ������ �Ϸ�. �α��� ���ֽñ� �ٶ��ϴ�.";

    // �ΰ��� ��ȣ�ۿ� ���� �޼���
    public const string INTERACTABLE_ALRAM_TEXT_DIALOGUE = "��ȭ�ϱ�";
    public const string INTERACTABLE_ALRAM_TEXT_GUIDE = "���̵� ����";
}
