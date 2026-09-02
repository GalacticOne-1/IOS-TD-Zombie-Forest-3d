using UnityEngine;
using System.Collections.Generic;
using Galactic1;

public class ObstacleDetector2D
{
    public LayerMask obstacleLayer;


    
    

    public ObstacleDetector2D(LayerMask obstacleLayer)
    {
        this.obstacleLayer = obstacleLayer;
    }


    /// <summary>
    /// Check for one cell
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool GetObstructedPoints(Vector2 point)
    {
        Collider2D hit = Physics2D.OverlapCircle(point, AppConstants.DetectionRadius, obstacleLayer);
        return hit != null;
    }

    /// <summary>
    /// Check which points in the array are obstructed by obstacles.
    /// </summary>
    /// <param name="points">Array of positions to check.</param>
    /// <returns>List of blocked points.</returns>
    public List<Vector2> GetObstructedPoints(Vector2[] points)
    {
        List<Vector2> blockedPoints = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            // Use OverlapCircle for a more robust check
            Collider2D hit = Physics2D.OverlapCircle(point, AppConstants.DetectionRadius, obstacleLayer);

            if (hit != null)
            {
                blockedPoints.Add(point);
                Debug.Log($"Obstacle detected at {point}");
            }
        }

        return blockedPoints;
    }

}
