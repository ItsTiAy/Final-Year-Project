using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<Player> players;
    public List<Enemy> enemies;

    public Bullet[] bulletTypes = new Bullet[3];
    public SecondaryItem[] secondaryItems = new SecondaryItem[1];

    public Transform bulletContainer;
    public Transform mineContainer;

    public int totalNumLevels;

    public GameObject interLevelScreen;
    public GameObject endScreen;
    public GameObject endlessEndScreen;
    public Transform primaryWeaponContainer;
    public Transform secondaryWeaponContainer;
    public Transform audioContainer;
    public Text weaponDescription;

    public GameObject pauseMenu;
    public static bool isPaused = true;

    public Transform ammoUI;
    public Slider bulletReloadUI;
    public Slider secondaryReloadUI;

    public Text countdown;

    private int numPrimaryWeaponsUnlocked = 1;
    private int numSecondaryWeaponsUnlocked = 1;
    private bool endlessUnlocked;
    private bool canPause = false;
    private int newBulletIndex;
    private int newSecondaryIndex;

    private int endlessScore;
    private int randomSongNum;

    private Coroutine bulletReload;
    private Coroutine secondaryReload;

    public Animator interLevelScreenFade;

    private void Awake()
    {
        PauseGame();

        instance = this;

        players = new List<Player>();
        enemies = new List<Enemy>();

        totalNumLevels = 10;
    }

    private void Start()
    {
        endlessUnlocked = SaveManager.instance.GetSaveData().endlessUnlocked;
        newBulletIndex = SaveManager.instance.GetSaveData().primaryWeaponIndex;
        Debug.Log(newBulletIndex);
        UpdateInterLevelScreenUI(true);

        if (endlessUnlocked)
        {
            endlessScore = 0;
            LevelManager.instance.GenerateLevel();
        }
        else
        {
            Debug.Log("Not endless");
            LevelManager.instance.LoadLevel(SaveManager.instance.GetSaveData().maxLevelNum);
        }
    }

    private void Update()
    {
        if (canPause)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (isPaused)
                {
                    ResumeGame();
                    pauseMenu.SetActive(false);
                }
                else
                {
                    pauseMenu.SetActive(true);
                    PauseGame();
                }
            }
        }
    }

    public void StartLevel()
    {
        randomSongNum = Random.Range(1, 11);

        ResetReloadUI();

        players[0].bullet = bulletTypes[newBulletIndex].GetComponent<Rigidbody2D>();
        players[0].ResetBulletClass();

        players[0].secondaryItem = secondaryItems[newSecondaryIndex];

        SaveManager.instance.GetSaveData().primaryWeaponIndex = newBulletIndex;
        SaveManager.instance.GetSaveData().secondaryWeaponIndex = newSecondaryIndex;

        AudioManager.instance.Play("ButtonClick");

        StartCoroutine(StartCountdown());
    }

    public void RestartLevel()
    {
        canPause = false;

        if (!endlessUnlocked)
        {
            StartCoroutine(AudioManager.instance.FadeOutTrack(LevelManager.instance.currentLevel));
        }
        else
        {
            StartCoroutine(AudioManager.instance.FadeOutTrack(randomSongNum));
        }

        if (endlessUnlocked)
        {
            EndEndless();
        }
        else
        {
            StartCoroutine(ReloadCurrentLevel());
        }
    }

    public void EndLevel()
    {
        canPause = false;
        PauseGame();

        if (!endlessUnlocked)
        {
            StartCoroutine(AudioManager.instance.FadeOutTrack(LevelManager.instance.currentLevel));
        }
        else
        {
            StartCoroutine(AudioManager.instance.FadeOutTrack(randomSongNum));
        }

        if (endlessUnlocked)
        {
            StartCoroutine(TransitionToNextEndlessLevel());
        }
        else
        {
            // Set endlessUnlocked to true if all levels have been completed
            if (LevelManager.instance.currentLevel >= totalNumLevels)
            {
                SaveManager.instance.GetSaveData().endlessUnlocked = true;
                SaveManager.instance.SaveProgress();
                endScreen.SetActive(true);
            }
            else
            {
                StartCoroutine(TransitionToNextLevel());
            }
        }
    }

    public void EndEndless()
    {
        canPause = false;
        PauseGame();
        SaveManager.instance.SaveProgress();
        endlessEndScreen.transform.GetChild(1).GetComponent<Text>().text = "Tanks Defeated: " + endlessScore + "\nHigh Score: " + SaveManager.instance.GetSaveData().endlessScore; ;
        endlessEndScreen.SetActive(true);
    }

    public void RestartEndless()
    {
        endlessScore = 0;
        canPause = false;
        PauseGame();
        StartCoroutine(TransitionToNextEndlessLevel());
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public IEnumerator TransitionToNextEndlessLevel()
    {
        UpdateInterLevelScreenUI(false);

        yield return new WaitForSecondsRealtime(1);

        interLevelScreenFade.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1);

        LevelManager.instance.GenerateLevel();
    }

    public IEnumerator TransitionToNextLevel()
    {
        PauseGame();

        UpdateInterLevelScreenUI(false);

        interLevelScreen.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Level: " + (LevelManager.instance.currentLevel + 1);

        yield return new WaitForSecondsRealtime(1);

        interLevelScreenFade.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1);

        SaveManager.instance.SaveProgress();
        LevelManager.instance.LoadNextLevel();
    }

    public IEnumerator ReloadCurrentLevel()
    {
        canPause = false;
        PauseGame();
        UpdateInterLevelScreenUI(true);

        yield return new WaitForSecondsRealtime(1);

        interLevelScreenFade.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1);

        LevelManager.instance.ReloadCurrentLevel();
    }

    public void SwitchWeapon(int index)
    {
        newBulletIndex = index;

        Debug.Log("Switch Primary");
        AudioManager.instance.Play("ButtonClick");
    }

    public void SwtichSecondary(int index)
    {
        newSecondaryIndex = index;

        AudioManager.instance.Play("ButtonClick");
    }

    public void UpdateAmmoUI(int value) 
    {
        for (int i = 0; i < ammoUI.childCount; i++)
        {
            ammoUI.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < value; i++)
        {
            ammoUI.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void ResetReloadUI()
    {
        if (bulletReload != null)
        {
            StopCoroutine(bulletReload);
        }

        if(secondaryReload != null)
        {
            StopCoroutine(secondaryReload);
        }

        bulletReloadUI.value = 0;
        secondaryReloadUI.value = 0;
    }

    public void AnimateBulletReload(float seconds)
    {
        bulletReload = StartCoroutine(AnimateSliderOverTime(seconds, bulletReloadUI));
    }

    public void AnimateSecondaryReload(float seconds)
    {
        secondaryReload = StartCoroutine(AnimateSliderOverTime(seconds, secondaryReloadUI));
    }

    private IEnumerator AnimateSliderOverTime(float seconds, Slider slider)
    {
        float animationTime = 0f;

        // Changes the slider used for the reloading animation over time
        while (animationTime < seconds)
        {
            animationTime += Time.deltaTime;
            float lerpValue = animationTime / seconds;
            slider.value = Mathf.Lerp(0, 1, lerpValue);
            yield return null;
        }

        slider.value = 0;
    }

    public void UpdateInterLevelScreenUI(bool reload)
    {
        int extra = 0;

        if (!reload)
        {
            extra = 1;
        }

        // Sets the controls help text on if the level is the first one
        if (LevelManager.instance.currentLevel + extra == 1)
        {
            interLevelScreen.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            interLevelScreen.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        }

        // Sets weapons unlocked depending on the number of levels completed
        if (LevelManager.instance.currentLevel + extra > 5)
        {
            numPrimaryWeaponsUnlocked = 2;
        }

        if (endlessUnlocked)
        {
            numPrimaryWeaponsUnlocked = 3;
        }

        if (LevelManager.instance.currentLevel + extra > 8)
        {
            numSecondaryWeaponsUnlocked = 1;
        }

        // Sets each weapon select button on for each weapon unlocked
        for (int i = 0; i < numPrimaryWeaponsUnlocked; i++)
        {
            primaryWeaponContainer.GetChild(i).gameObject.SetActive(true);
        }

        for (int i = 0; i < numSecondaryWeaponsUnlocked; i++)
        {
            secondaryWeaponContainer.GetChild(i).gameObject.SetActive(true);
        }

        primaryWeaponContainer.GetChild(SaveManager.instance.GetSaveData().primaryWeaponIndex).GetComponent<Toggle>().SetIsOnWithoutNotify(true);
        secondaryWeaponContainer.GetChild(SaveManager.instance.GetSaveData().secondaryWeaponIndex).GetComponent<Toggle>().SetIsOnWithoutNotify(true);

        string text = "Endless";

        if (!endlessUnlocked)
        {
             text = "Level " + LevelManager.instance.currentLevel;
        }

        interLevelScreen.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = text;
    }

    // Countdown at the start of each level
    private IEnumerator StartCountdown()
    {
        interLevelScreenFade.SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(1);

        foreach (Transform child in primaryWeaponContainer)
        {
            child.gameObject.SetActive(false);
        }

        Debug.Log("countdown");

        countdown.text = 3.ToString();
        countdown.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1);
        countdown.text = 2.ToString();
        yield return new WaitForSecondsRealtime(1);
        countdown.text = 1.ToString();
        yield return new WaitForSecondsRealtime(1);
        countdown.gameObject.SetActive(false);
        ResumeGame();
        canPause = true;

        if (!endlessUnlocked)
        {
            AudioManager.instance.PlayTrack(LevelManager.instance.currentLevel);
        }
        else
        {
            AudioManager.instance.PlayTrack(randomSongNum);
        }
    }

    public void ButtonHoverOn(string text)
    {
        weaponDescription.text = text;
        weaponDescription.enabled = true;
    }

    public void ButtonHoverOff()
    {
        weaponDescription.enabled = false;
    }

    public void Exit()
    {
        SceneController.LoadScene(0);
    }

    public void IncreaseEndlessScore()
    {
        endlessScore++;
    }

    public int GetEndlessScore()
    {
        return endlessScore;
    }    
}
