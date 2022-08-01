using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager
{
    List<string> npcNames;
    Dictionary<int, string> npcIndexNameDictionary;
    Dictionary<string, int> npcNameIndexDictionary;

    public SystemManager()
    {
        InitializeNpc(
            names: out npcNames, 
            indexNameDictionary: out npcIndexNameDictionary, 
            nameIndexDictionary: out npcNameIndexDictionary
        );
    }

    public bool GetNpcIndexByName(string name, out int index)
    {
        if (npcNameIndexDictionary.TryGetValue(key: name, value: out index))
        {
            return true;
        }

        return false;
    }

    public bool GetNpcNameByIndex(int index, out string name)
    {
        if (npcIndexNameDictionary.TryGetValue(key: index, out name))
        {
            return true;
        }

        return false;
    }

    private void InitializeNpc(out List<string> names, out Dictionary<int, string> indexNameDictionary, out Dictionary<string, int> nameIndexDictionary)
    {
        string[] nameList = {
            NameManager.NPC_NAME_FAIRY_CHILD,
            NameManager.NPC_NAME_FAIRY_OLD_MAN,
            NameManager.NPC_NAME_GIANT_STONE_STATUE,
            NameManager.NPC_NAME_GIANT_TWIN_A,
            NameManager.NPC_NAME_GIANT_TWIN_B,
            NameManager.NPC_NAME_HUMAN_GATEKEEPER,
            NameManager.NPC_NAME_HUMAN_OLD_MAN,
            NameManager.NPC_NAME_HUMAN_YOUNG_MAN,
            NameManager.NPC_NAME_NONAME
        };

        names = new List<string>();
        names.AddRange(nameList);


        indexNameDictionary = new Dictionary<int, string>();
        nameIndexDictionary = new Dictionary<string, int>();
        for (int i = 0; i < names.Count; i++)
        {
            indexNameDictionary.Add(i, npcNames[i]);
            nameIndexDictionary.Add(npcNames[i], i);
        }
    }
}
