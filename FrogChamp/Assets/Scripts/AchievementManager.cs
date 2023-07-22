using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;
    public static List<Achievement> achievements;
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

        if (achievements == null)
            return false;

        Achievement[] achievementsArray = achievements.ToArray();
        Achievement a = Array.Find(achievementsArray, ach => achievementName == ach.title);

        if (a == null)
            return false;

        result = a.achieved;

        return result;
    }

    private void InitializeAchievements()
    {
        if (achievements != null)
            return;

        achievements = new List<Achievement>();
        achievements.Add(new Achievement("Leap Of Faith", "Made your first jump.", (object o) => StatsManager.instance.GetJumps() == 1));
        achievements.Add(new Achievement("Finisher", "Does the air on top smell fresher?", (object o) => UIManager.instance.endUI.activeInHierarchy));
    }

    private void Update()
    {
        CheckAchievementCompletion();
    }

    private void CheckAchievementCompletion()
    {
        if (achievements == null)
            return;

        foreach (var achievement in achievements)
        {
            achievement.UpdateCompletion();
        }
    }

    public string GetAchievementStatus(List<Achievement> _achievements)
    {
        string res = "";
        int count = 0;
        foreach (var achievement in achievements)
        {
            if (achievement.achieved || bool.Parse(savedAchievement[count]))
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
    public Achievement(string title, string description, Predicate<object> requirement)
    {
        this.title = title;
        this.description = description;
        this.requirement = requirement;
    }

    public string title;
    public string description;
    public Predicate<object> requirement;

    public bool achieved;

    public void UpdateCompletion()
    {
        if (achieved)
            return;

        if (RequirementsMet())
        {
            Debug.Log($"{title}: {description}"); // remove after implementation
            achieved = true;
        }
    }

    public bool RequirementsMet()
    {
        return requirement.Invoke(null);
    }
}