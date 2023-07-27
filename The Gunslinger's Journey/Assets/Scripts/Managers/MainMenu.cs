using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;

    void Start()
    {
        startButton.onClick.AddListener(LoadOverworld);
        quitButton.onClick.AddListener(QuitGame);
    }
    public void LoadOverworld()
    {
        SceneManager.LoadScene(OverworldStats.CurrentScene);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
