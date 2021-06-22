using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class TaskActions 
{
    public static List<Action<NPC,Task, float>> GetActions(Task.Type type)
    {
        List<Action<NPC, Task, float>> actions = new List<Action<NPC, Task, float>>();

        switch (type)
        {
            case Task.Type.Collect:
                break;
            case Task.Type.Deliver:
                actions.Add(Find);
                actions.Add(Deliver);
                break;
            case Task.Type.Construct:
                actions.Add(GoTo);
                actions.Add(WorkOnJob);
                break;
            case Task.Type.Cut:
                actions.Add(GoTo);
                actions.Add(CutDown);
                break;
        }

        return actions;
    }

    public static void Find(NPC dude, Task task, float deltaTime)
    {
        int2 pos = WorldController.current.RoundPositionToInt(task.Location);
        int x = UnityEngine.Random.Range(pos.x - 5, pos.x + 5);
        int y = UnityEngine.Random.Range(pos.y - 5, pos.y + 5);
        
        PathJob pathJob = dude.SetPathJob(new int2(x,y), () => 
        {
            InventoryItem proto = WorldController.current.inventoryManager.ItemPrototypes[task.ItemName];
            InventoryItem item = InventoryItem.CreateItem(proto, 1);
            dude.inventory.AddItem(item);
            task.StepComplete();
            dude.OnArriveAction = null;
            dude.state = NPC.State.ExecuteTask;
        });

        WorldController.current.findPathJobsList.Add(pathJob);
        dude.state = NPC.State.WaitForPath;
    }

    public static void Deliver(NPC dude, Task task, float deltaTime)
    {
        int2 destination = WorldController.current.RoundPositionToInt(task.Location);

        PathJob pathJob = dude.SetPathJob(destination, () => 
        {
            Inventory stockPile = WorldController.current.jobManager.GetJob(WorldController.current.RoundPositionToInt(dude.Position())).inventory;
            int amount, slotID;
            if(dude.inventory.LookForItem(task.ItemName, out amount, out slotID) == false)
            {
                //We dont have the item
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
                return;
            }
            InventoryItem item = dude.inventory.AddItem(new InventoryItem(), slotID);
            stockPile.AddItem(item);
            task.OnComplete(dude);
            dude.OnArriveAction = null;
        });

        WorldController.current.findPathJobsList.Add(pathJob);
        dude.state = NPC.State.WaitForPath;
    }

    public static void GoTo(NPC dude, Task task, float deltaTime)
    {
        int2 destination = WorldController.current.RoundPositionToInt(task.Location);
        TileGridObj gridObj = WorldController.current.bigDaddyGrid.GetGridObject(destination.x, destination.y);

        if (gridObj.IsWalkable() == false)
        {
            bool foundTile = false;
            for (int x = destination.x - 1; x < destination.x + 1; x++)
            {
                if (foundTile == false)
                {
                    for (int y = destination.y - 1; y < destination.y + 1; y++)
                    {
                        TileGridObj checkObj = WorldController.current.bigDaddyGrid.GetGridObject(x, y);

                        if (checkObj.IsWalkable() == true)
                        {
                            destination = new int2(x, y);
                            foundTile = true;
                            break;
                        }
                    }
                }
            }
        }


        PathJob pathJob = dude.SetPathJob(destination, () =>
        {
            task.StepComplete();
            dude.OnArriveAction = null;
            dude.state = NPC.State.ExecuteTask;
        });

        WorldController.current.findPathJobsList.Add(pathJob);
        dude.state = NPC.State.WaitForPath;
    }

    public static void WorkOnJob(NPC dude, Task task, float deltaTime)
    {
        Job job = dude.job;

        float workDone = dude.characterSheet.WorkSpeed * deltaTime;

        job.WorkJob(workDone);

        if(job.WorkNeeded <= 0)
        {
            task.OnComplete(dude);
        }
    }

    public static void CutDown(NPC dude, Task task, float deltaTime)
    {
        //Look at task and play cut action

        WorldController.current.CutDown(task.Location);
        dude.state = NPC.State.Idle;
        task.OnComplete(dude);
    }
}
