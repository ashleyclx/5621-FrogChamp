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
    [SerializeField] private float marshEnd = 330;
    [SerializeField] private float iceStart = 410;
    [SerializeField] private float iceEnd = 488;
    [SerializeField] private float spaceStart = 488;
    [SerializeField] private float spaceEnd = 580.5f;

    private bool inMarsh = false;
    private bool inSpace = false;

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
        float ratio = 0.65f;
        if (!inMarsh) 
        {
            if (transform.position.y > marshStart && transform.position.y < marshEnd)
            {
                ScaleMovement(ratio, ratio, ratio, 1);
                movement.body.velocity = new Vector2(movement.body.velocity.x, ratio * movement.body.velocity.y);
                inMarsh = true;
            }
        }
        else
        {
            if (transform.position.y < marshStart || transform.position.y > marshEnd)
            {
                float inverse = 1.0f / ratio;
                ScaleMovement(inverse, inverse, inverse, 1);
                movement.body.velocity = new Vector2(movement.body.velocity.x, inverse * movement.body.velocity.y);
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

    // Scale player movement when entering/exiting space area based on y coordinate.
    // Space y boundary: 488 to 580.5
    public void Space()
    {
        float gravityRatio = 0.75f;
        float speedRatio = 1.05f;
        if (!inSpace) 
        {
            if (transform.position.y > spaceStart && transform.position.y < spaceEnd)
            {
                ScaleMovement(gravityRatio, speedRatio, speedRatio, speedRatio);
                movement.body.velocity = new Vector2(movement.body.velocity.x, speedRatio * movement.body.velocity.y);
                inSpace = true;
            }
        }
        else
        {
            if (transform.position.y < spaceStart || transform.position.y > spaceEnd)
            {
                float gravityInverse = 1.0f / gravityRatio;
                float speedInverse = 1.0f / speedRatio;
                ScaleMovement(gravityInverse, speedInverse, speedInverse, speedInverse);
                movement.body.velocity = new Vector2(movement.body.velocity.x, speedInverse * movement.body.velocity.y);
                inSpace = false;
            }
        }
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
