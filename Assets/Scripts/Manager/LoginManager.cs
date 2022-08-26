using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogInManager : MonoBehaviour
{
    const string ERROR_MESSAGE_LOGIN_FAIL = "�α��� ����. �α��� ������ Ȯ�����ֽñ� �ٶ��ϴ�.";
    const string ERROR_MESSAGE_MODE_SELECT_FAIL = "�̾��ϱ� ����. ����� ���� ������ �����ϴ�.";

    public GameObject inputPanel, modePanel;
    public Text inputAccount, inputPassword, failureText;

    private void Start()
    {
        inputPanel.SetActive(true);
        modePanel.SetActive(false);

        failureText.text = string.Empty;
    }

    public void OnClickExit()
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
            StartCoroutine(AlertMessage(text: ERROR_MESSAGE_LOGIN_FAIL));
        }
    }

    public void OnClickNewGame()
    {
        SystemManager.instance.ingameAttributes.Clear();
        LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
    }

    public void OnClickContinueGame()
    {
        if (SystemManager.instance.ingameAttributes.Count > 0)
        {
            LoadingSceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
        }
        else
        {
            StopCoroutine(AlertMessage(text: string.Empty));
            StartCoroutine(AlertMessage(text: ERROR_MESSAGE_MODE_SELECT_FAIL));
        }
    }

    IEnumerator AlertMessage(string text)
    {
        failureText.text = text;

        yield return new WaitForSeconds(3f);

        failureText.text = string.Empty;
    }
}
