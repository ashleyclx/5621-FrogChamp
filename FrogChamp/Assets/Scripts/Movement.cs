using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private int speed;
    [SerializeField] private int jumpSpeed;
    [SerializeField] private LayerMask platformLayer;

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
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (IsGrounded() && !Input.GetKey(KeyCode.Space))
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        // flip player model when changing directions
        if (horizontalInput > 0.01f && IsGrounded())
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f && IsGrounded())
            transform.localScale = new Vector3(-1, 1, 1);

        if(Input.GetKeyUp(KeyCode.Space) && IsGrounded())
            Jump();
    }

    private void Jump()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput > 0.01f) 
            body.velocity = new Vector2(speed, jumpSpeed);
        else if (horizontalInput < -0.01f)
            body.velocity = new Vector2(-speed, jumpSpeed);
        else if(horizontalInput == 0.0f)
            body.velocity = new Vector2(0, jumpSpeed);
    }
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;
    }
}
