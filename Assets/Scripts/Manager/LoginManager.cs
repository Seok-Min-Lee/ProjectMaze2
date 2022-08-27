using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LogInManager : MonoBehaviour
{ 
    public GameObject inputPanel, modePanel;
    public Text inputAccount, inputPassword, failureText;
    public AudioMixer masterMixer;

    private void Start()
    {
        inputPanel.SetActive(true);
        modePanel.SetActive(false);

        failureText.text = string.Empty;

        InitAudioMixer();
    }

    public void OnClickQuit()
    {
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void OnClickSubmit()
    {
        string userAccount = inputAccount.text;
        string userPassword = inputPassword.text;

        if(SystemManager.instance.TryLogIn(account: userAccount, password: userPassword))
        {
            inputPanel.SetActive(false);
            modePanel.SetActive(true);
        }
        else
        {
            StopCoroutine(AlertMessage(text: string.Empty));
            StartCoroutine(AlertMessage(text: ValueManager.ERROR_MESSAGE_LOGIN_FAIL));
        }
    }

    public void OnClickNewGame()
    {
        SystemManager.instance.DeleteIngameIngameAttributeData();
        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
    }

    public void OnClickContinueGame()
    {
        if (SystemManager.instance.ingameAttributes.Count > 0)
        {
            int sceneIndex;
            string sceneName;

            if (TryGetSavedSceneIndexInIngameAttributes(ingameAttributes: SystemManager.instance.ingameAttributes, sceneIndex: out sceneIndex) &&
                TryGetSceneNameByIngameAttributeSceneIndex(sceneIndex: sceneIndex, sceneName: out sceneName))
            {
                LoadingSceneManager.LoadScene(sceneName: sceneName);
            }
            else
            {
                LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
            }
        }
        else
        {
            StopCoroutine(AlertMessage(text: string.Empty));
            StartCoroutine(AlertMessage(text: ValueManager.ERROR_MESSAGE_MODE_SELECT_FAIL));
        }
    }

    private void InitAudioMixer()
    {
        foreach (IngamePreference preference in SystemManager.instance.ingamePreferences)
        {
            if (preference.name == NameManager.INGAME_PREFERENCE_NAME_BGM_VOLUME)
            {
                masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_BGM, ConvertManager.ConvertStringToFloat(preference.value));
            }
            else if (preference.name == NameManager.INGAME_PREFERENCE_NAME_SE_VOLUME)
            {
                masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_EFFECT, ConvertManager.ConvertStringToFloat(preference.value));
            }
        }
    }

    private bool TryGetSavedSceneIndexInIngameAttributes(IEnumerable<IngameAttribute> ingameAttributes, out int sceneIndex)
    {
        sceneIndex = 0;
        bool enabled = false;
        
        foreach(IngameAttribute ingameAttribute in ingameAttributes)
        {
            if(string.Equals(objA: ingameAttribute.attributeName, objB: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_POSITION_ENABLED))
            {
                enabled = ingameAttribute.value > 0 ? true : false;
            }
            if (string.Equals(objA: ingameAttribute.attributeName, objB: NameManager.INGAME_ATTRIBUTE_NAME_SAVED_SCENE_NUMBER))
            {
                sceneIndex = ingameAttribute.value;
            }
        }

        return enabled;
    }

    private bool TryGetSceneNameByIngameAttributeSceneIndex(int sceneIndex, out string sceneName)
    {
        sceneName = string.Empty;

        switch (sceneIndex)
        {
            case 0:
                sceneName = NameManager.SCENE_LOBBY;
                break;
            case 1:
                sceneName = NameManager.SCENE_VILLAGE;
                break;
            case 2:
                sceneName = NameManager.SCENE_STAGE_1;
                break;
            case 3:
                sceneName = NameManager.SCENE_STAGE_2;
                break;
            case 4:
                sceneName = NameManager.SCENE_STAGE_3;
                break;
            case 5:
                sceneName = NameManager.SCENE_LOADING;
                break;
            default:
                break;
        }

        return !string.IsNullOrEmpty(sceneName);
    }

    private IEnumerator AlertMessage(string text)
    {
        failureText.text = text;

        yield return new WaitForSeconds(3f);

        failureText.text = string.Empty;
    }
}
