using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : IPathfindableInterface 
{
    public GameObject gameObject;
    public CharacterSheet characterSheet;
    public PathData pathData;
    public Vector3 CurrentNode;

    public Player(GameObject gameObject, CharacterSheet characterSheet)
    {
        this.gameObject = gameObject;
        this.characterSheet = characterSheet;
    }

    public void FollowPath(float deltaTime)
    {
        if(pathData.Path != null)
        {
            Vector3 startPos = gameObject.transform.position;
            
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, CurrentNode, deltaTime * characterSheet.Speed);
            
            if (Vector3.Distance(gameObject.transform.position, CurrentNode) <= 0.1)
            {
                if(pathData.Path.Count == 0)
                {
                    pathData.Path = null;
                }
                else
                {
                    int2 pos = pathData.Path[pathData.Path.Count - 1];
                    CurrentNode = new Vector3(pos.x, pos.y);
                    pathData.Path.Remove(pos);
                }
            }
        }
    }

    public Vector3 Position()
    {
        return gameObject.transform.position;
    }

    public PathJob SetPathJob(Vector3 destination, Action onArriveAction = null)
    {
        int2 startPos = new int2(Mathf.RoundToInt(Position().x), Mathf.RoundToInt(Position().y));
        int2 endPos = new int2(Mathf.RoundToInt(destination.x), Mathf.RoundToInt(destination.y));

        return new PathJob(OnPathReceived, startPos, endPos);
    }

    public void OnPathReceived(object pathData)
    {
        PathData data = (PathData)pathData;
        this.pathData = data;
        if(this.pathData.Path == null)
        {
            return;
        }
        this.pathData.Path.Remove(this.pathData.Path[this.pathData.Path.Count - 1]);
        int2 pos = this.pathData.Path[this.pathData.Path.Count - 1];
        CurrentNode = new Vector3(pos.x, pos.y);
        this.pathData.Path.Remove(pos);
    }
}
