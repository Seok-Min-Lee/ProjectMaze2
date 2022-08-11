using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    const string LOGIN_FAILURE_MESSAGE = "�α��� ����. �α��� ������ Ȯ�����ֽñ� �ٶ��ϴ�.";
    const string MODE_FAILURE_MESSAGE = "�̾��ϱ� ����. ����� ���� ������ �����ϴ�.";

    public GameObject inputPanel, modePanel;
    public Text inputAccount, inputPassword, failureText;

    SystemManager systemManager;
    string account, password;

    private void Start()
    {
        systemManager = new SystemManager();
        inputPanel.SetActive(true);
        modePanel.SetActive(false);
    }

    public void OnClickExit()
    {

    }

    public void OnClickSubmit()
    {
        account = inputAccount.text;
        password = inputPassword.text;

        if(systemManager.TryLogin(account: account, password: password))
        {
            inputPanel.SetActive(false);
            modePanel.SetActive(true);
        }
        else
        {
            StopCoroutine(AlertMessage(text: string.Empty));
            StartCoroutine(AlertMessage(text: LOGIN_FAILURE_MESSAGE));
        }
    }

    public void OnClickNewGame()
    {
        Debug.Log("�� ������ �����մϴ�.");
    }

    public void OnClickContinueGame()
    {
        StopAllCoroutines();
        StartCoroutine(AlertMessage(text: MODE_FAILURE_MESSAGE));
    }

    IEnumerator AlertMessage(string text)
    {
        failureText.text = text;

        yield return new WaitForSeconds(3f);

        failureText.text = string.Empty;
    }
}
