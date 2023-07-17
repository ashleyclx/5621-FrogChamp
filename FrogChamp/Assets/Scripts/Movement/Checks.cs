using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checks : MonoBehaviour
{
    [Header("Script References:")]
    public Movement movement;

    // Boolean variables
    private bool isJumping = false;
    private bool isIcy = false;
    private bool isFalling = false;

    public bool IsGrounded()
    {
        Vector3 boxSize = movement.capsuleCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(movement.capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, movement.platformLayer);
        RaycastHit2D raycastHitIce = Physics2D.BoxCast(movement.capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, movement.icyLayer);
        return raycastHit.collider != null || raycastHitIce.collider != null;

        /*Vector3 boxSize = boxCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;*/
    }

    public void IsFalling()
    {
        if (movement.body.velocity.y == -18 && !isFalling) 
            {
                MakeFalling();
                StatsManager.instance.AddFall();
            }

        if (IsGrounded())
            Invoke("NotFalling", 0.2f);
    }

    public void MakeFalling()
    {
        isFalling = true;
    }

    public void NotFalling()
    {
        isFalling = false;
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    public void ToggleJumpingState(bool set)
    {
        isJumping = set;
    }

    public bool IsIcy()
    {
        return isIcy;
    }

    public void ToggleIcyState(bool set)
    {
        isIcy = set;
    }

    // Check if player contacts platform from the sides (NOT IN USE)
    public bool ContactWall()
    {
        Vector3 boxSize = movement.capsuleCollider.bounds.size;
        boxSize = new Vector3(boxSize.x, boxSize.y - 0.1f, boxSize.z);
        RaycastHit2D raycastHitLeft = Physics2D.BoxCast(movement.capsuleCollider.bounds.center, boxSize, 0, Vector2.left, 0.1f, movement.platformLayer);
        RaycastHit2D raycastHitRight = Physics2D.BoxCast(movement.capsuleCollider.bounds.center, boxSize, 0, Vector2.right, 0.1f, movement.platformLayer);
        return raycastHitLeft.collider != null || raycastHitRight.collider != null;

        /*RaycastHit2D raycastHitLeft = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.left, 0.1f, platformLayer);
        RaycastHit2D raycastHitRight = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.right, 0.1f, platformLayer);
        return raycastHitLeft.collider != null || raycastHitRight.collider != null;*/
    }

    // Setting terminal velocity of player
    public void TerminalVelocity()
    {
        if (movement.body.velocity.y < -18)
            movement.body.velocity = new Vector2(movement.body.velocity.x, -18);
        if (movement.body.velocity.x > 10)
            movement.body.velocity = new Vector2(10, movement.body.velocity.y);
        if (movement.body.velocity.x < -10)
            movement.body.velocity = new Vector2(-10, movement.body.velocity.y);
    }
}
