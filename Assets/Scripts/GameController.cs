using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<Player> players;
    public List<Enemy> enemies;
    //public Bullet[] primaryWeapons = new Bullet[2];

    public Bullet[] bulletTypes = new Bullet[3];
    public SecondaryItem[] secondaryItems = new SecondaryItem[1]; 

    //public int enemiesRemaining;
    public int totalNumLevels;

    public GameObject interLevelScreen;
    public GameObject endScreen;
    public GameObject endlessEndScreen;
    public Transform primaryWeaponContainer;
    public Transform secondaryWeaponContainer;
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

    private Coroutine bulletReload;
    private Coroutine secondaryReload;

    public Animator interLevelScreenFade;

    private void Awake()
    {
        PauseGame();

        // Checks to make sure there is only 1 instance of the game controller
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        players = new List<Player>();
        enemies = new List<Enemy>();

        //int wombat = Resources.LoadAll("Levels").Length;

        //DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Levels");
        //int len = info.GetFiles("*.json").Length;

        totalNumLevels = 10;
    }

    private void Start()
    {
        endlessUnlocked = SaveManager.instance.GetSaveData().endlessUnlocked;
        UpdateInterLevelScreenUI(true);

        if (endlessUnlocked)
        {
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
        //interLevelScreen.SetActive(false);

        ResetReloadUI();

        players[0].bullet = bulletTypes[newBulletIndex].GetComponent<Rigidbody2D>();
        players[0].ResetBulletClass();

        players[0].secondaryItem = secondaryItems[newSecondaryIndex];

        SaveManager.instance.GetSaveData().primaryWeaponIndex = newBulletIndex;
        SaveManager.instance.GetSaveData().secondaryWeaponIndex = newSecondaryIndex;

        StartCoroutine(StartCountdown());
    }

    public void RestartLevel()
    {
        canPause = false;
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
        if (endlessUnlocked)
        {
            StartCoroutine(TransitionToNextEndlessLevel());
        }
        else
        {
            if (LevelManager.instance.currentLevel >= totalNumLevels)
            {
                PauseGame();
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
        endlessEndScreen.SetActive(true);
    }

    public void RestartEndless()
    {
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
        canPause = false;
        PauseGame();

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

        //players[0].bullet = newBullet;
        //players[0].ResetBulletClass();

        //newBullet.GetComponent<Bullet>().GetBulletIndex();
    }

    public void SwtichSecondary(int index)
    {
        newSecondaryIndex = index;
        //players[0].ResetBulletClass();
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

        if(!reload)
        {
            extra = 1;
        }

        if (LevelManager.instance.currentLevel + extra > 5)
        {
            numPrimaryWeaponsUnlocked = 2;
        }

        if (LevelManager.instance.currentLevel + extra > 7)
        {
            numPrimaryWeaponsUnlocked = 3;
        }

        if (LevelManager.instance.currentLevel + extra > 8)
        {
            numSecondaryWeaponsUnlocked = 1;
        }

        for (int i = 0; i < numPrimaryWeaponsUnlocked; i++)
        {
            primaryWeaponContainer.GetChild(i).gameObject.SetActive(true);
            //primaryWeaponContainer.GetChild(i).GetComponent<Button>().OnPointerEnter();
        }

        for (int i = 0; i < numSecondaryWeaponsUnlocked; i++)
        {
            secondaryWeaponContainer.GetChild(i).gameObject.SetActive(true);
        }

        primaryWeaponContainer.GetChild(SaveManager.instance.GetSaveData().primaryWeaponIndex).GetComponent<Toggle>().isOn = true;
        secondaryWeaponContainer.GetChild(SaveManager.instance.GetSaveData().secondaryWeaponIndex).GetComponent<Toggle>().isOn = true;

        interLevelScreen.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Level " + LevelManager.instance.currentLevel;
    }

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
        //SaveManager.instance.SaveProgress();
        SceneController.LoadScene(0);
    }
}
