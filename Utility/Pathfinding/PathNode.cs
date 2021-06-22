using Unity.Entities;

public struct PathNode : IComponentData
{
    public int x;
    public int y;

    public int index;

    public int gCost;
    public int hCost;
    public int fCost;

    private bool isWalkable;

    public int cameFromNodeIndex;

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
}
