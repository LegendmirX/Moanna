using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Task : ITaskInterface
{
    public enum Type
    {
        Deliver,
        Collect,
        Construct,
        Cut
    }
    public Type type;

    List<Action<NPC,Task,float>> actions;
    int stepIndex;
    public Vector3 Location;


    Action<Task, NPC> Callback;
    public string ItemName { get; protected set; }
    public int Quantity { get; protected set; }

    #region BuildFuncs
    public Task()
    {

    }

    protected Task(Task other)
    {
        this.type = other.type;
        this.actions = new List<Action<NPC, Task, float>>(other.actions);
    }

    virtual public Task Clone()
    {
        return new Task(this);
    }

    static public Task CreatePrototype(Type type, List<Action<NPC,Task,float>> actions)
    {
        Task prototypeableObj = new Task();

        prototypeableObj.type = type;
        prototypeableObj.actions = actions;

        return prototypeableObj;
    }
    #endregion

    public static Task CreateTask(Task proto, Vector3 location, string itemName = null, int quantity = 0, Action<Task, NPC> callback = null)
    {
        Task task = proto.Clone();

        task.Location = location;
        if (itemName != null)
        {
            task.ItemName = itemName;
        }
        if(quantity > 0)
        {
            task.Quantity = quantity;
        }
        if(callback != null)
        {
            task.Callback = callback;
        }
        task.stepIndex = 0;

        return task;
    }

    public void Execute(NPC dude, float deltaTime)
    {
        //Debug.Log("Execute - " + actions[stepIndex].Method.Name);
        if(actions[stepIndex] != null)
        {
            actions[stepIndex].Invoke(dude, this, deltaTime);
        }
        else
        {
            Debug.Log("No Action? Step: " + stepIndex);
        }
    }

    public void OnComplete(NPC dude)
    {
        if(Callback != null)
        {
            Callback(this, dude);
        }
        else
        {
            dude.task = null;
            WorldController.current.jobManager.RemoveTask(this);
        }

    }

    public void StepComplete()
    {
        stepIndex++;
        Mathf.Clamp(stepIndex, 0, actions.Count - 1);
    }

    public void FailedToComplete()
    {
        stepIndex = 0;
    }
}
