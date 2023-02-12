using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SaveManager;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public GameObject saveContainer;
    private SaveData saveData;
    private int slotNumber;

    private void Awake()
    {
        // Checks to make sure there is only 1 instance of the level manager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < saveContainer.transform.childCount; i++)
        {
            if (File.Exists(Application.dataPath + "/Saves/save" + (i + 1) + ".json"))
            {
                string json = File.ReadAllText(Application.dataPath + "/Saves/save" + (i + 1) + ".json");
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data.endlessUnlocked)
                {
                    saveContainer.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text = "Slot " + (i + 1) + "\nEndless";
                }
                else
                {
                    saveContainer.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text = "Slot " + (i + 1) + "\nLevel: " + data.maxLevelNum;
                }
            }
        }
    }

    public void LoadSave(int saveNumber)
    {
        slotNumber = saveNumber;

        if (!File.Exists(Application.dataPath + "/Saves/save" + saveNumber + ".json"))
        {
            CreateNewSave(saveNumber);
        }

        string json = File.ReadAllText(Application.dataPath + "/Saves/save" + saveNumber + ".json");

        saveData = JsonUtility.FromJson<SaveData>(json);

        SceneController.LoadScene(1);
    }

    private void CreateNewSave(int saveNumber)
    {
        SaveData data = new SaveData();
        data.maxLevelNum = 1;
        data.primaryWeaponIndex = 0;

        string json = JsonUtility.ToJson(data, true);
        string save = "save" + saveNumber;
        File.WriteAllText(Application.dataPath + "/Saves/" + save + ".json", json);

        Debug.Log("Save");
    }

    public void DeleteSave(int saveNumber)
    {
        string filepath = Application.dataPath + "/Saves/save" + saveNumber + ".json";

        if (File.Exists(filepath))
        {
            File.Delete(filepath);

            saveContainer.transform.GetChild(saveNumber - 1).GetChild(0).GetComponentInChildren<TMP_Text>().text = "Empty";
        }
        else
        {
            Debug.Log("File does not exist");
        }
    }
    
    public SaveData GetSaveData()
    {
        return saveData;
    }

    public void SaveProgress()
    {
        saveData.maxLevelNum = LevelManager.instance.currentLevel + 1;
        saveData.primaryWeaponIndex = GameController.instance.players[0].bullet.GetComponent<Bullet>().GetBulletIndex();
        saveData.secondaryWeaponIndex = GameController.instance.players[0].secondaryItem.GetIndex();

        string json = JsonUtility.ToJson(saveData, true);
        string save = "save" + slotNumber;
        File.WriteAllText(Application.dataPath + "/Saves/" + save + ".json", json);
    }
    

    [System.Serializable]
    public class SaveData
    {
        public int maxLevelNum;
        public int primaryWeaponIndex;
        public int secondaryWeaponIndex;
        public bool endlessUnlocked = false;
    }
}
