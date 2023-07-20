using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement instance;

    [SerializeField] private float speed;

    [SerializeField] private float currX;
    [SerializeField] private float currY;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currX = transform.position.x;
        currY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currX, currY, transform.position.z), 
                                                ref velocity, speed);
    }

    public void ChangeScreen(Transform _newScreen)
    {
        currY = _newScreen.position.y;
    }

    public void SetX(float _x)
    {
        currX = _x;
    }

    public void SetY(float _y)
    {
        currY = _y;
    }

    public void SetZ(float _z)
    {
        // nil
    }
}