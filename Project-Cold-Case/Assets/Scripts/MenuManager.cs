using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
