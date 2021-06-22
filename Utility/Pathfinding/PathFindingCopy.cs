using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class PathFindingCopy
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    
    private int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

    public PathNode[] GetPathNodeArray()
    {
        GridUtil<TileGridObj> grid = WorldController.current.bigDaddyGrid; //this is the tile map

        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());

        PathNode[] pathNodeArray = new PathNode[gridSize.x * gridSize.y];

        //using tile map to build path node grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNode pathNode = new PathNode();
                pathNode.x = x;
                pathNode.y = y;
                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;

                if (grid.GetGridObject(x, y) == null)
                {
                    Debug.Log(x + "," + y + " is Null" + "\n" + grid.GetWidth());
                }
                pathNode.SetIsWalkable(grid.GetGridObject(x, y).IsWalkable());
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        return pathNodeArray;
    }

    public static PathData FindPathJob(int2 startPos, int2 endPos, PathNode[] pathNodeArray, int2 gridSize)
    {
        //Set Up PathNode Array
        for (int i = 0; i<pathNodeArray.Length; i++)
        {
            PathNode pathNode = pathNodeArray[i];
            pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPos);
            pathNode.cameFromNodeIndex = -1;

            pathNodeArray[i] = pathNode;
        }

        //Get our end node so we know when we arrive
        int endNodeIndex = CalculateIndex(endPos.x, endPos.y, gridSize.x);

        //SetUp Start Node
        PathNode startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
        startNode.gCost = 0;
        startNode.CalculateFCost();
        pathNodeArray[startNode.index] = startNode;

        //prep lists
        List<int> openList = new List<int>();
        List<int> closedList = new List<int>();

        openList.Add(startNode.index);

        //loop time
        while (openList.Count > 0)
        {
            int currentNodeIndex = GetLowestCostFNodeIndex();
            PathNode currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endNodeIndex)
            {
                //We are there
                List<int2> path = CalculatePath();
                return new PathData() { Path = path };
            }

            //Remove current node from list
            openList.Remove(currentNodeIndex);
            
            closedList.Add(currentNodeIndex);
            List<int2> neighbourOffsetList = GetNeighbourList(currentNode, true);

            for (int i = 0; i < neighbourOffsetList.Count; i++)
            {
                int2 neighbourOffset = neighbourOffsetList[i];
                int2 neighbourPos = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);
                    
                bool IsPositionInsideGrid = neighbourPos.x >= 0 && neighbourPos.y >= 0 && neighbourPos.x < gridSize.x && neighbourPos.y < gridSize.y;

                if (IsPositionInsideGrid == false)
                {
                    //Outside of grid
                    continue;
                }

                int neighbourNodeIndex = CalculateIndex(neighbourPos.x, neighbourPos.y, gridSize.x);

                if (closedList.Contains(neighbourNodeIndex) == true)
                {
                    //Already searched
                    continue;
                }

                PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                if (neighbourNode.IsWalkable() == false)
                {
                    //Not walkable
                    continue;
                }

                int2 currentNodePostition = new int2(currentNode.x, currentNode.y);

                //Guesstimate G cost
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePostition, neighbourPos);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    //Cost is less from this node so use this instead
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(new int2(neighbourNode.x, neighbourNode.y), endPos);
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;

                    if (openList.Contains(neighbourNode.index) == false)
                    {
                        openList.Add(neighbourNode.index);
                    }
                }

            }
            neighbourOffsetList.Clear();
        }

        openList.Clear();
        closedList.Clear();

        return new PathData();
        
        List<int2> CalculatePath()
        {
            //Getting end node
            PathNode endNode = pathNodeArray[endNodeIndex];

            if (endNode.cameFromNodeIndex == -1)
            {
                //Didnt Find Path
                return null;
            }
            else
            {
                //Path 
                List<int2> path = new List<int2>();
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }
                return path;
            }
        }

        List<int2> GetNeighbourList(PathNode currentNode, bool noCuttingCorners = true)
        {
            List<int2> neighbourList = new List<int2>();

            //Bools used later to check diagonals
            bool n = false;
            bool e = false;
            bool s = false;
            bool w = false;

            //Check N,E,S,W
            if (currentNode.y + 1 < gridSize.x)//N
            {
                PathNode node = pathNodeArray[CalculateIndex(currentNode.x, currentNode.y + 1, gridSize.x)];
                neighbourList.Add(new int2(0, +1));

                if (noCuttingCorners == true && node.IsWalkable() == false)
                {
                    //this node is unpassable so we cannot cut corners to a diagonal tile.
                    n = false;
                }
                else
                {
                    //this node is passabe so we can cut corners to a diagonal tile
                    n = true;
                }

            }

            if (currentNode.x + 1 < gridSize.x)//E
            {
                PathNode node = pathNodeArray[CalculateIndex(currentNode.x + 1, currentNode.y, gridSize.x)];
                neighbourList.Add(new int2(+1, 0));

                if (noCuttingCorners == true && node.IsWalkable() == false)
                {
                    e = false;
                }
                else
                {
                    e = true;
                }
            }

            if (currentNode.y - 1 >= 0)//S
            {
                PathNode node = pathNodeArray[CalculateIndex(currentNode.x, currentNode.y - 1, gridSize.x)];
                neighbourList.Add(new int2(0, -1));

                if (noCuttingCorners == true && node.IsWalkable() == false)
                {
                    s = false;
                }
                else
                {
                    s = true;
                }
            }

            if (currentNode.x - 1 >= 0)//W
            {
                PathNode node = pathNodeArray[CalculateIndex(currentNode.x - 1, currentNode.y, gridSize.x)];
                neighbourList.Add(new int2(-1, 0));

                if (noCuttingCorners == true && node.IsWalkable() == false)
                {
                    w = false;
                }
                else
                {
                    w = true;
                }
            }

            //CheckDiagonals
            //If both are passable we can add the diagonal tile to the check list
            if (n == true && e == true)//NE
            {
                neighbourList.Add(new int2(+1, +1));
            }
            if (e == true && s == true)//SE
            {
                neighbourList.Add(new int2(+1, -1));
            }
            if (s == true && w == true)//SW
            {
                neighbourList.Add(new int2(-1, -1));
            }
            if (w == true && n == true)//NW
            {
                neighbourList.Add(new int2(-1, +1));
            }

            return neighbourList;
        }

        int GetLowestCostFNodeIndex()
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 0; i < openList.Count; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                //if this nodes Fcost is lower we make a return this one unless we find a lower one
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }

            return lowestCostPathNode.index;
        }

        int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);

            //Diagonal cost * the shortest of the two distances
            //this assumes we can move diagonally  across this with no blocking tiles
            //+ 
            //straight cost * the remaining distance after the shortest was taken from the longest.
            //
            //so this will return a cost as the crow flies
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }
    }
}
