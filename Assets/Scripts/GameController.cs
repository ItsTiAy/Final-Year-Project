using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<Player> players;
    public List<Enemy> enemies;
    //public Bullet[] primaryWeapons = new Bullet[2];

    public Bullet[] bulletTypes = new Bullet[2];
    public SecondaryItem[] secondaryItems = new SecondaryItem[1]; 

    //public int enemiesRemaining;
    public int totalNumLevels;

    public GameObject interLevelScreen;
    public GameObject endScreen;
    public GameObject endlessEndScreen;
    public Transform primaryWeaponContainer;
    public Transform secondaryWeaponContainer;

    public GameObject pauseMenu;
    public static bool isPaused = true;

    public TMP_Text ammoUI;

    public TMP_Text countdown;

    private int numPrimaryWeaponsUnlocked = 1;
    private int numSecondaryWeaponsUnlocked = 1;
    private bool endlessUnlocked;

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

        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Levels");
        int len = info.GetFiles("*.json").Length;

        totalNumLevels = len;
    }

    private void Start()
    {
        UpdateInterLevelScreenUI(true);
        endlessUnlocked = SaveManager.instance.GetSaveData().endlessUnlocked;

        if (endlessUnlocked)
        {
            LevelManager.instance.GenerateLevel();
        }
        else
        {
            LevelManager.instance.LoadLevel(SaveManager.instance.GetSaveData().maxLevelNum);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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

    public void StartLevel()
    {
        //interLevelScreen.SetActive(false);

        StartCoroutine(StartCountdown());
    }

    public void RestartLevel()
    {
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

        interLevelScreen.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "Level: " + (LevelManager.instance.currentLevel + 1);

        yield return new WaitForSecondsRealtime(1);

        interLevelScreenFade.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1);

        SaveManager.instance.SaveProgress();
        LevelManager.instance.LoadNextLevel();
    }

    public IEnumerator ReloadCurrentLevel()
    {
        PauseGame();
        UpdateInterLevelScreenUI(true);

        yield return new WaitForSecondsRealtime(1);

        interLevelScreenFade.SetTrigger("FadeIn");

        yield return new WaitForSecondsRealtime(1);

        LevelManager.instance.ReloadCurrentLevel();
    }

    public void SwitchWeapon(Rigidbody2D newBullet)
    {
        players[0].bullet = newBullet;
        players[0].ResetBulletClass();

        SaveManager.instance.GetSaveData().primaryWeaponIndex = newBullet.GetComponent<Bullet>().GetBulletIndex();
    }

    public void SwtichSecondary(SecondaryItem newSecondary)
    {
        players[0].secondaryItem = newSecondary;
        //players[0].ResetBulletClass();

        SaveManager.instance.GetSaveData().secondaryWeaponIndex = newSecondary.GetIndex();
    }

    public void UpdateAmmoUI(int value) 
    {
        ammoUI.text = value.ToString();
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
        }

        for (int i = 0; i < numSecondaryWeaponsUnlocked; i++)
        {
            secondaryWeaponContainer.GetChild(i).gameObject.SetActive(true);
        }

        interLevelScreen.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "Level: " + LevelManager.instance.currentLevel;
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
    }
}
