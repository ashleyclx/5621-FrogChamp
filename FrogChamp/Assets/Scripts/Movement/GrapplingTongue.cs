using UnityEngine;

public class GrapplingTongue : MonoBehaviour
{
    [Header("General References:")]
    public Movement movement;
    public LineRenderer m_lineRenderer;

    [SerializeField] private int precision = 40;
    [HideInInspector] public bool isGrappling = true;

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
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < precision; i++)
        {
            m_lineRenderer.SetPosition(i, movement.firePoint.position);
        }
    }

    void DrawTongue()
    {
        if (!isGrappling)
        {
            movement.Grapple();
            isGrappling = true;
        }
        if (m_lineRenderer.positionCount != 2) { m_lineRenderer.positionCount = 2; }
        m_lineRenderer.SetPosition(0, movement.firePoint.position);
        m_lineRenderer.SetPosition(1, movement.grapplePoint);
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }
        
}
