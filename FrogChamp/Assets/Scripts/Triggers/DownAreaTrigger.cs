// NOT IN USE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownAreaTrigger : MonoBehaviour
{
    [Header("General References:")]
    public Movement movement;

    [Header("Factors:")]
    [SerializeField] private float gravity;
    [SerializeField] private float speed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float horizontalJumpSpeed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.transform.position.y > transform.position.y)
            {
                movement.ScaleMovement(gravity, speed, jumpSpeed, horizontalJumpSpeed);
            }
        }
    }
}