using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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