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
    [SerializeField] private int speed;
    [SerializeField] private int jumpSpeed;
    [SerializeField] private float maxHoldDuration;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private PhysicsMaterial2D bounceMaterial;
    [SerializeField] private PhysicsMaterial2D noBounceMaterial;

    private float horizontalInput;
    private float holdDuration;
    private Rigidbody2D body;
    // private BoxCollider2D boxCollider;
    private CapsuleCollider2D capsuleCollider;

    private void Start()
    {
        grappleTongue.enabled = false;
        m_springJoint2D.enabled = false;
    }
    

    // Awake is called every time the script is loaded
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        // boxCollider = GetComponent<BoxCollider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
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
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            holdDuration = 0.0f;
            body.velocity = new Vector2(0.0f, body.velocity.y);
            body.sharedMaterial = bounceMaterial;
        }

        // While space is held, charge up jump (by increasing delta time)
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
            holdDuration += Time.deltaTime;

        // Let go of space to jump. Player can only jump when grounded
        if (Input.GetKeyUp(KeyCode.Space) && IsGrounded())
        {
            Jump();
            holdDuration = 0.0f;
        }
            

        if (body.velocity.y <= -1)
            body.sharedMaterial = noBounceMaterial;

        // Player bounces back when he collides into a platform
        /*if (ContactWall() && !IsGrounded())
            Bounce();*/

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
                body.velocity = new Vector2(GetPlayerDirection() * speed, body.velocity.y);
                grappleTongue.enabled = false;
                m_springJoint2D.enabled = false;
                body.gravityScale = 2.5F;
            }
        }

        // When left mouse button is not held down.
        else
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        }

    }

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
                if (Vector2.Distance(hit.point, firePoint.position) <= maxDistance)
                {
                    grapplePoint = hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)firePoint.position;
                    grappleTongue.enabled = true;
                }
            }
        }
    }

    public void Grapple()
    {
        body.gravityScale = 0;
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
        body.velocity = new Vector2(GetPlayerDirection() * speed, holdFactor * jumpSpeed);
    }

    // Returns 1 if player is facing right, -1 if player is facing left, else returns 0.
    private int GetPlayerDirection()
    {
        if (horizontalInput > 0.01f) { return 1; }
        else if (horizontalInput < -0.01f) { return -1; }
        else { return 0; }

    }

    // Bounce away from platform when player contacts platform from the sides
    private void Bounce()
    {
        body.velocity = new Vector2(-body.velocity.x, body.velocity.y);
    }

    // Check if player is on the platform
    private bool IsGrounded()
    {
        Vector3 boxSize = capsuleCollider.bounds.size;
        RaycastHit2D raycastHit = Physics2D.BoxCast(capsuleCollider.bounds.center, new Vector3(boxSize.x - 0.1f, boxSize.y, boxSize.z), 0, Vector2.down, 0.1f, platformLayer);
        return raycastHit.collider != null;

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
}
