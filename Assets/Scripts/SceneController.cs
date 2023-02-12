using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;
    public Animator transition;

    private void Awake()
    {
        // Checks to make sure only 1 instance exists
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadScene(int sceneIndex)
    {
        instance.StartCoroutine(LoadWithCrossFade(sceneIndex));
    }

    private static IEnumerator LoadWithCrossFade(int sceneIndex)
    {
        instance.transition.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1);

        SceneManager.LoadSceneAsync(sceneIndex);
    }
}