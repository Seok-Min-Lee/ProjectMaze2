using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    const string LOGIN_FAILURE_MESSAGE = "로그인 실패. 로그인 정보를 확인해주시길 바랍니다.";
    const string MODE_FAILURE_MESSAGE = "이어하기 실패. 저장된 게임 정보가 없습니다.";

    public GameObject inputPanel, modePanel;
    public Text inputAccount, inputPassword, failureText;

    private void Awake()
    {
    }

    private void Start()
    {
        inputPanel.SetActive(true);
        modePanel.SetActive(false);
    }

    public void OnClickExit()
    {

    }

    public void OnClickSubmit()
    {
        string userAccount = inputAccount.text;
        string userPassword = inputPassword.text;

        if(SystemManager.instance.TryLogin(account: userAccount, password: userPassword))
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
        Debug.Log("새 게임을 시작합니다.");
        // 인게임 속성들 제거 - 추가
        SceneManager.LoadScene(sceneName: NameManager.SCENE_VILLAGE);
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
