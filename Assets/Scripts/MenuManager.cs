using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Text VersionNum;

    private void Start()
    {
        GameVersion();
    }

    public void ButtonClickSound()
    {
        AudioManager.instance.Play("ButtonClick");
    }

    public void GameVersion()
    {
        VersionNum.text = "V " + Application.version;
    }
}
