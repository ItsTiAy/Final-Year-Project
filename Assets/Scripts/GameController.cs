using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<Player> players;
    //public Bullet[] primaryWeapons = new Bullet[2];

    public Bullet[] bulletTypes = new Bullet[2];

    public int enemiesRemaining;
    public int totalNumLevels;

    public GameObject interLevelScreen;
    public Transform weaponContainer;

    public GameObject pauseMenu;
    public static bool isPaused = true;

    public TMP_Text ammoUI;

    private int numPrimaryWeaponsUnlocked = 2;
    //private int numSecondaryWeaponsUnlocked = 0;

    private void Awake()
    {
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

        //int wombat = Resources.LoadAll("Levels").Length;

        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Levels");
        int len = info.GetFiles("*.json").Length;

        totalNumLevels = len;

        Debug.Log(totalNumLevels);
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
        interLevelScreen.SetActive(false);
        
        foreach(Transform child in weaponContainer)
        {
            child.gameObject.SetActive(false);
        }

        ResumeGame();
    }

    public void EndLevel()
    {
        LoadInterLevelScreen();
        SaveManager.instance.SaveProgress();
        LevelManager.instance.LoadNextLevel();
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

    public void LoadInterLevelScreen()
    {
        PauseGame();

        for (int i = 0; i < numPrimaryWeaponsUnlocked; i++)
        {
            weaponContainer.GetChild(i).gameObject.SetActive(true);
        }

        interLevelScreen.SetActive(true);
    }

    public void SwitchWeapon(Rigidbody2D newBullet)
    {
        players[0].bullet = newBullet;
        players[0].ResetBulletClass();
    }

    public void UpdateAmmoUI(int value) 
    {
        ammoUI.text = value.ToString();
    }
}
