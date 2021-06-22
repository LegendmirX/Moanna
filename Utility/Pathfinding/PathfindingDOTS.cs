using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class PathfindingDOTS : ComponentSystem
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    protected override void OnUpdate()
    {

    }

    //protected override void OnUpdate()
    //{
    //    //Getting GridSize
    //    int width = WorldController.current.bigDaddyGrid.GetWidth();
    //    int height = WorldController.current.bigDaddyGrid.GetHeight();
    //    int2 gridSize = new int2(width, height);

    //    //List of path finding jobs and JobHandle list for multithreading
    //    Dictionary<FindPathJob, PathJob> findPathJobsList = new Dictionary<FindPathJob, PathJob>();
    //    NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);

    //    //Getting pathArray for all pathfinding jobs as they can all use a copy of the same one.
    //    if(WorldController.current.bigDaddyGrid == null)
    //    {
    //        Debug.Log("NullGrid");
    //        return;
    //    }
    //    NativeArray<PathNode> pathNodeArray = GetPathNodeArray();
    
    //    ////Finding all entitys with pathfinding params. this means they want to go somewhere       //This is for entitys
    //    //Entities.ForEach((Entity entity, DynamicBuffer<PathBuffer> pathBuffer, ref PathFindingParams pathfindingParams) => 
    //    //{
    //    //    //Path array copy
    //    //    NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(pathNodeArray, Allocator.TempJob);

    //    //    //Job setup
    //    //    FindPathJob findPathJob = new FindPathJob
    //    //    {
    //    //        entity = entity,
    //    //        startPos = pathfindingParams.StartPosition,
    //    //        endPos = pathfindingParams.EndPosition,
    //    //        pathNodeArray = tmpPathNodeArray,
    //    //        gridSize = gridSize
    //    //    };
    //    //    //Adding jobs to lists
    //    //    findPathJobsList.Add(findPathJob);
    //    //    jobHandles.Add(findPathJob.Schedule());

    //    //    //making sure to remove the path request
    //    //    PostUpdateCommands.RemoveComponent<PathFindingParams>(entity);
    //    //});

    //    //Firing off all the jobs
    //    JobHandle.CompleteAll(jobHandles);

    //    //Now to tell all entitys there paths   //This is for entitys
    //    //foreach (FindPathJob findPathJob in findPathJobsList) 
    //    //{
    //    //    //buffer job fills out the path buffer with the path
    //    //    new SetBufferPathJob
    //    //    {
    //    //        entity = findPathJob.entity,
    //    //        gridSize = findPathJob.gridSize,
    //    //        pathNodeArray = findPathJob.pathNodeArray,
    //    //        pathFindingParamsFromEntity = GetComponentDataFromEntity<PathFindingParams>(),
    //    //        pathCurrentNodeFromEntity = GetComponentDataFromEntity<PathCurrentNode>(),
    //    //        pathBufferFromEntity = GetBufferFromEntity<PathBuffer>()
    //    //    }.Run();
    //    //}

    //    pathNodeArray.Dispose();
    //}



    public void FindPath(List<PathJob> jobsList)
    {
        int width = WorldController.current.bigDaddyGrid.GetWidth();
        int height = WorldController.current.bigDaddyGrid.GetHeight();
        int2 gridSize = new int2(width, height);

        //List of path finding jobs and JobHandle list for multithreading
        Dictionary<FindPathJob, PathJob> findPathJobsList = new Dictionary<FindPathJob, PathJob>();
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);
        NativeArray<PathNode> pathNodeArray = GetPathNodeArray();

        foreach (PathJob job in jobsList)
        {
            Debug.Log("Set FindPathJob");
            NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(pathNodeArray, Allocator.TempJob);

            FindPathJob findPathJob = new FindPathJob
            {
                startPos = job.startPoint,
                endPos = job.endPoint,
                pathNodeArray = tmpPathNodeArray,
                gridSize = gridSize
            };
            jobHandles.Add(findPathJob.Schedule());
        }

        JobHandle.CompleteAll(jobHandles);

        foreach (FindPathJob findPathJob in findPathJobsList.Keys)
        {
            Debug.Log("SetPath");
            new SetPathJob
            {
                callBack = findPathJobsList[findPathJob].callBack,
                gridSize = findPathJob.gridSize,
                pathNodeArray = findPathJob.pathNodeArray,
                endPos = findPathJob.endPos
                
            }.Run();

        }
        pathNodeArray.Dispose();
    }

    private NativeArray<PathNode> GetPathNodeArray()
    {
        GridUtil<TileGridObj> grid = WorldController.current.bigDaddyGrid; //this is the tile map

        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());

        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);

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

                if(grid.GetGridObject(x,y) == null)
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


    private int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

    [BurstCompile]
    private struct SetPathJob : IJob
    {
        public int2 gridSize;

        public int2 endPos;

        [DeallocateOnJobCompletionAttribute]
        public NativeArray<PathNode> pathNodeArray;

        public System.Action<List<int2>> callBack;
        

        public void Execute()
        {
            int endNodeIndex = CalculateIndex(endPos.x, endPos.y, gridSize.x);
            PathNode endNode = pathNodeArray[endNodeIndex];

            Debug.Log("Path Job Complete");

            if (endNode.cameFromNodeIndex == -1)
            {
                //Didnt Find Path
                callBack.Invoke(null);
            }
            else
            {
                //We got a path so calculate it and send it back
                callBack.Invoke(CalculatePath(pathNodeArray, endNode));
            }
            pathNodeArray.Dispose();
        }

        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        List<int2> CalculatePath(NativeArray<PathNode> nodeArray, PathNode endNode)
        {
            List<int2> path = new List<int2>();

            if (endNode.cameFromNodeIndex == -1)
            {
                //No path
                return null;
            }
            else
            {
                //Path 
                path = new List<int2>();
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = nodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }
            }

            return path;
        }
    }

    [BurstCompile]
    private struct SetBufferPathJob : IJob
    {
        public int2 gridSize;

        [DeallocateOnJobCompletionAttribute]
        public NativeArray<PathNode> pathNodeArray;

        public Entity entity;

        public ComponentDataFromEntity<PathFindingParams> pathFindingParamsFromEntity;
        public ComponentDataFromEntity<PathCurrentNode> pathCurrentNodeFromEntity;
        public BufferFromEntity<PathBuffer> pathBufferFromEntity;

        public void Execute()
        {
            DynamicBuffer<PathBuffer> pathBuffer = pathBufferFromEntity[entity];
            pathBuffer.Clear();

            //Getting end node
            //TODO: if i want to request a path to a useable objext i will need an end point of the where i can use it from unless i cahnge this
            PathFindingParams pathfindingParams = pathFindingParamsFromEntity[entity];
            int endNodeIndex = CalculateIndex(pathfindingParams.EndPosition.x, pathfindingParams.EndPosition.y, gridSize.x);
            PathNode endNode = pathNodeArray[endNodeIndex];

            if(endNode.cameFromNodeIndex == -1)
            {
                //Didnt Find Path
                pathCurrentNodeFromEntity[entity] = new PathCurrentNode { currentPathIndex = -1 };
            }
            else
            {
                CalculatePath(pathNodeArray, endNode, pathBuffer);

                pathCurrentNodeFromEntity[entity] = new PathCurrentNode { currentPathIndex = pathBuffer.Length - 1 };
            }
        }

        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathBuffer> pathBuffer)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                //No path
            }
            else
            {
                //Path 
                DynamicBuffer<int2> path = pathBuffer.Reinterpret<int2>();
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }
            }
        }
    }

    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 startPos;
        public int2 endPos;
        
        public NativeArray<PathNode> pathNodeArray;
    
        public int2 gridSize;
        
        public void Execute()
        {
            //Set Up PathNode Array
            for (int i = 0; i < pathNodeArray.Length; i++)
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
            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            //loop time
            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    //We are there
                    break;
                }

                //Remove current node from list
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }


                closedList.Add(currentNodeIndex);
                NativeList<int2> neighbourOffsetList = GetNeighbourList(currentNode, gridSize, pathNodeArray);

                for (int i = 0; i < neighbourOffsetList.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetList[i];
                    int2 neighbourPos = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (IsPositionInsideGrid(neighbourPos, gridSize) == false)
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
                neighbourOffsetList.Dispose();
            }
            
            openList.Dispose();
            closedList.Dispose();
        }
        
        private bool IsPositionInsideGrid(int2 gridPos, int2 gridSize)
        {
            return gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < gridSize.x && gridPos.y < gridSize.y;
        }

        private NativeList<int2> GetNeighbourList(PathNode currentNode, int2 gridSize, NativeArray<PathNode> pathNodeArray, bool noCuttingCorners = true)
        {
            NativeList<int2> neighbourList = new NativeList<int2>(Allocator.Temp);

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

        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 0; i < openList.Length; i++)
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

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
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

        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }
        

    }
    
}
