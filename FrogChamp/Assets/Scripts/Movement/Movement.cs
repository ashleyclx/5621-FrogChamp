using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Script references:")]
    public Area area;
    public Checks checks;
    public GrapplingTongue grappleTongue;
    public SoundPlayer soundPlayer;
    private Animator animator;

    [Header("Jump references:")]
    [SerializeField] public float speed;
    [SerializeField] public float jumpSpeed;
    [SerializeField] public float horizontalJumpSpeed;
    [SerializeField] private float maxHoldDuration;
    [SerializeField] private PhysicsMaterial2D bounceMaterial;
    [SerializeField] private PhysicsMaterial2D noBounceMaterial;

    [Header("Layer references:")]
    [SerializeField] public LayerMask icyLayer;
    [SerializeField] public LayerMask platformLayer;
    [SerializeField] private float slipperyFactor;

    // General variables
    public float horizontalInput;
    private float holdDuration;
    [HideInInspector] public Rigidbody2D body;
    [HideInInspector] public CapsuleCollider2D capsuleCollider;
    // private BoxCollider2D boxCollider;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        // boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        TimeManager.instance.BeginTimer();
    }

    private void Update()
    {
        // Setting horizontalInput
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // For Milestone2 testing
        PlayerTeleport();

        // Player Movement
        checks.TerminalVelocity();
        Bounce();
        Landing();
        Grapple();

        if (checks.IsGrounded())
        {
            Walk();
            Charge();
            Jump();
            FlipPlayerDirection();
        }

        // Setting animations
        SetAnimation();

        // Area updates
        area.Marsh();
        area.Ice();
        area.Space();
    }

    #region Player Movement
    // For Milestone2 testing
    private void PlayerTeleport()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            transform.position = new Vector2(3.53f, 30f);
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
            transform.position = new Vector2(-8f, 103f);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            transform.position = new Vector2(6f, 225f);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            transform.position = new Vector2(5f, 353f);
    }

    // Moves left or right when arrow keys/a or d are pressed based on set speed
    private void Walk()
    {
        // if on ice, player will slide
        if (checks.IsIcy() && !Input.GetKey(KeyCode.Space))
            body.AddForce(new Vector2(slipperyFactor * horizontalInput, 0), ForceMode2D.Force);

        // When space is held, player should not be able to move (for charging jump strength)
        else if (!checks.IsIcy() && !Input.GetKey(KeyCode.Space))
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
    }

    // Holding down space to charge up jump
    private void Charge()
    {
        // Press down space to start charging jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            body.sharedMaterial = bounceMaterial;

            // player slides while charging if on ice
            // player stops if not on ice
            if (!checks.IsIcy())
                body.velocity = new Vector2(0.0f, body.velocity.y);
            
        }

        // While space is held, charge up jump (by increasing delta time)
        if (Input.GetKey(KeyCode.Space))
        {
            holdDuration += Time.deltaTime;

            // player stops if not on ice
            if (!checks.IsIcy())
                body.velocity = new Vector2(0.0f, body.velocity.y);
        }
    }

    // Jump in a fixed arc based on your directional input
    private void Jump()
    {
        // Let go of space to jump. Player can only jump when grounded
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // calculate factor to see how charged a jump is
            float holdFactor = holdDuration / maxHoldDuration > 0.65f ? 0.65f : holdDuration / maxHoldDuration;

            // have a minimum jump strength of 35%
            holdFactor += 0.35f;

            // if left or right is pressed, jump left or right
            // if no directional input is pressed, jump upwards
            body.velocity = new Vector2(horizontalInput * speed, holdFactor * jumpSpeed);

            // Plays jump sound
            soundPlayer.JumpSound();

        }
    }

    // Landing after jump
    private void Landing()
    {
        if (body.velocity.y < -5 && holdDuration > 0)
            checks.ToggleJumpingState(true);

        if (checks.IsJumping() && checks.IsGrounded())
        {
            // Plays landing sound
            soundPlayer.LandingSound();
            checks.ToggleJumpingState(false);

            // reset hold duration after jump
            holdDuration = -0.05f;
        }
    }

    // Bounce away from platform when player contacts platform from the sides
    private void Bounce()
    {
        // Changes bounciness based on vertical velocity
        if (body.velocity.y <= -1)
            body.sharedMaterial = noBounceMaterial;

        // Player bounces back when he collides into a platform [NOT IN USE]
        /*if (ContactWall() && !checks.IsGrounded())
            body.velocity = new Vector2(-body.velocity.x, body.velocity.y);*/
        
    }

    // Flip player model when changing directions on ground
    private void FlipPlayerDirection()
    {
        if (Time.timeScale != 0)
        {
            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f && checks.IsGrounded())
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Grapple()
    {
        // Grapple logic
        // When player chooses a grapple point. Checks if grapple point is valid and starts the grapple process.
        if (Input.GetKeyDown(KeyCode.Mouse0))
            grappleTongue.SetGrapplePoint();
        
        // When player is holding down left mouse button (grappling).
        else if (Input.GetKey(KeyCode.Mouse0))
            grappleTongue.DuringGrapple();

        // When player releases the left mouse button (releases grapple).
        else if (Input.GetKeyUp(KeyCode.Mouse0))
            grappleTongue.ReleaseGrapple();
    }
    #endregion

    #region Grapple
    private void OnDrawGizmosSelected()
    {
        if (grappleTongue.firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grappleTongue.firePoint.position, grappleTongue.maxDistance);
        }
    }
    #endregion

    #region Animation
    private void SetAnimation()
    {
        // Set animator parameters
        if (checks.IsGrounded() && holdDuration <= 0.0f)
            animator.SetBool("walk", horizontalInput != 0);
        animator.SetBool("grounded", checks.IsGrounded());
        animator.SetBool("charging", holdDuration > 0.0f);
        animator.SetBool("grappling", grappleTongue.IsGrappling());
    }
    #endregion
}
