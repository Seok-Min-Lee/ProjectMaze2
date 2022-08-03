public static class NameManager
{
    public const string TAG_PLAYER = "Player";
    public const string TAG_ITEM = "Item";
    public const string TAG_PLAYER_RESPAWN = "PlayerRespawn";
    public const string TAG_FALL = "Fall";
    public const string TAG_MONSTER = "Monster";
    public const string TAG_MONSTER_ATTACK = "MonsterAttack";
    public const string TAG_NEAGTIVE_EFFECT = "NegativeEffect";
    public const string TAG_MONSTER_TURN_BACK_AREA= "MonsterTurnBackArea";
    public const string TAG_TRAP_ACTIVATOR = "TrapActivator";
    public const string TAG_TRAP_DEACTIVATOR = "TrapDeactivator";
    public const string TAG_TRAP = "Trap";
    public const string TAG_NPC = "NPC";
    public const string TAG_NPC_INTERACTION_ZONE = "NpcInteractionZone";

    public const string LAYER_PLAYER = "Player";

    public const string ANIMATION_PARAMETER_RUN_FORWARD = "Run Forward";
    public const string ANIMATION_PARAMETER_STAB_ATTACK = "Stab Attack";
    public const string ANIMATION_PARAMETER_DO_ATTACK = "DoAttack";
    public const string ANIMATION_PARAMETER_DO_DIE = "DoDie";

    public const string NPC_NAME_HUMAN_GATEKEEPER = "������";
    public const string NPC_NAME_FAIRY_CHILD = "���� ����";
    public const string NPC_NAME_FAIRY_OLD_MAN = "���� ���";
    public const string NPC_NAME_GIANT_STONE_STATUE = "���� ����";
    public const string NPC_NAME_GIANT_TWIN_A = "�ֵ��� ���� A";
    public const string NPC_NAME_GIANT_TWIN_B = "�ֵ��� ���� B";
    public const string NPC_NAME_HUMAN_YOUNG_MAN = "�ΰ� û��";
    public const string NPC_NAME_HUMAN_OLD_MAN = "�ΰ� ����";
    public const string NPC_NAME_NONAME = "�ΰ��� NPC";

    public static bool TryConvertNpcTypeToName(NpcType type, out string name)
    {
        name = string.Empty;

        switch (type)
        {
            case NpcType.HumanGatekeeper:
                name = NPC_NAME_HUMAN_GATEKEEPER;
                break;
            case NpcType.FairyChild:
                name = NPC_NAME_FAIRY_CHILD;
                break;
            case NpcType.FairyOldMan:
                name = NPC_NAME_FAIRY_OLD_MAN;
                break;
            case NpcType.GiantStoneStatue:
                name = NPC_NAME_GIANT_STONE_STATUE;
                break;
            case NpcType.GiantTwinA:
                name = NPC_NAME_GIANT_TWIN_A;
                break;
            case NpcType.GiantTwinB:
                name = NPC_NAME_GIANT_TWIN_B;
                break;
            case NpcType.HumanYoungMan:
                name = NPC_NAME_HUMAN_YOUNG_MAN;
                break;
            case NpcType.HumanOldMan:
                name = NPC_NAME_HUMAN_OLD_MAN;
                break;
            case NpcType.Noname:
                name = NPC_NAME_NONAME;
                break;
        }

        if(string.Equals(name, string.Empty))
        {
            return false;
        }
        
        return true;
    }

}