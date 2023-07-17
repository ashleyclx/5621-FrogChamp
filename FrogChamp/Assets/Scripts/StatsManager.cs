using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;
    private int numJumps;
    private int numFalls;

    private void Awake()
    {
        instance = this;
    }

    public void AddJump()
    {
        numJumps += 1;
    }

    public void AddFall()
    {
        numFalls += 1;
    }
}
