using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public TMP_Text timeCounter;

    private TimeSpan timePlaying;
    private bool timerGoing;

    private float elapsedTime;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        timeCounter.text = "Time: 00:00:00.00";
        //timerGoing = false;
    }

    public void BeginTimer()
    {
        timerGoing = true;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerGoing = false;
    }

    public void SetTime(float _time)
    {
        elapsedTime = _time;
    }

    public float GetTime()
    {
        return (float)timePlaying.TotalSeconds;
    }

    private IEnumerator UpdateTimer()
        {
            while (timerGoing)
            {
                elapsedTime += Time.deltaTime;
                timePlaying = TimeSpan.FromSeconds(elapsedTime);
                string timePlayingStr = "Time: " + timePlaying.ToString("hh':'mm':'ss'.'ff");
                timeCounter.text = timePlayingStr;

                yield return null;
            }
        }
}
