public static class ValueManager
{
    public const int PLAYER_HP_MAX = 100;   // �ִ� HP
    public const int PLAYER_LIFE_MAX = 5;   // �ִ� Life ����
    public const int PLAYER_MAGIC_GIANT_STACK_MAX = 3;  // �÷��̾� ��� �ִ� ����
    public const int PLAYER_POISON_STACK_MAX = 5;   // �� �ִ� ����
    public const int PLAYER_POISON_TIC_DAMAGE = 2;    // �� ���ô� ��Ʈ ������
    public const int PLAYER_CONFUSION_STACK_MAX = 100;  // '����' ���� �ִ밪
    
    public const int TRAP_MACH_PAIR_FAIL_DAMAGE = 33;

    public const float ITEM_ROTATION_SPEED = 60f;   // ������ ���� �ӵ�

    public const float MONSTSER_TURN_BACK_ANGLE = 180f; // ���� �ڵ��� ����

    public const float PLAYER_MOVE_SPEED_DEFAULT = 4f;  // �÷��̾� �̵� �ӵ� �⺻��
    public const float PLAYER_SPRINT_SPEED_DEFAULT = 30f;   // �÷��̾� �޸��� �ӵ� �⺻��
    public const float PLAYER_DETOX_ACTIVATE_TIME = 5.0f;     //'�ߵ�' ���� ������ ���� ���Է� �ð�
    public const float PLAYER_CONFUSION_STACK_UPDATE_TIME = 0.5f;   //'����' ���� ������Ʈ �ð�
    public const float PLAYER_CONFUSION_DURATION = 5.0f;  // '����' ���� �� ���� �ð�
    public const float PLAYER_MAGIC_SPEED_RATIO = 1.5f; // �÷��̾� �̵��ӵ� ���� ����

    public const float TRAP_FADE_IN_AND_OUT_UPDATE_COUNT_PER_ONE_SECOND = 20.0f; // 1�ʰ� �Ž� ������Ʈ Ƚ��, �̰��� ������ ������Ʈ ���͹� �ð�
    public const float TRAP_FADE_IN_AND_OUT_FADE_CHANGE_DELAY = 3f;  // Fade ���� ��ȯ ��� �ð�.
    public const float TRAP_MACH_PAIR_CALIBRATION_START_POSITION_Y = -0.1f;   // Mach Pair ������ �ٴ� �浹�� ���� ������ ���� ������ ���� ������
    
    public const float TRAP_LIFT_CALIBRATION_START_POSITION_Y = -0.1f;   // Lift ������ �ٴ� �浹�� ���� ������ ���� ������ ���� ������
    public const float TRAP_LIFT_DERECTION_CHANGE_DELAY = 5f;  // ����Ʈ ���� ��ȯ ��� �ð�.

    public const float TIME_SCALE_PASUE = 0f;   // ���� �Ͻ����� ������ ���� Ÿ�� ������ ��
    public const float TIME_SCALE_PLAY = 1.0f;  // �Ͻ����� ���� �� ������ Ÿ�� �����ϰ�

    public const string PREFIX_PLAYER_LIFE = "X ";
    public const string PREFIX_PLAYER_EFFECT_STACK = "X ";
    public const string PROPERY_SKYBOX_ROTATION = "_Rotation";

    public const string JSON_PATH_TB_USER = "/Resources/tb_user.json";
    public const string JSON_PATH_TB_NPC = "/Resources/tb_npc.json";
    public const string JSON_PATH_TB_DIALOGUE = "/Resources/tb_dialogue.json";
    public const string JSON_PATH_TB_INGAME_ATTRIBUTE = "/Resources/tb_ingame_attribute.json";
    public const string JSON_PATH_TB_GUIDE = "/Resources/tb_guide.json";

    public const string ERROR_MESSAGE_LOGIN_FAIL = "�α��� ����. �α��� ������ Ȯ�����ֽñ� �ٶ��ϴ�.";
    public const string ERROR_MESSAGE_MODE_SELECT_FAIL = "�̾��ϱ� ����. ����� ���� ������ �����ϴ�.";
}
