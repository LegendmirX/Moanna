using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        float cellSize = radius / Mathf.Sqrt(2); //this is how to get the size of the side from the diagonal

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(new Vector2( Random.Range(0, sampleRegionSize.x), Random.Range(0, sampleRegionSize.y)));
        while(spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)); //these 2 lines pick a direction
                Vector2 candidatePoint = spawnCentre + dir * Random.Range(radius, 2 * radius);//this line picks a point along that direction to place our point on.
                
                if(IsValid(candidatePoint, sampleRegionSize, cellSize, radius, points, grid) == true)
                {
                    points.Add(candidatePoint);
                    spawnPoints.Add(candidatePoint);
                    grid[(int)(candidatePoint.x / cellSize), (int)(candidatePoint.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }

            if(candidateAccepted == false)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        return points;
    }

    static bool IsValid(Vector2 candidatePoint, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if(candidatePoint.x >= 0 && candidatePoint.x < sampleRegionSize.x && candidatePoint.y >= 0 && candidatePoint.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidatePoint.x / cellSize);
            int cellY = (int)(candidatePoint.y / cellSize);

            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);

            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if(pointIndex != -1)
                    {
                        float sqrDistance = (candidatePoint - points[pointIndex]).sqrMagnitude;
                        if(sqrDistance < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}
