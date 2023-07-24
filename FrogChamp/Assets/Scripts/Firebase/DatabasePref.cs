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
    public TMP_Text _5CT;
    public TMP_Text _6CT;
    public TMP_Text _7CT;
    public TMP_Text _8CT;

    //Scoreboard Data
    [Header("Scoreboard Data")]
    public TMP_Text firstTimePlaceholder;
    public TMP_Text secondTimePlaceholder;
    public TMP_Text thirdTimePlaceholder;

    private string defaultAchievement = "false false false false false false false false";

    private void Start()
    {
        if (PlayerPrefs.GetFloat("currTime") == 0f && PlayerPrefs.GetInt("numClears") == 0)
        {
            UpdateXCamPos(4.15f);
            UpdateYCamPos(3f);
            UpdateZCamPos(10f);
        }
    }

    // Button to open user stats screen
    public void StatisticsButton()
    {
        LoadStatsData();
    }

    // Button to open user scoreboard screen
    public void ScoreboardButton()
    {
        LoadScoreboardData();
        UIManager.instance.ScoreboardScreen();
    }

    // Button to return to main menu screen from scoreboard screen
    public void ScoreboardBackButton()
    {
        UIManager.instance.MainMenuScreen();
        ClearScoreboardFields();
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
        UIManager.instance.StartGame();
    }

    // Button to return to main menu screen from end screen
    public void EndMainMenuButton()
    {
        SaveAchievement();
        UpdateScoreboard(TimeManager.instance.GetTime());
        FinishProgress();
        UIManager.instance.MainMenuScreen();
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
        UpdateXCamPos(4.15f);
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
        UpdateTotalJumps(PlayerPrefs.GetInt("totalJumps", 0) + StatsManager.instance.GetJumps());

        // Updates cummulative falls to database
        UpdateTotalFalls(PlayerPrefs.GetInt("totalFalls", 0) + StatsManager.instance.GetFalls());

        // Updates number of clears
        UpdateNumClears(PlayerPrefs.GetInt("numClears", 0) + 1);
        
        // Resets all curr values to 0
        UpdateCurrJumps(0);
        UpdateCurrFalls(0);
        UpdateXPos(-6.86f);
        UpdateYPos(-3.69f);
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
        _3CT.text = achievementText[2];
        _4CT.text = achievementText[3];
        _5CT.text = achievementText[4];
        _6CT.text = achievementText[5];
        _7CT.text = achievementText[6];
        _8CT.text = achievementText[7];
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

    private void ClearScoreboardFields()
    {
        firstTimePlaceholder.text = "";
        secondTimePlaceholder.text = "";
        thirdTimePlaceholder.text = "";
    }

    private void UpdateScoreboard(float _runTime)
    {
        List<float> topTimings = new List<float>();
        string[] prefsName = {"firstBestTime", "secondBestTime", "thirdBestTime"};
        topTimings.Add(PlayerPrefs.GetFloat("firstBestTime", 0f));
        topTimings.Add(PlayerPrefs.GetFloat("secondBestTime", 0f));
        topTimings.Add(PlayerPrefs.GetFloat("thirdBestTime", 0f));
        topTimings.Add(_runTime);
        topTimings.RemoveAll(item => item == 0);
        topTimings.Sort();

        for (int i = 0; i < topTimings.Count; i++)
        {
            if (i == 3)
                break;

            PlayerPrefs.SetFloat(prefsName[i], topTimings[i]);
        }
    }

    private void LoadScoreboardData()
    {
        List<float> topTimings = new List<float>();
        List<TMP_Text> textPlaceholders = new List<TMP_Text> {firstTimePlaceholder, secondTimePlaceholder, thirdTimePlaceholder};
        topTimings.Add(PlayerPrefs.GetFloat("firstBestTime", 0f));
        topTimings.Add(PlayerPrefs.GetFloat("secondBestTime", 0f));
        topTimings.Add(PlayerPrefs.GetFloat("thirdBestTime", 0f));
        topTimings.RemoveAll(item => item == 0);
        topTimings.Sort();

        for (int i = 0; i < topTimings.Count; i++)
        {
            textPlaceholders[i].text = topTimings[i].ToString();
        }
    }

    private void LoadUserData()
    {
        if (PlayerPrefs.GetFloat("currTime", 0) == 0f)
        {
            //No data exists yet (new game)
            TimeManager.instance.SetTime(0f);
        }

        // Sets data for resume progress
        StatsManager.instance.SetPosition(PlayerPrefs.GetFloat("xpos", -6.86f), PlayerPrefs.GetFloat("ypos", -3.69f));
        StatsManager.instance.SetCameraPosition(PlayerPrefs.GetFloat("xcampos", 4.15f), PlayerPrefs.GetFloat("ycampos", 3f), PlayerPrefs.GetFloat("zcampos", 10f));
        StatsManager.instance.SetJumps(PlayerPrefs.GetInt("currJumps"));
        StatsManager.instance.SetFalls(PlayerPrefs.GetInt("currFalls"));
        TimeManager.instance.SetTime(PlayerPrefs.GetFloat("currTime"));
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
}
