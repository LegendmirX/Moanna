using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[Serializable]
public class Job
{
    public enum Priority
    {
        High,
        Low
    }
    public Priority priority;
    public string Name { get; protected set; }
    public Dictionary<string, int> ItemsNeeded { get; protected set; }
    public float WorkNeeded { get; protected set; }
    public Vector3 Position { get; protected set; }
    public GameObject GO;
    public Vector3 StockpilePosition { get; protected set; }
    public Inventory inventory { get; protected set; }

    public List<NPC> Workers;
    public int MaxWorkers { get; protected set; }

    [SerializeField]
    public List<Task> tasks;

    #region BuildFuncs
    public Job()
    {

    }

    protected Job(Job other)
    {
        this.priority = other.priority;
        this.Name = other.Name;
        this.ItemsNeeded = new Dictionary<string, int>(other.ItemsNeeded);
        this.WorkNeeded = other.WorkNeeded;
        this.MaxWorkers = other.MaxWorkers;
    }

    virtual public Job Clone()
    {
        return new Job(this);
    }

    static public Job CreatePrototype(Priority priority, string name, Dictionary<string, int> itemsNeeded, float workNeeded, int maxWorkers)
    {
        Job prototypeableObj = new Job();

        prototypeableObj.priority = priority;
        prototypeableObj.Name = name;
        prototypeableObj.ItemsNeeded = itemsNeeded;
        prototypeableObj.WorkNeeded = workNeeded;
        prototypeableObj.MaxWorkers = maxWorkers;

        return prototypeableObj;
    }
    #endregion

    public static Job CreateJob(Job proto, int2 position)
    {
        Job job = proto.Clone();

        int size = 0;

        foreach(string item in job.ItemsNeeded.Keys)
        {
            size += job.ItemsNeeded[item];
        }

        job.Position = new Vector3(position.x, position.y);
        job.StockpilePosition = new Vector3(position.x, position.y - 1);
        job.inventory = new Inventory(size, Inventory.Type.Public, "JobInventory");
        job.tasks = new List<Task>();
        job.Workers = new List<NPC>();

        if (job.ItemsNeeded.Count > 0)
        {
            foreach (string itemType in job.ItemsNeeded.Keys)
            {
                for (int i = 0; i < job.ItemsNeeded[itemType]; i++)
                {
                    Task task = WorldController.current.jobManager.CreateTask(Task.Type.Deliver, job.StockpilePosition, itemType, 1, job.OnTaskComplete);
                    job.tasks.Add(task);
                }
            }
        }
        else
        {
            job.tasks.Add(WorldController.current.jobManager.CreateTask(Task.Type.Construct, job.StockpilePosition, null, 0, job.OnTaskComplete));
        }

        return job;
    }

    public void SetWorkers()
    {
        MaxWorkers = tasks.Count;
    }

    public Task RequestTask()
    {
        if(tasks != null && tasks.Count > 0)
        {
            Task t = tasks[0];
            tasks.Remove(t);
            return t;
        }

        return null;
    }

    public void ReQueueTask(Task task)
    {
        tasks.Add(task);
    }

    public void WorkJob(float workDone)
    {
        WorkNeeded -= workDone;
    }

    public void OnTaskComplete(Task task, NPC dude)
    {
        dude.task = null;
        dude.state = NPC.State.ExecuteTask;

        List<string> itemsToRemove = new List<string>();
        
        foreach(string item in ItemsNeeded.Keys)
        {
            int quantityFound;
            int slotID;
            inventory.LookForItem(item, out quantityFound, out slotID, ItemsNeeded[item]);

            if(ItemsNeeded[item] <= quantityFound)
            {
                itemsToRemove.Add(item);
            }
        }

        if(itemsToRemove.Count > 0)
        {
            foreach(string item in itemsToRemove)
            {
                Debug.Log("Removing " + item + " from needed list");

                foreach(NPC w in Workers)
                {
                    if(w.task != null && w.task.ItemName == item)
                    {
                        w.task.OnComplete(w);
                    }
                }

                ItemsNeeded.Remove(item);
            }
        }

        if(ItemsNeeded.Count <= 0 && WorkNeeded > 0)
        {
            for (int i = 0; i < MaxWorkers; i++)
            {
                Task constructionTask = WorldController.current.jobManager.CreateTask(Task.Type.Construct, StockpilePosition, null, 0, OnTaskComplete);
                tasks.Add(constructionTask);
            }
        }

        if(WorkNeeded <= 0)
        {
            Debug.Log("Construction Complete: " + Name);
            foreach(NPC worker in Workers)
            {
                worker.job = null;
                worker.task = null;
                worker.state = NPC.State.Idle;
            }
            Workers.Clear();
            WorldController.current.CreateInstalledObject(Name, new Vector2(Position.x, Position.y), this);
        }
    }
}
