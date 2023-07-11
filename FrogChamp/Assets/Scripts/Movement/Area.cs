using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [Header("Script References:")]
    public Movement movement;
    public Checks checks;

    [Header("Area Coordinates:")]
    [SerializeField] private float marshStart = 208;
    [SerializeField] private float marshEnd = 325;
    [SerializeField] private float iceStart = 410;
    [SerializeField] private float iceEnd = 600;

    private bool inMarsh = false;

    // Scales gravity of player by a factor
    public void ScaleMovement(float _gravity, float _speed, float _jumpSpeed, float _horizontalJumpSpeed)
    {
        movement.body.gravityScale *= _gravity;
        movement.speed *= _speed;
        movement.jumpSpeed *= _jumpSpeed;
        movement.horizontalJumpSpeed *= _horizontalJumpSpeed;
    }

    // Scale player movement when entering/exiting marsh area based on y coordinate.
    // Marsh y boundary: 208 to 325
    public void Marsh()
    {
        if (!inMarsh) 
        {
            if (transform.position.y > marshStart && transform.position.y < marshEnd)
            {
                float ratio = 0.65f;
                ScaleMovement(ratio, ratio, ratio, 1);
                movement.body.velocity = new Vector2(movement.body.velocity.x, 0.65f * movement.body.velocity.y);
                inMarsh = true;
            }
        }
        else
        {
            if (transform.position.y < marshStart || transform.position.y > marshEnd)
            {
                float inverse = 1.0f / 0.65f;
                ScaleMovement(inverse, inverse, inverse, 1);
                movement.body.velocity = new Vector2(movement.body.velocity.x, 2.0f * movement.body.velocity.y);
                inMarsh = false;
            }
        }
    }

    public void Ice()
    {
        Vector3 boxSize = movement.capsuleCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(movement.capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, movement.platformLayer);
        if (transform.position.y > iceStart && transform.position.y < iceEnd)
            checks.ToggleIcyState(true);
        if (raycastHit.collider != null)
            checks.ToggleIcyState(false);

    }

    // Scale player movement when entering/exiting desert area based on y coordinate.
    // Desert y boundary: 335 to xxx
    // NOT IN USE
    /*public void Desert()
    {
        if (!inDesert)
        {
            if (transform.position.y > desertStart && transform.position.y < desertEnd)
                inDesert = true;
        }
        else
        {
            if (transform.position.y < desertStart || transform.position.y > desertEnd)
                inDesert = false;
            else
            {
                if (windDuration <= 5)
                {
                    float forceRatio = windDuration / 3.0f * 5;
                    if (forceRatio > 1)
                        forceRatio = 1;

                    body.velocity = new Vector2(body.velocity.x + forceRatio, body.velocity.y);
                }
            }
        }
    }*/
}
