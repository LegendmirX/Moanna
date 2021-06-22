using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class JobManager : MonoBehaviour 
{
    Dictionary<string, Job> jobPrototypes;
    Dictionary<Task.Type, Task> taskPrototypes;
    
    [SerializeField]
    List<Job> JobQueue;

    [SerializeField]
    List<Task> TaskQueue;

    Dictionary<int2, Job> jobByLocation;

    public void SetUp()
    {
        taskPrototypes = PrototypeManager.BuildTaskPrototypes();
        jobPrototypes = PrototypeManager.BuildJobPrototypes();
        TaskQueue = new List<Task>();
        jobByLocation = new Dictionary<int2, Job>();
        JobQueue = new List<Job>();
    }

    public Job CreateJob(string jobName, int2 position)
    {
        Debug.Log("CreatingJob: " + jobName);
        if (jobByLocation.ContainsKey(position) == true)
        {
            Debug.Log("We really need to validate job placement");
            return null;
        }
        if(jobPrototypes.ContainsKey(jobName) == false)
        {
            Debug.Log(jobName + " Not fount");
            return null;
        }
        Job job = Job.CreateJob(jobPrototypes[jobName], position);
        
        InstalledObject proto = WorldController.current.installedObjectManager.InstalledObjectPrototypes[jobName];
        GameObject go = WorldController.current.installedObjectManager.installedObjectVisuals.CreateMouseGhost(jobName, proto, new Vector3(position.x, position.y), this.transform);

        job.GO = go;

        JobQueue.Add(job);
        jobByLocation.Add(position, job);
       
        return job;
    }

    public Task CreateTask(Task.Type type, Vector3 position, string itemName = null, int quantity = 0, Action<Task,NPC> callback = null)
    {
        Debug.Log("CreatingTask: " + type.ToString());
        Task proto = taskPrototypes[type];
        Task task = Task.CreateTask(proto, position, itemName, quantity, callback);

        return task;
    }

    public Task RequestTask(int2 position)
    {
        if (TaskQueue.Count <= 0)
        {
            return null;
        }

        Task task = TaskQueue[0];
        TaskQueue.Remove(task);

        return task;
    }

    public void ReQueueTask(Task task)
    {
        TaskQueue.Add(task);
    }

    public Job RequestJob(int2 position)
    {
        if (JobQueue.Count <= 0)
        {
            return null;
        }

        Job job = null;

        foreach(Job j in JobQueue)
        {
            if (j.Workers.Count < j.MaxWorkers)
            {
                job = j;
                break;
            }
        }

        return job;
    }

    public void RemoveTask(Task task)
    {
        TaskQueue.Remove(task);
    }

    public void RemoveJob(Job job)
    {

        if(job.Workers != null)
        {
            foreach(NPC worker in job.Workers)
            {
                worker.job = null;
                worker.state = NPC.State.Idle;
            }
        }
        
        jobByLocation.Remove(WorldController.current.RoundPositionToInt(job.Position));
        GameObject go = job.GO;
        JobQueue.Remove(job);
        Destroy(go);
    }
    
    public Job GetJob(int2 position)
    {
        int2 pos = new int2(position.x, position.y + 1);
        if(jobByLocation.ContainsKey(pos) == false)
        {
            Debug.Log("Job Not Found");
            return null;
        }
        Job job = jobByLocation[pos];
        return job;
    }
}
