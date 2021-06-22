using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[Serializable]
public class NPC : IPathfindableInterface
{
    public enum State
    {
        Idle,
        FollowPath,
        ExecuteTask,
        WaitForPath
    }
    public State state;

    public GameObject gameObject;
    public CharacterSheet characterSheet;
    public Inventory inventory;

    [NonSerialized]
    public Job job;
    [NonSerialized]
    public Task task;
    public Action OnArriveAction;
    float checkTaskTimer;
    float checkTaskTime = 1f;

    public bool PathRecived = false;
    float waitForPathTimer;
    float waitForPathTime = 3f;
    public PathData pathData;
    public Vector3 CurrentNode;

    public NPC(CharacterSheet characterSheet)
    {
        this.characterSheet = characterSheet;
        this.inventory = new Inventory(1, Inventory.Type.Personal, characterSheet.Name + " Inventory");
        this.state = State.Idle;
        this.task = null;
    }

    public Vector3 Position()
    {
        return gameObject.transform.position;
    }

    public void FollowPath(float deltaTime)
    {
        Vector3 startPos = gameObject.transform.position;

        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, CurrentNode, deltaTime * characterSheet.Speed);

        if (Vector3.Distance(gameObject.transform.position, CurrentNode) <= 0.1)
        {
            int2 p = RoundPositionToInt(CurrentNode);

            pathData.Path.Remove(p);

            if (pathData.Path.Count == 0)
            {
                pathData.Path = null;
                PathRecived = false;
                if (OnArriveAction != null)
                {
                    OnArriveAction.Invoke();
                }
            }
            else
            {
                int2 pos = pathData.Path[pathData.Path.Count - 1];
                CurrentNode = new Vector3(pos.x, pos.y);
            }
        }
    }

    public void OnPathReceived(object path)
    {
        PathRecived = true;
        PathData data = (PathData)path;
        this.pathData = data;

        if(this.pathData.Path == null)
        {
            return;
        }

        if (this.pathData.Path.Count > 1)
        {
            this.pathData.Path.Remove(this.pathData.Path[this.pathData.Path.Count - 1]);
        }

        int2 pos = this.pathData.Path[this.pathData.Path.Count - 1];
        CurrentNode = new Vector3(pos.x, pos.y);
        this.pathData.Path.Remove(pos);
        state = State.FollowPath;
    }

    public PathJob SetPathJob(Vector3 destination, Action onArrive = null)
    {
        int2 endPos = new int2(Mathf.RoundToInt(destination.x), Mathf.RoundToInt(destination.y));

        return SetPathJob(endPos, onArrive);
    }
    public PathJob SetPathJob(int2 destination, Action onArrive = null)
    {
        int2 startPos = new int2(Mathf.RoundToInt(Position().x), Mathf.RoundToInt(Position().y));

        if (onArrive == null)
        {
            OnArriveAction = () => { this.state = State.Idle; };
        }
        else
        {
            OnArriveAction = onArrive;
        }

        return new PathJob(OnPathReceived, startPos, destination);
    }

    public bool CheckTask(float deltaTime)
    {
        checkTaskTimer += deltaTime;
        if (checkTaskTimer >= checkTaskTime)
        {
            checkTaskTimer = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool WaitForPathTimer(float deltaTime)
    {
        //So apparently my NPCs are getting stuck in wait for path mode. Cant find out why they arnt geting a path.
        waitForPathTimer += deltaTime;
        if(waitForPathTimer >= waitForPathTime)
        {
            waitForPathTimer = 0f;
            return true;
        }
        return false;
    }

    public void RequestTaskFromJob(float deltaTime)
    {
        if(CheckTask(deltaTime) == true)
        {
            task = job.RequestTask();
        }
    }

    public void RemoveJob()
    {
        job = null;
        task = null;
        state = State.Idle;
    }

    public int2 RoundPositionToInt(Vector3 position)
    {
        return new int2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
