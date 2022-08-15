public static class NameManager
{
    public const string JSON_COLUMN_ACCOUNT = "account";
    public const string JSON_COLUMN_PASSWORD = "password";
    public const string JSON_COLUMN_ID = "id";
    public const string JSON_COLUMN_NAME = "name";
    public const string JSON_COLUMN_NPC_ID = "npcid";
    public const string JSON_COLUMN_NPC_NAME = "npcname";
    public const string JSON_COLUMN_SITUATION_NO = "situationno";
    public const string JSON_COLUMN_SEQUENCE_NO = "sequenceno";
    public const string JSON_COLUMN_SEQUENCE_SUB_NO = "sequencesubno";
    public const string JSON_COLUMN_DIALOGUE_TYPE = "dialoguetype";
    public const string JSON_COLUMN_TEXT = "text";

    public const string SCENE_LOBBY = "Lobby";
    public const string SCENE_VILLAGE = "Village";
    public const string SCENE_STAGE_1 = "Stage 1";
    public const string SCENE_STAGE_2 = "Stage 2";
    public const string SCENE_STAGE_3 = "Stage 3";

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
    public const string TAG_PORTAL = "Portal";

    public const string LAYER_PLAYER = "Player";

    public const string ANIMATION_PARAMETER_RUN_FORWARD = "Run Forward";
    public const string ANIMATION_PARAMETER_STAB_ATTACK = "Stab Attack";
    public const string ANIMATION_PARAMETER_DO_ATTACK = "DoAttack";
    public const string ANIMATION_PARAMETER_DO_DIE = "DoDie";

    public const string NPC_NAME_HUMAN_GATEKEEPER = "문지기";
    public const string NPC_NAME_FAIRY_CHILD = "소인족 아이";
    public const string NPC_NAME_FAIRY_ADULT = "소인족 청년";
    public const string NPC_NAME_GIANT_STONE_STATUE = "거인족 석상";
    public const string NPC_NAME_GIANT_TWIN_A = "쌍둥이 거인 A";
    public const string NPC_NAME_GIANT_TWIN_B = "쌍둥이 거인 B";
    public const string NPC_NAME_HUMAN_CHILD = "인간 아이";
    public const string NPC_NAME_HUMAN_ADULT = "인간 청년";
    public const string NPC_NAME_NONAME = "인게임 NPC";

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
            case NpcType.FairyAdult:
                name = NPC_NAME_FAIRY_ADULT;
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
            case NpcType.HumanChild:
                name = NPC_NAME_HUMAN_CHILD;
                break;
            case NpcType.HumanAdult:
                name = NPC_NAME_HUMAN_ADULT;
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