using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Tween        _drawTween;

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

    public void AnimateDraw(Vector3[] pathPoints, float duration)
    {
        _drawTween?.Kill();

        if (pathPoints == null || pathPoints.Length < 2)
        {
            Hide();
            return;
        }

        float totalLength = 0;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            totalLength += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }

        float currentLength = 0;
        _drawTween = DOTween.To(() => currentLength,
                x => currentLength = x,
                totalLength,
                duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                UpdateLine(pathPoints, currentLength);
            });
    }

    private void UpdateLine(Vector3[] originalPath, float travelledLength)
    {
        List<Vector3> pointsToShow = new List<Vector3>();
        pointsToShow.Add(originalPath[0]);

        float lengthSoFar = 0;
        for (int i = 0; i < originalPath.Length - 1; i++)
        {
            Vector3 startPoint    = originalPath[i];
            Vector3 endPoint      = originalPath[i + 1];
            float   segmentLength = Vector3.Distance(startPoint, endPoint);

            if (lengthSoFar + segmentLength >= travelledLength)
            {
                float   lengthNeeded = travelledLength - lengthSoFar;
                float   percentage   = lengthNeeded / segmentLength;
                Vector3 newEndPoint  = Vector3.Lerp(startPoint, endPoint, percentage);
                pointsToShow.Add(newEndPoint);
                break;
            }
            else
            {
                lengthSoFar += segmentLength;
                pointsToShow.Add(endPoint);
            }
        }

        lineRenderer.positionCount = pointsToShow.Count;
        lineRenderer.SetPositions(pointsToShow.ToArray());
        lineRenderer.enabled = true;
    }

    /// <summary>
    /// Hides the line.
    /// </summary>
    public void Hide()
    {
        _drawTween?.Kill();
        lineRenderer.enabled       = false;
        lineRenderer.positionCount = 0;
    }
}