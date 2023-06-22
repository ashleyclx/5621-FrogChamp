using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownTriggerZone : MonoBehaviour
{
    [SerializeField] private Transform prevScreen;
    [SerializeField] private CameraMovement cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.transform.position.y > transform.position.y)
                cam.ChangeScreen(prevScreen);
        }
    }
}