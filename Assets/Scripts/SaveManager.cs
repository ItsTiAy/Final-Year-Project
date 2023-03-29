using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public GameObject saveContainer;
    private SaveData saveData;
    private int slotNumber;

    [DllImport("__Internal")]
    private static extern void SetCookie(string cname, string cvalue);
    [DllImport("__Internal")]
    private static extern string GetCookie(string cname);
    [DllImport("__Internal")]
    private static extern string DeleteCookie(string cname);

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

        InitalizeSaveUI();
    }

    // NEED TO FIX SAVING STUFF IN FUTURE (maybe)
    public void LoadSave(int saveNumber)
    {
        slotNumber = saveNumber;
        string json;

        try
        {
            json = GetCookie("save" + saveNumber);

            Debug.Log(json);

            if (json == "")
            {
                Debug.Log("New save");
                CreateNewSave(saveNumber);
                json = GetCookie("save" + saveNumber.ToString());
            }
        }
        catch 
        {
            if (!File.Exists(Application.streamingAssetsPath + "/Saves/save" + saveNumber + ".json"))
            {
                CreateNewSave(saveNumber);
            }

            json = File.ReadAllText(Application.streamingAssetsPath + "/Saves/save" + saveNumber + ".json");
        }

        Debug.Log("Load save");
        saveData = JsonUtility.FromJson<SaveData>(json);
        SceneController.LoadScene(1);
        
        


        /*
        if (!File.Exists(Application.dataPath + "/Saves/save" + saveNumber + ".json"))
        {
            CreateNewSave(saveNumber);
        }
        */

        //string json = File.ReadAllText(Application.dataPath + "/Saves/save" + saveNumber + ".json");

        //string path = Application.streamingAssetsPath + "/Saves/save" + saveNumber + ".json";
        //UnityWebRequest uwr = UnityWebRequest.Get(path);
        //uwr.SendWebRequest();

        //StartCoroutine(LoadSaveCoroutine(path));
    }

    private void CreateNewSave(int saveNumber)
    {
        SaveData data = new SaveData();
        data.maxLevelNum = 1;
        data.primaryWeaponIndex = 0;

        string json = JsonUtility.ToJson(data, false);
        string save = "save" + saveNumber;

        try
        {
            SetCookie(save, json);
        }
        catch
        {
            File.WriteAllText(Application.streamingAssetsPath + "/Saves/" + save + ".json", json);
        }

        Debug.Log("Save");
    }

    public void DeleteSave(int saveNumber)
    {
        try
        {
            string json = GetCookie("save" + saveNumber);

            if (json != "")
            {
                DeleteCookie("save" + saveNumber);

                saveContainer.transform.GetChild(saveNumber - 1).GetChild(0).GetComponentInChildren<Text>().text = "Empty";
            }
            else
            {
                Debug.Log("Cookie does not exist");
            }
        }
        catch
        {
            string filepath = Application.streamingAssetsPath + "/Saves/save" + saveNumber + ".json";

            if (File.Exists(filepath))
            {
                File.Delete(filepath);

                saveContainer.transform.GetChild(saveNumber - 1).GetChild(0).GetComponentInChildren<Text>().text = "Empty";
            }
            else
            {
                Debug.Log("File does not exist");
            }
        }
    }
    
    public SaveData GetSaveData()
    {
        return saveData;
    }

    private IEnumerator LoadSaveCoroutine(string path)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(path);
        yield return uwr.SendWebRequest();
        saveData = JsonUtility.FromJson<SaveData>(uwr.downloadHandler.text);
        SceneController.LoadScene(1);
    }

    private void InitalizeSaveUI()
    {
        // For web
        try
        {
            for (int i = 0; i < saveContainer.transform.childCount; i++)
            {
                string json = GetCookie("save" + (i + 1));

                if (json != "")
                {
                    SaveData data = JsonUtility.FromJson<SaveData>(json);

                    if (data.endlessUnlocked)
                    {
                        saveContainer.transform.GetChild(i).GetComponentInChildren<Text>().text = "Slot " + (i + 1) + " | Endless";
                    }
                    else
                    {
                        saveContainer.transform.GetChild(i).GetComponentInChildren<Text>().text = "Slot " + (i + 1) + " | Level: " + data.maxLevelNum;
                    }
                }
            }
        }
        // For unity editor
        catch
        {
            for (int i = 0; i < saveContainer.transform.childCount; i++)
            {
                string path = Application.streamingAssetsPath + "/Saves/save" + (i + 1) + ".json";

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);

                    SaveData data = JsonUtility.FromJson<SaveData>(json);

                    if (data.endlessUnlocked)
                    {
                        saveContainer.transform.GetChild(i).GetComponentInChildren<Text>().text = "Slot " + (i + 1) + " | Endless";
                    }
                    else
                    {
                        saveContainer.transform.GetChild(i).GetComponentInChildren<Text>().text = "Slot " + (i + 1) + " | Level: " + data.maxLevelNum;
                    }
                }
            }
        }
    }

    public void SaveProgress()
    {
        saveData.maxLevelNum = LevelManager.instance.currentLevel + 1;
        saveData.primaryWeaponIndex = GameController.instance.players[0].bullet.GetComponent<Bullet>().GetBulletIndex();
        saveData.secondaryWeaponIndex = GameController.instance.players[0].secondaryItem.GetIndex();

        string json = JsonUtility.ToJson(saveData);
        string save = "save" + slotNumber;

        try
        {
            SetCookie(save, json);
        }
        catch
        {
            File.WriteAllText(Application.streamingAssetsPath + "/Saves/" + save + ".json", json);
        }
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
