using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // Hide it initially.
        Hide();
    }

    /// <summary>
    /// Draws a line using an array of world positions.
    /// </summary>
    public void Draw(Vector3[] points)
    {
        if (points == null || points.Length < 2)
        {
            Hide();
            return;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
    }

    /// <summary>
    /// Hides the line.
    /// </summary>
    public void Hide()
    {
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
    }
}