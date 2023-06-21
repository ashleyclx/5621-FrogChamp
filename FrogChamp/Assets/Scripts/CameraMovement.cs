using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float speed;

    private float currX = 3.91f;
    private float currY = 3.34f;
    private Vector3 velocity = Vector3.zero;

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
}