using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DatabasePref : MonoBehaviour
{
    // Completion Data Variables
    [Header("Statistics Data")]
    public TMP_Text bestTimingPlaceholder;
    public TMP_Text totalJumpsPlaceholder;
    public TMP_Text totalFallsPlaceholder;
    public TMP_Text numClearsPlaceholder;

    // Achievement Data Variables
    [Header("Achievement Data")]
    public TMP_Text _1CT;
    public TMP_Text _2CT;
    public TMP_Text _3CT;
    public TMP_Text _4CT;

    private string defaultAchievement = "false false";

    // Button to open user stats screen
    public void StatisticsButton()
    {
        LoadStatsData();
    }

    // Button to open achievement screen
    public void AchievementButton()
    {
        DisplayAchievement();
        UIManager.instance.Achievement1Screen();
    }

    // Button to start game
    public void StartGameButton()
    {
        LoadUserData();
    }

    // Function to save progress (pause game)
    public void SaveProgress()
    {
        Vector2 currentPosition = StatsManager.instance.GetPosition();
        Vector3 currentCameraPosition = StatsManager.instance.GetCameraPosition();
        // Saves user stats to DB
        UpdateCurrJumps(StatsManager.instance.GetJumps());
        UpdateCurrFalls(StatsManager.instance.GetFalls());
        UpdateXPos(currentPosition.x);
        UpdateYPos(currentPosition.y);
        UpdateXCamPos(currentCameraPosition.x);
        UpdateYCamPos(currentCameraPosition.y);
        UpdateZCamPos(currentCameraPosition.z);
        UpdateCurrTime(TimeManager.instance.GetTime());
    }

    // Function to update scoreboard (finished game)
    public void FinishProgress()
    {
        // Updates bestTime to database
        if (PlayerPrefs.GetFloat("bestTime") == 0f)
            UpdateBestTime(TimeManager.instance.GetTime());
        else
        {
            if (TimeManager.instance.GetTime() < PlayerPrefs.GetFloat("bestTime"))
                UpdateBestTime(TimeManager.instance.GetTime());
        }

        // Updates cummulative jumps to database
        if (PlayerPrefs.GetInt("totalJumps") == 0)
            UpdateTotalJumps(StatsManager.instance.GetJumps());
        else
            UpdateTotalJumps(PlayerPrefs.GetInt("totalJumps") + StatsManager.instance.GetJumps());

        // Updates cummulative falls to database
        if (PlayerPrefs.GetInt("totalFalls")== 0)
            UpdateTotalFalls(StatsManager.instance.GetFalls());
        else
            UpdateTotalFalls(PlayerPrefs.GetInt("totalFalls") + StatsManager.instance.GetFalls());

        // Updates number of clears
        UpdateNumClears(PlayerPrefs.GetInt("numClears") + 1);
        
        // Resets all curr values to 0
        UpdateCurrJumps(0);
        UpdateCurrFalls(0);
        UpdateXPos(-6f);
        UpdateYPos(-3.5f);
        UpdateXCamPos(4.15f);
        UpdateYCamPos(3f);
        UpdateZCamPos(10f);
        UpdateCurrTime(0f);

        // Go to main menu screen
        UIManager.instance.MainMenuScreen();
    }

    // Function to save user's achievement status into DB
    public void SaveAchievement()
    {
        LoadAchievementData();
        UpdateAchievementStatus(AchievementManager.instance.GetAchievementStatus());
    }

    // Function to display user's achievement status on achievement screen
    private void DisplayAchievement()
    {
        List<string> achievementText = new List<string>();

        LoadAchievementData();
        
        foreach (var achievementBool in AchievementManager.savedAchievement)
        {
            if (achievementBool == "true")
                achievementText.Add("completed");
            else
                achievementText.Add("not completed");
        }

        _1CT.text = achievementText[0];
        _2CT.text = achievementText[1];
        //_3CT.text = achievementText[2];
        //_4CT.text = achievementText[3];
    }

    private void UpdateUsername(string _username)
    {
        PlayerPrefs.SetString("username", _username);
    }

    private void UpdateCurrJumps(int _currJumps)
    {
        PlayerPrefs.SetInt("currJumps", _currJumps);
    }

    private void UpdateCurrFalls(int _currFalls)
    {
        PlayerPrefs.SetInt("currFalls", _currFalls);
    }

    private void UpdateXPos(float _xpos)
    {
        PlayerPrefs.SetFloat("xpos", _xpos);
    }

    private void UpdateYPos(float _ypos)
    {
        PlayerPrefs.SetFloat("ypos", _ypos);
    }

    private void UpdateXCamPos(float _xcampos)
    {
        PlayerPrefs.SetFloat("xcampos", _xcampos);
    }

    private void UpdateYCamPos(float _ycampos)
    {
        PlayerPrefs.SetFloat("ycampos", _ycampos);
    }

    private void UpdateZCamPos(float _zcampos)
    {
        PlayerPrefs.SetFloat("zcampos", _zcampos);
    }

    private void UpdateCurrTime(float _currTime)
    {
        PlayerPrefs.SetFloat("currTime", _currTime);
    }

    private void UpdateBestTime(float _bestTime)
    {
        PlayerPrefs.SetFloat("bestTime", _bestTime);
    }

    private void UpdateTotalJumps(int _totalJumps)
    {
        PlayerPrefs.SetInt("totalJumps", _totalJumps);
    }

    private void UpdateTotalFalls(int _totalFalls)
    {
        PlayerPrefs.SetInt("totalFalls", _totalFalls);
    }

    private void UpdateNumClears(int _numClears)
    {
        PlayerPrefs.SetInt("numClears", _numClears);
    }

    private void UpdateAchievementStatus(string _achievementStatus)
    {
        PlayerPrefs.SetString("achievementStatus", _achievementStatus);
    }

    private void LoadUserData()
    {
        if (PlayerPrefs.GetFloat("currTime", 0) == 0f)
        {
            //No data exists yet (new game)
            TimeManager.instance.SetTime(0f);

            // Start game
            UIManager.instance.StartGame();
        }
        else
        {
            // Sets data for resume progress
            StatsManager.instance.SetPosition(PlayerPrefs.GetFloat("xpos"), PlayerPrefs.GetFloat("ypos"));
            StatsManager.instance.SetCameraPosition(PlayerPrefs.GetFloat("xcampos"), PlayerPrefs.GetFloat("ycampos"), PlayerPrefs.GetFloat("zcampos"));
            StatsManager.instance.SetJumps(PlayerPrefs.GetInt("currJumps"));
            StatsManager.instance.SetFalls(PlayerPrefs.GetInt("currFalls"));
            TimeManager.instance.SetTime(PlayerPrefs.GetFloat("currTime"));

            // Start game
            UIManager.instance.StartGame();
        }
    }

    private void LoadStatsData()
    {
        // Sets text display for statistics screen
        bestTimingPlaceholder.text = PlayerPrefs.GetFloat("bestTime", 0).ToString();
        totalJumpsPlaceholder.text = PlayerPrefs.GetInt("totalJumps", 0).ToString();
        totalFallsPlaceholder.text = PlayerPrefs.GetInt("totalFalls", 0).ToString();
        numClearsPlaceholder.text = PlayerPrefs.GetInt("numClears", 0).ToString();

        // Go to statistics screen
        UIManager.instance.StatisticsScreen();
    }

    private void LoadAchievementData()
    {
        string achievementStatus = PlayerPrefs.GetString("achievementStatus", defaultAchievement);
        string[] achievementBoolArray = achievementStatus.Split(' ');
        AchievementManager.savedAchievement = achievementBoolArray;
    }

    private void LoadScoreboardData()
    {
        // No need this function as no longer can implement scoreboard.
    }
}
