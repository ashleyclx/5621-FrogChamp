using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] GameObject pauseScreen;

    private void Awake()
    {
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

        // press r to restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

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

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
}
