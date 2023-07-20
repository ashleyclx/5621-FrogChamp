using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseScreen;

    [Header("Timer")]
    public GameObject timerUI;

    [Header("Firebase")]
    public GameObject startUI;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject statisticsUI;
    public GameObject userDataUI;
    public GameObject leaderboardUI;
    public GameObject mainMenuUI;

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

        // press F5 to restart
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Restart();
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

    public void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //public void MainMenu()
    //{
       // mainMenuUI.SetActive(true);
    //}

    public void Restart() //issue
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PauseGame(false);
    }
    #endregion

    #region Firebase
    public void ClearScreen() //Turn off all screens
    {
        startUI.SetActive(false);
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        statisticsUI.SetActive(false);
        userDataUI.SetActive(false);
        leaderboardUI.SetActive(false);
        mainMenuUI.SetActive(false);
        pauseScreen.SetActive(false);
        timerUI.SetActive(false);
    }

    public void MainMenuScreen()
    {
        ClearScreen();
        mainMenuUI.SetActive(true);
    }

    public void LoginScreen() //Back button
    {
        ClearScreen();
        loginUI.SetActive(true);
    }

    public void StartScreen() // Back button to Start Screen
    {
        ClearScreen();
        startUI.SetActive(true);
    }

    public void RegisterScreen() // Register button
    {
        ClearScreen();
        registerUI.SetActive(true);
    }

    public void StatisticsScreen()
    {
        ClearScreen();
        statisticsUI.SetActive(true);
    }

    public void UserDataScreen() //Logged in
    {
        ClearScreen();
        userDataUI.SetActive(true);
    }

    public void LeaderboardScreen() //Scoreboard button
    {
        ClearScreen();
        leaderboardUI.SetActive(true);
    }
    #endregion
}
