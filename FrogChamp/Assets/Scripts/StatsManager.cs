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

    public Vector3 GetCameraPosition()
    {
        return Camera.main.transform.position;
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

    public void SetPosition(float _x, float _y)
    {
        transform.position = new Vector2(_x, _y);
    }

    public void SetCameraPosition(float _x, float _y, float _z)
    {
        CameraMovement.instance.SetX(_x);
        CameraMovement.instance.SetY(_y);
        CameraMovement.instance.SetZ(_z);
    }

    public int GetBestRecord(int databaseRecord, int currentRecord)
    {
        return databaseRecord < currentRecord ? databaseRecord : currentRecord;
    }
}
