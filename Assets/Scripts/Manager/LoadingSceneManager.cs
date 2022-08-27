using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    public RectTransform progressBar;
    
    private void Start()
    {
        progressBar.localScale = new Vector3(0, 1, 1);
        StartCoroutine(LoadScene());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene(NameManager.SCENE_LOADING);
    }

    IEnumerator LoadScene()
    {
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;

            if (op.progress < 0.9f) 
            {
                progressBar.localScale = new Vector3(timer, 1,1);

                if (timer >= op.progress) 
                { 
                    timer = op.progress; 
                } 
            } 
            else 
            {
                progressBar.localScale = new Vector3(op.progress, 1, 1);

                // 너무 빠르게 지나가서 임의로 대기 시간을 잡아둠
                if(timer >= 0.5f)
                {
                    progressBar.localScale = new Vector3(1, 1, 1);

                    op.allowSceneActivation = true;

                    yield break;
                }
            }
        }
    }
}
