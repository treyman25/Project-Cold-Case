using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void LoadGame()
    {
        SceneManager.LoadScene("IntroCard");
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
