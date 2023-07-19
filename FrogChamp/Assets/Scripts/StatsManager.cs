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

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void AddJump()
    {
        numJumps += 1;
    }

    public void AddFall()
    {
        numFalls += 1;
    }

    public int GetJumps()
    {
        return numJumps;
    }

    public int GetFalls()
    {
        return numFalls;
    }

    public void SetJumps(int _jumps)
    {
        numJumps = _jumps;
    }

    public void SetFalls(int _falls)
    {
        numFalls = _falls;
    }

    public int GetBestRecord(int databaseRecord, int currentRecord)
    {
        return databaseRecord < currentRecord ? databaseRecord : currentRecord;
    }
}
