using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private int speed;
    [SerializeField] private int jumpSpeed;
    [SerializeField] private float maxHoldDuration;
    [SerializeField] private LayerMask platformLayer;

    private float horizontalInput;
    private float holdDuration;
    private Rigidbody2D body;
    // private BoxCollider2D boxCollider;
    private EdgeCollider2D[] edgeColliderArray;
    

    // Awake is called every time the script is loaded
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        // boxCollider = GetComponent<BoxCollider2D>();
        edgeColliderArray = GetComponentsInChildren<EdgeCollider2D>();
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

        // Press down space to start charging jump
        if (Input.GetKeyDown(KeyCode.Space))
            holdDuration = 0.0f;

        // While space is held, charge up jump (by increasing delta time)
        if (Input.GetKey(KeyCode.Space))
            holdDuration += Time.deltaTime;

        // Let go of space to jump. Player can only jump when grounded
        if (Input.GetKeyUp(KeyCode.Space) && IsGrounded())
            Jump();

        // Player bounces back when he collides into a platform
        if (ContactWall() && !IsGrounded())
            Bounce();

        Debug.Log(IsGrounded());
    }

    // Jump in a fixed arc based on your directional input
    private void Jump()
    {
        // calculate factor to see how charged a jump is
        float holdFactor = holdDuration / maxHoldDuration > 0.65f ? 0.65f : holdDuration / maxHoldDuration;
        holdFactor += 0.35f;
        // have a minimum jump strength of 35%
        // holdFactor = holdFactor < 0.35f ? 0.35f : holdFactor;

        // if left or right is pressed, jump left or right
        // if no directional input is pressed, jump upwards
        if (horizontalInput > 0.01f) 
            body.velocity = new Vector2(speed, holdFactor * jumpSpeed);
        else if (horizontalInput < -0.01f)
            body.velocity = new Vector2(-speed, holdFactor * jumpSpeed);
        else if(horizontalInput == 0.0f)
            body.velocity = new Vector2(0, holdFactor * jumpSpeed);
    }

    // Bounce away from platform when player contacts platform from the sides
    private void Bounce()
    {
        body.velocity = new Vector2(-body.velocity.x, body.velocity.y);
    }

    // Check if player is on the platorm
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(edgeColliderArray[3].bounds.center, edgeColliderArray[3].bounds.size, 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;

        /*RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;*/
    }

    // Check if player contacts platform from the sides
    private bool ContactWall()
    {
        foreach (EdgeCollider2D edgeCollider in edgeColliderArray)
        {
            RaycastHit2D raycastHitLeft = Physics2D.BoxCast(edgeCollider.bounds.center, edgeCollider.bounds.size, 0, Vector2.left, 0.1f, platformLayer);
            RaycastHit2D raycastHitRight = Physics2D.BoxCast(edgeCollider.bounds.center, edgeCollider.bounds.size, 0, Vector2.right, 0.1f, platformLayer);
            if (raycastHitLeft.collider != null || raycastHitRight.collider != null)
                return true;
        }
        return false;

        /*RaycastHit2D raycastHitLeft = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.left, 0.1f, platformLayer);
        RaycastHit2D raycastHitRight = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.right, 0.1f, platformLayer);
        return raycastHitLeft.collider != null || raycastHitRight.collider != null;*/
    }
}
