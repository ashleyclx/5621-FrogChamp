using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private int speed;
    [SerializeField] private int jumpSpeed;
    [SerializeField] private LayerMask platformLayer;

    private float horizontalInput;
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    

    // Awake is called every time the script is loaded
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // When space is held, player should not be able to move (for charging jump strength)
        if (IsGrounded() && !Input.GetKey(KeyCode.Space))
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        // flip player model when changing directions on ground
        if (horizontalInput > 0.01f && IsGrounded())
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f && IsGrounded())
            transform.localScale = new Vector3(-1, 1, 1);

        // Player can only jump when grounded
        if (Input.GetKeyUp(KeyCode.Space) && IsGrounded())
            Jump();

        // Player bounces back when he collides into a platform
        if (ContactWall() && !IsGrounded())
            Bounce();
    }

    // Jump in a fixed arc based on your directional input
    private void Jump()
    {
        if (horizontalInput > 0.01f) 
            body.velocity = new Vector2(speed, jumpSpeed);
        else if (horizontalInput < -0.01f)
            body.velocity = new Vector2(-speed, jumpSpeed);
        else if(horizontalInput == 0.0f) // if no directional input is pressed, jump upwards
            body.velocity = new Vector2(0, jumpSpeed);
    }

    // Bounce away from platform when player contacts platform from the sides
    private void Bounce()
    {
        body.velocity = new Vector2(-body.velocity.x, body.velocity.y);
    }

    // Check if player is on the platorm
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;
    }

    // Check if player contacts platform from the sides
    private bool ContactWall()
    {
        RaycastHit2D raycastHitLeft = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.left, 0.1f, platformLayer);
        RaycastHit2D raycastHitRight = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.right, 0.1f, platformLayer);
        return raycastHitLeft.collider != null || raycastHitRight.collider != null;
    }
}
