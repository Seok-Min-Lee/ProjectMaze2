using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LogInManager : MonoBehaviour
{ 
    public GameObject inputPanel, modePanel;
    public Text inputAccount, inputPassword, messageText, versionText;
    public Toggle bgmToggle;
    public AudioMixer masterMixer;

    float bgmVolume;

    private void Start()
    {
        this.inputPanel.SetActive(true);
        this.modePanel.SetActive(false);

        this.messageText.text = string.Empty;
        TryGetBgmVolumeByIngamePreferences(preferences: SystemManager.instance.ingamePreferences, volume: out this.bgmVolume);

        InitAudioMixer();

        versionText.text = ValueManager.SUFFIX_VERSION + Application.version;

        Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnClickQuit()
    {
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void OnClickSignUp()
    {
        string userAccount = inputAccount.text;
        string userPassword = inputPassword.text;

        if(!string.IsNullOrEmpty(userAccount) && !string.IsNullOrEmpty(userPassword))
        {
            if (!SystemManager.instance.IsExistAccount(account: userAccount))
            {
                SystemManager.instance.SignUpUser(account: userAccount, password: userPassword);

                StopCoroutine(AlertMessage(text: string.Empty, color: Color.red));
                StartCoroutine(AlertMessage(text: ValueManager.CONFIRM_MESSAGE_SIGN_UP_SUCCESS, color: Color.blue, target: this.messageText));
            }
            else
            {
                // 이미 존재하는 계정
                StopCoroutine(AlertMessage(text: string.Empty, color: Color.red));
                StartCoroutine(AlertMessage(text: ValueManager.ERROR_MESSAGE_SIGN_UP_OVERLAP, color: Color.red, target: this.messageText));
            }
        }
        else
        {
            // 미입력 된경우
            StopCoroutine(AlertMessage(text: string.Empty, color: Color.red));
            StartCoroutine(AlertMessage(text: ValueManager.ERROR_MESSAGE_SIGN_UP_NOT_INPUT, color: Color.red, target: this.messageText));
        }
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
            StopCoroutine(AlertMessage(text: string.Empty, color: Color.red));
            StartCoroutine(AlertMessage(text: ValueManager.ERROR_MESSAGE_LOGIN_FAIL, color: Color.red, target: this.messageText));
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
            StopCoroutine(AlertMessage(text: string.Empty, color: Color.red));
            StartCoroutine(AlertMessage(text: ValueManager.ERROR_MESSAGE_MODE_SELECT_FAIL, color: Color.red, target: this.messageText));
        }
    }

    public void OnValueChangedBgmToggle()
    {
        // isOn 일 때 Mute 로 처리한다.
        if (this.bgmToggle.isOn)
        {
            this.masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_BGM, ValueManager.INGAME_PREFERENCE_BGM_VOLUME_MUTE);
        }
        else
        {
            this.masterMixer.SetFloat(ValueManager.PROPERY_AUDIO_MIXER_BGM, this.bgmVolume);
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

    private bool TryGetBgmVolumeByIngamePreferences(IEnumerable<IngamePreference> preferences, out float volume)
    {
        foreach(IngamePreference preference in preferences)
        {
            if(preference.name == NameManager.INGAME_PREFERENCE_NAME_BGM_VOLUME)
            {
                volume = ConvertManager.ConvertStringToFloat(preference.value);

                return true;
            }
        }

        volume = ValueManager.INGAME_PREFERENCE_BGM_VOLUME_DEFAULT;
        return false;
    }

    private IEnumerator AlertMessage(
        string text, 
        Color color, 
        Text target = null
    )
    {
        if(target != null)
        {
            target.color = color;
            target.text = text;

            yield return new WaitForSeconds(ValueManager.MESSAGE_DISPLAY_DURATION);

            target.text = string.Empty;
        }
    }
}
