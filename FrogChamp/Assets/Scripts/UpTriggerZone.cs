using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpTriggerZone : MonoBehaviour
{
    [SerializeField] private Transform nextScreen;
    [SerializeField] private CameraMovement cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.transform.position.y < transform.position.y)
                cam.ChangeScreen(nextScreen);
        }
    }
}