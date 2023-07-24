using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;
    private static List<Achievement> achievementsList;
    public static string[] savedAchievement;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeAchievements();
    }

    public bool AchievementUnlocked(string achievementName)
    {
        bool result = false;

        if (achievementsList == null)
            return false;

        Achievement[] achievementsArray = achievementsList.ToArray();
        Achievement a = Array.Find(achievementsArray, ach => achievementName == ach.title);

        if (a == null)
            return false;

        result = a.achievedAchievement;

        return result;
    }

    private void InitializeAchievements()
    {
        if (achievementsList != null)
            return;

        achievementsList = new List<Achievement>();
        achievementsList.Add(new Achievement("Leap Of Faith", "Made your first jump.", (object o) => StatsManager.instance.GetJumps() == 1));
        achievementsList.Add(new Achievement("Finisher", "Completed the Game Once", (object o) => PlayerPrefs.GetInt("numClears") == 1));
        achievementsList.Add(new Achievement("Never Gonna Give You Up", "Fell 50 tmes", (object o) => PlayerPrefs.GetInt("currFalls") == 50));
        achievementsList.Add(new Achievement("Stuck In The Well", "Played for more than 12 hours", (object o) => TimeManager.instance.GetTime() == 43200f));
    }

    private void Update()
    {
        CheckAchievementCompletion();
    }

    private void CheckAchievementCompletion()
    {
        if (achievementsList == null)
            return;

        foreach (var achievement in achievementsList)
        {
            achievement.UpdateCompletion();
        }
    }

    public string GetAchievementStatus()
    {
        string res = "";
        int count = 0;
        foreach (var achievement in achievementsList)
        {
            if (achievement.achievedAchievement || bool.Parse(savedAchievement[count]))
                res += "true ";
            else
                res += "false ";
            count++;
        }

        res = res.Remove(res.Length - 1, 1);
        return res;
    }
}

public class Achievement
{
    public Achievement(string title, string description, Predicate<object> req)
    {
        this.title = title;
        this.description = description;
        this.req = req;
    }

    public string title;
    public string description;
    public Predicate<object> req;

    public bool achievedAchievement;

    public void UpdateCompletion()
    {
        if (achievedAchievement)
            return;

        if (RequirementsMet())
        {
            achievedAchievement = true;
        }
    }

    public bool RequirementsMet()
    {
        return req.Invoke(null);
    }
}