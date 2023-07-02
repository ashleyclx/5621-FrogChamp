using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // variables for grapple mechanics

    [Header("Grapple references:")]
    public GrapplingTongue grappleTongue;
    [SerializeField] private int grappableLayerNumber = 8;
    public Camera m_camera;
    public Transform tongueHolder;
    public Transform firePoint;
    public SpringJoint2D m_springJoint2D;
    [SerializeField] private float maxDistance = 10;

    public Vector2 grapplePoint;
    public Vector2 grappleDistanceVector;

    [Header("Jump references:")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float horizontalJumpSpeed;
    [SerializeField] private float maxHoldDuration;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private PhysicsMaterial2D bounceMaterial;
    [SerializeField] private PhysicsMaterial2D noBounceMaterial;

    [Header("Sound references:")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioClip windSound;

    [Header("Area references:")]
    [SerializeField] private float windSpeed;
    [SerializeField] private float marshStart = 197;
    [SerializeField] private float marshEnd = 335;
    [SerializeField] private float iceStart = 410;
    [SerializeField] private float iceEnd = 600;
    [SerializeField] private LayerMask icyLayer;
    [SerializeField] private float slipperyFactor;
    private bool inMarsh = false;
    private bool icy = false;
    private bool isJumping = false;

    private float horizontalInput;
    private float holdDuration;
    private Rigidbody2D body;
    // private BoxCollider2D boxCollider;
    private CapsuleCollider2D capsuleCollider;

    private Animator animator;

    

    // Awake is called every time the script is loaded
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        // boxCollider = GetComponent<BoxCollider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        grappleTongue.enabled = false;
        m_springJoint2D.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // For Milestone2 testing
        PlayerTeleport();

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Player Movement
        TerminalVelocity();
        Bounce();
        if (IsGrounded())
        {
            Walk();
            Charge();
            Jump();
            FlipPlayerDirection();
        }
        Landing();

        // Grapple logic

        // When player chooses a grapple point. Checks if grapple point is valid and starts the grapple process.
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SetGrapplePoint();
        }
        
        // When player is holding down left mouse button (grappling).
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (!grappleTongue.enabled)
            {
                Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            }

            if (grappleTongue.isGrappling)
            {
                body.velocity = Vector2.zero;
                Vector2 firePointDistance = firePoint.position - tongueHolder.localPosition;
                Vector2 targetPos = grapplePoint - firePointDistance;
                tongueHolder.position = Vector2.Lerp(tongueHolder.position, targetPos, Time.deltaTime * 3);
            }
        }

        // When player releases the left mouse button (releases grapple).
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (grappleTongue.enabled)
            {
                body.velocity = new Vector2(horizontalInput * horizontalJumpSpeed * 1.5f, body.velocity.y);
                grappleTongue.enabled = false;
                m_springJoint2D.enabled = false;
                // body.gravityScale = 4.0F;
            }
        }

        // When left mouse button is not held down.
        else
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        }

        // Set animator parameters
        if (IsGrounded() && holdDuration == 0.0f)
            animator.SetBool("walk", horizontalInput != 0);
        animator.SetBool("grounded", IsGrounded());
        animator.SetBool("charging", holdDuration > 0.0f);

        // Area updates
        Marsh();
        Ice();
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
        if (icy && !Input.GetKey(KeyCode.Space))
            body.AddForce(new Vector2(slipperyFactor * horizontalInput, 0), ForceMode2D.Force);

        // When space is held, player should not be able to move (for charging jump strength)
        else if (!icy && !Input.GetKey(KeyCode.Space))
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
    }

    // Holding down space to charge up jump
    private void Charge()
    {
        // Press down space to start charging jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            holdDuration = 0.0f;
            body.sharedMaterial = bounceMaterial;

            // player slides while charging if on ice
            // player stops if not on ice
            if (!icy)
                body.velocity = new Vector2(0.0f, body.velocity.y);
            
        }

        // While space is held, charge up jump (by increasing delta time)
        if (Input.GetKey(KeyCode.Space))
        {
            holdDuration += Time.deltaTime;

            // player stops if not on ice
            if (!icy)
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
            SoundManager.instance.PlaySound(jumpSound);

        }
    }

    // Landing after jump
    private void Landing()
    {
        if (body.velocity.y < -5 && holdDuration > 0)
            isJumping = true;

        if (isJumping && IsGrounded())
        {
            // Plays landing sound
            SoundManager.instance.PlaySound(landingSound);
            isJumping = false;

            // reset hold duration after jump
            holdDuration = 0.0f;
        }
    }

    // Bounce away from platform when player contacts platform from the sides
    private void Bounce()
    {
        // Changes bounciness based on vertical velocity
        if (body.velocity.y <= -1)
            body.sharedMaterial = noBounceMaterial;

        // Player bounces back when he collides into a platform [NOT IN USE]
        /*if (ContactWall() && !IsGrounded())
            body.velocity = new Vector2(-body.velocity.x, body.velocity.y);*/
        
    }

    // Flip player model when changing directions on ground
    private void FlipPlayerDirection()
    {
        if (Time.timeScale != 0)
        {
            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f && IsGrounded())
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    // Setting terminal velocity of player
    private void TerminalVelocity()
    {
        if (body.velocity.y < -18)
            body.velocity = new Vector2(body.velocity.x, -18);
        if (body.velocity.x > 10)
            body.velocity = new Vector2(10, body.velocity.y);
        if (body.velocity.x < -10)
            body.velocity = new Vector2(-10, body.velocity.y);
    }

    #endregion

    #region Grapple
    // Checks if grapple point selected is valid and is in radius range.
    // Player cannot grapple when not grounded.
    void SetGrapplePoint()
    {

        Vector2 distanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - firePoint.position;
        if (Physics2D.Raycast(firePoint.position, distanceVector.normalized))
        {
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized, distanceVector.magnitude);
            if (hit.transform.gameObject.layer == grappableLayerNumber && IsGrounded())
            {
                Vector2 grappleObjectCentre = hit.transform.position - firePoint.position;
                RaycastHit2D hitCentre = Physics2D.Raycast(firePoint.position, grappleObjectCentre.normalized, grappleObjectCentre.magnitude);
                if (Vector2.Distance(hitCentre.point, firePoint.position) <= maxDistance)
                {
                    grapplePoint = hitCentre.point;
                    grappleDistanceVector = grapplePoint - (Vector2)firePoint.position;
                    grappleTongue.enabled = true;
                }
            }
        }
    }

    public void Grapple()
    {
        // body.gravityScale = 0;
        body.velocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistance);
        }
    }
    #endregion

    #region Checks
    // Check if player is on the platform
    private bool IsGrounded()
    {
        Vector3 boxSize = capsuleCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, platformLayer);
        RaycastHit2D raycastHitIce = Physics2D.BoxCast(capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, icyLayer);
        return raycastHit.collider != null || raycastHitIce.collider != null;

        /*Vector3 boxSize = boxCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;*/
    }

    // Check if player contacts platform from the sides (NOT IN USE)
    private bool ContactWall()
    {
        Vector3 boxSize = capsuleCollider.bounds.size;
        boxSize = new Vector3(boxSize.x, boxSize.y - 0.1f, boxSize.z);
        RaycastHit2D raycastHitLeft = Physics2D.BoxCast(capsuleCollider.bounds.center, boxSize, 0, Vector2.left, 0.1f, platformLayer);
        RaycastHit2D raycastHitRight = Physics2D.BoxCast(capsuleCollider.bounds.center, boxSize, 0, Vector2.right, 0.1f, platformLayer);
        return raycastHitLeft.collider != null || raycastHitRight.collider != null;

        /*RaycastHit2D raycastHitLeft = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.left, 0.1f, platformLayer);
        RaycastHit2D raycastHitRight = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.right, 0.1f, platformLayer);
        return raycastHitLeft.collider != null || raycastHitRight.collider != null;*/
    }
    #endregion

    #region Terrains
    // Scales gravity of player by a factor
    public void ScaleMovement(float _gravity, float _speed, float _jumpSpeed, float _horizontalJumpSpeed)
    {
        body.gravityScale *= _gravity;
        speed *= _speed;
        jumpSpeed *= _jumpSpeed;
        horizontalJumpSpeed *= _horizontalJumpSpeed;
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
                body.velocity = new Vector2(body.velocity.x, 0.65f * body.velocity.y);
                inMarsh = true;
            }
        }
        else
        {
            if (transform.position.y < marshStart || transform.position.y > marshEnd)
            {
                float inverse = 1.0f / 0.65f;
                ScaleMovement(inverse, inverse, inverse, 1);
                body.velocity = new Vector2(body.velocity.x, 2.0f * body.velocity.y);
                inMarsh = false;
            }
        }
    }

    // Check if player is on ice
    // By default if above y = 410 and below y = 600, player is on ice unless we detect a platform layer
    private void Ice()
    {
        Vector3 boxSize = capsuleCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, platformLayer);
        if (transform.position.y > iceStart && transform.position.y < iceEnd)
            icy = true;
        if (raycastHit.collider != null)
            icy = false;

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
    #endregion
}
