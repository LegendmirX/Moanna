using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class NPCManager : MonoBehaviour 
{
    NPCVisuals dudeVisuals;
    [SerializeField]
    List<NPC> dudeList;

    public void SetUp(EntityManager entityManager)
    {
        dudeVisuals = FindObjectOfType<NPCVisuals>();
        dudeVisuals.SetUp(entityManager);
        dudeList = new List<NPC>();
    }

    public void FrameUpdate(float deltaTime)
    {
        foreach(NPC dude in dudeList)
        {
            switch (dude.state)
            {
                case NPC.State.Idle:
                    #region Look for job or task
                    if (dude.CheckTask(deltaTime) == true)
                    {
                        int2 pos = WorldController.current.RoundPositionToInt(dude.Position());

                        Job job = WorldController.current.jobManager.RequestJob(pos);
                        if(job != null)
                        {
                            Debug.Log("JobFound: " + job.Name);
                            dude.job = job;
                            job.Workers.Add(dude);
                            dude.state = NPC.State.ExecuteTask;
                            return;
                        }

                        Task task = WorldController.current.jobManager.RequestTask(pos);
                        if (task != null)
                        {
                            Debug.Log("TaskFound: " + task.type.ToString());
                            dude.task = task;
                            dude.state = NPC.State.ExecuteTask;
                            return;
                        }
                    }
                    #endregion
                    break;
                case NPC.State.FollowPath:
                    if (dude.pathData.Path != null)
                    {
                        dude.FollowPath(deltaTime);
                    }
                    break;
                case NPC.State.ExecuteTask:
                    if(dude.task != null)
                    {
                        dude.task.Execute(dude, deltaTime);
                    }
                    else if(dude.job != null)
                    {
                        dude.RequestTaskFromJob(deltaTime);
                    }
                    break;
                case NPC.State.WaitForPath:
                    #region WhatIfThePathIsNull/PathNeverComes?
                    if ((dude.PathRecived == true && dude.pathData.Path == null) || dude.WaitForPathTimer(deltaTime) == true)
                    {
                        dude.PathRecived = false;
                        ReQueueTask(dude);
                    }
                    #endregion
                    break;
            }
        }
    }

    void ReQueueTask(NPC dude)
    {
        if (dude.task != null && dude.job != null)
        {
            dude.task.FailedToComplete();
            dude.job.ReQueueTask(dude.task);
            dude.task = null;
            dude.state = NPC.State.ExecuteTask;
        }
        else if (dude.task != null && dude.job == null)
        {
            dude.task.FailedToComplete();
            WorldController.current.jobManager.ReQueueTask(dude.task);
            dude.task = null;
            dude.state = NPC.State.Idle;
        }
        else
        {
            dude.state = NPC.State.Idle;
        }
    }

    public NPC CreateNPC(string name, int2 position)
    {
        CharacterSheet characterSheet = ScriptableObject.CreateInstance<CharacterSheet>();
        characterSheet.Name = name;
        characterSheet.Speed = 2;
        characterSheet.WorkSpeed = 2;

        NPC dude = new NPC(characterSheet);

        GameObject go = dudeVisuals.BuildVisuals(dude, position);
        dude.gameObject = go;

        dudeList.Add(dude);
        return dude;
    }
}
