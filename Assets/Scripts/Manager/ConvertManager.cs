using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConvertManager
{
    public static int ConvertStringToInt(string input)
    {
        int output;

        if(Int32.TryParse(input, out output))
        {
            return output;
        }
        else
        {
            return -1;
        }
    }

    public static float ConvertStringToFloat(string input)
    {
        float output;

        if(float.TryParse(input, out output))
        {
            return output;
        }
        else
        {
            return -1f;
        }
    }

    public static string ConvertSceneTypeToString(SceneType sceneType)
    {
        string result;

        switch (sceneType)
        {
            case SceneType.Lobby:
                result = NameManager.SCENE_LOBBY;
                break;
            case SceneType.Village:
                result = NameManager.SCENE_VILLAGE;
                break;
            case SceneType.Stage1:
                result = NameManager.SCENE_STAGE_1;
                break;
            case SceneType.Stage2:
                result = NameManager.SCENE_STAGE_2;
                break;
            case SceneType.Stage3:
                result = NameManager.SCENE_STAGE_3;
                break;
            default:
                result = string.Empty;
                break;
        }

        return result;
    }
}
