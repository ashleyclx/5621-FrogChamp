using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Pause Menu")]
    public GameObject pauseScreen;

    [Header("Timer")]
    public GameObject timerUI;

    [Header("Firebase")]
    public GameObject startUI;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject statisticsUI;
    public GameObject scoreboardUI;
    public GameObject achievement1UI;
    public GameObject mainMenuUI;
    public GameObject endUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        pauseScreen.SetActive(false);
    }

    private void Update()
    {
        // press esc to pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // if pause screen inactive, pause
            // if pause screen already active, unpause
            if (pauseScreen.activeInHierarchy)
                PauseGame(false);
            else
                PauseGame(true);
        }
    }

    #region Main Menu
    public void StartGame()
    {
        ClearScreen();
        timerUI.SetActive(true);

        if (Time.timeScale == 0) 
        {
            Time.timeScale = 1;
        }
        TimeManager.instance.BeginTimer();
    }

    public void Quit()
    {
        // Quits game for builds
        Application.Quit();

        #if UNITY_EDITOR
        // Exits play mode for editor
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    #endregion

    #region Pause Menu
    public void PauseGame(bool status)
    {
        // pause if status is true, else unpause
        pauseScreen.SetActive(status);

        // when paused, time scale == 0 (time stops)
        // when unpaused, time scale == 1 (time flows normally)
        if (status)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    //to remove
    public void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Restart() //issue
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PauseGame(false);
    }
    #endregion

    #region Firebase

    // Turns off all screens
    public void ClearScreen() 
    {
        startUI.SetActive(false);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        statisticsUI.SetActive(false);
        scoreboardUI.SetActive(false);
        achievement1UI.SetActive(false);
        mainMenuUI.SetActive(false);
        pauseScreen.SetActive(false);
        timerUI.SetActive(false);
        endUI.SetActive(false);
    }

    // Opens up main menu screen
    public void MainMenuScreen()
    {
        ClearScreen();
        mainMenuUI.SetActive(true);
    }

    // Opens up login screen
    public void LoginScreen()
    {
        ClearScreen();
        loginUI.SetActive(true);
    }

    // Opens up start screen
    public void StartScreen()
    {
        ClearScreen();
        startUI.SetActive(true);
    }

    // Opens up register screen
    public void RegisterScreen()
    {
        ClearScreen();
        registerUI.SetActive(true);
    }

    // Opens up user's personal stats page
    public void StatisticsScreen() // Opens up user's personal stats page
    {
        ClearScreen();
        statisticsUI.SetActive(true);
    }

    // Opens up public leaderboard screen
    public void ScoreboardScreen() 
    {
        ClearScreen();
        scoreboardUI.SetActive(true);
    }

    // Opens up achievement 1 screen
    public void Achievement1Screen()
    {
        ClearScreen();
        achievement1UI.SetActive(true);
    }

    // Opens up the end game screen
    public void EndScreen()
    {
        ClearScreen();
        endUI.SetActive(true);
    }
    #endregion
}
