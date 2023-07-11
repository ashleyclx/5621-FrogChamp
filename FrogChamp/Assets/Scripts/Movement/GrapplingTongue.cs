using UnityEngine;

public class GrapplingTongue : MonoBehaviour
{
    [Header("Script References:")]
    public Movement movement;
    public LineRenderer m_lineRenderer;
    public Checks checks;
    public Camera m_camera;
    public Transform tongueHolder;
    public Transform firePoint;
    public SpringJoint2D m_springJoint2D;

    [Header("General References:")]
    [SerializeField] private int grappableLayerNumber = 8;
    [SerializeField] private int precision = 40;
    [SerializeField] public float maxDistance = 10;
    [HideInInspector] public bool isGrappling = true;

    public Vector2 grapplePoint;
    public Vector2 grappleDistanceVector;
    private Vector2 mousePos;

    private void Awake()
    {
        enabled = false;
        m_springJoint2D.enabled = false;
    }

    private void OnEnable()
    {
        m_lineRenderer.positionCount = precision;
        LinePointsToFirePoint();
        m_lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        m_lineRenderer.enabled = false;
        isGrappling = false;
    }

    private void Update()
    {
        DrawTongue();

        // When left mouse button is not held down.
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
    }
    
    // Checks if grapple point selected is valid and is in radius range.
    // Player cannot grapple when not grounded.
    public void SetGrapplePoint()
    {
        Vector2 distanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - firePoint.position;
        if (Physics2D.Raycast(firePoint.position, distanceVector.normalized))
        {
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized, distanceVector.magnitude);
            if (hit.transform.gameObject.layer == grappableLayerNumber && checks.IsGrounded())
            {
                Vector2 grappleObjectCentre = hit.transform.position - firePoint.position;
                RaycastHit2D hitCentre = Physics2D.Raycast(firePoint.position, grappleObjectCentre.normalized, grappleObjectCentre.magnitude);
                if (Vector2.Distance(hitCentre.point, firePoint.position) <= maxDistance)
                {
                    grapplePoint = hitCentre.point;
                    grappleDistanceVector = grapplePoint - (Vector2)firePoint.position;
                    enabled = true;
                }
            }
        }
    }

    public void DuringGrapple()
    {
        if (!enabled)
                mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);

            if (isGrappling)
            {
                movement.body.velocity = Vector2.zero;
                Vector2 firePointDistance = firePoint.position - tongueHolder.localPosition;
                Vector2 targetPos = grapplePoint - firePointDistance;
                tongueHolder.position = Vector2.Lerp(tongueHolder.position, targetPos, Time.deltaTime * 3);
            }
    }

    public void ReleaseGrapple()
    {
        if (enabled)
            {
                movement.body.velocity = new Vector2(movement.horizontalInput * movement.horizontalJumpSpeed * 1.5f, movement.body.velocity.y);
                enabled = false;
                m_springJoint2D.enabled = false;
                // body.gravityScale = 4.0F;
            }
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }

    private void DrawTongue()
    {
        if (!isGrappling)
        {
            movement.body.velocity = Vector2.zero;
            isGrappling = true;
        }
        if (m_lineRenderer.positionCount != 2) { m_lineRenderer.positionCount = 2; }
        m_lineRenderer.SetPosition(0, firePoint.position);
        m_lineRenderer.SetPosition(1, grapplePoint);
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < precision; i++)
        {
            m_lineRenderer.SetPosition(i, firePoint.position);
        }
    }
}
