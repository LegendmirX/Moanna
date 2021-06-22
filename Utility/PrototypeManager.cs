using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class PrototypeManager
{
    public static Dictionary<string, InstalledObject> BuildInstalledObjectPrototypes()
    {
        Dictionary<string, InstalledObject> installedObjectProtos = new Dictionary<string, InstalledObject>();

        installedObjectProtos.Add("Storage",
            InstalledObject.CreatePrototype(
                InstalledObject.ObjectType.Storage,
                "Storage",
                true,
                new List<TileGridObj.TileType> { TileGridObj.TileType.GRASS, TileGridObj.TileType.SAND },
                new int2(1, 1)
                )
            );
        installedObjectProtos.Add("StoreHouse",
            InstalledObject.CreatePrototype(
                InstalledObject.ObjectType.Storage,
                "StoreHouse",
                false,
                new List<TileGridObj.TileType> { TileGridObj.TileType.GRASS, TileGridObj.TileType.SAND },
                new int2(3, 4)
                )
            );
        installedObjectProtos.Add("House",
            InstalledObject.CreatePrototype(
                InstalledObject.ObjectType.InstalledObject,
                "House",
                false,
                new List<TileGridObj.TileType> { TileGridObj.TileType.GRASS, TileGridObj.TileType.SAND },
                new int2(2, 2)
                )
            );

        return installedObjectProtos;
    }

    public static Dictionary<Task.Type, Task> BuildTaskPrototypes()
    {
        Dictionary<Task.Type, Task> tasks = new Dictionary<Task.Type, Task>();

        tasks.Add(Task.Type.Collect,
            Task.CreatePrototype(
                Task.Type.Collect,
                TaskActions.GetActions(Task.Type.Collect)
                )
            );
        tasks.Add(Task.Type.Deliver,
            Task.CreatePrototype(
                Task.Type.Deliver,
                TaskActions.GetActions(Task.Type.Deliver)
                )
            );
        tasks.Add(Task.Type.Construct,
            Task.CreatePrototype(
                Task.Type.Construct,
                TaskActions.GetActions(Task.Type.Construct)
                )
            );
        tasks.Add(Task.Type.Cut,
            Task.CreatePrototype(
                Task.Type.Cut,
                TaskActions.GetActions(Task.Type.Cut)
                )
            );

        return tasks;
    }

    public static Dictionary<string, Job> BuildJobPrototypes()
    {
        Dictionary<string, Job> jobProtos = new Dictionary<string, Job>();

        jobProtos.Add("House",
            Job.CreatePrototype(
                Job.Priority.High,
                "House",
                new Dictionary<string, int> { ["Wood"] = 5 },
                10f,
                3
                )
            );
        jobProtos.Add("StoreHouse",
            Job.CreatePrototype(
                Job.Priority.High,
                "StoreHouse",
                new Dictionary<string, int> { ["Wood"] = 20 },
                40f,
                5
                )
            );
        jobProtos.Add("Storage",
            Job.CreatePrototype(
                Job.Priority.High,
                "Storage",
                new Dictionary<string, int>(),
                2f,
                1
                )
            );

        return jobProtos;
    }

    public static Dictionary<string, InventoryItem> BuildInventoryItemPrototypes()
    {
        Dictionary<string, InventoryItem> protos = new Dictionary<string, InventoryItem>();

        protos.Add("Wood",
            InventoryItem.CreatePrototype(
                InventoryItem.InventoryType.MATERIAL,
                "Wood",
                1,
                true,
                2
                )
            );

        return protos;
    }

    public static Dictionary<string, Plant> BuildPlantPrototypes()
    {
        Dictionary<string, Plant> protos = new Dictionary<string, Plant>();

        protos.Add("PalmTree",
            Plant.CreatePrototype(
                "PalmTree",
                "CocoNuts",
                "Wood",
                new List<TileGridObj.TileType> { TileGridObj.TileType.GRASS, TileGridObj.TileType.SAND },
                false,
                4,
                true,
                4,
                1
                )
            );

        return protos;
    }

    //private static Dictionary<string, Relationship.RelationshipStatus> ChatLines(string name)
    //{
    //    Dictionary<string, Relationship.RelationshipStatus> chatLines = new Dictionary<string, Relationship.RelationshipStatus>();

    //    switch (name)
    //    {
    //        case "Kevin":
    //            chatLines.Add("oh its you.",
    //                Relationship.RelationshipStatus.Adversary);
    //            chatLines.Add("Hello there",
    //                Relationship.RelationshipStatus.Acquaintance);
    //            chatLines.Add("Its nice to see you again",
    //                Relationship.RelationshipStatus.Friend);
    //            chatLines.Add("Whats up. How gose the farming",
    //                Relationship.RelationshipStatus.CloseFriend);
    //            chatLines.Add("We should go fishing some time",
    //                Relationship.RelationshipStatus.LoveInterest);
    //            chatLines.Add("Hope your day gose well <3",
    //                Relationship.RelationshipStatus.Lover);
    //            break;
    //        case "Jessica":
    //            chatLines.Add("oh its you.",
    //                Relationship.RelationshipStatus.Adversary);
    //            chatLines.Add("Hello there",
    //                Relationship.RelationshipStatus.Acquaintance);
    //            chatLines.Add("Its nice to see you again",
    //                Relationship.RelationshipStatus.Friend);
    //            chatLines.Add("Whats up. How gose the farming",
    //                Relationship.RelationshipStatus.CloseFriend);
    //            chatLines.Add("We should go fishing some time",
    //                Relationship.RelationshipStatus.LoveInterest);
    //            chatLines.Add("Hope your day gose well <3",
    //                Relationship.RelationshipStatus.Lover);
    //            break;
    //    }

    //    return chatLines;
    //}

    //public static List<string> ShopItems(Shop.Type type)
    //{
    //    List<string> items = new List<string>();

    //    switch (type)
    //    {
    //        case Shop.Type.General:
    //            items.Add("CornSeeds");
    //            break;
    //    }

    //    return items;
    //}

    //private static List<Action<InstalledObject,float>> getInstalledObjectUpdateActions(string objType)
    //{
    //    List<Action<InstalledObject, float>> udpateActions = new List<Action<InstalledObject, float>>();

    //    switch (objType)
    //    {
    //        case "Door":
    //            udpateActions.Add(InstalledObjectActions.DoorUpdateAction);
    //            break;
    //    }

    //    return udpateActions;
    //}

    //private static Dictionary<string, object> getInstalledObjectParamaters(string objType)
    //{
    //    Dictionary<string, object> objParams = new Dictionary<string, object>();

    //    switch (objType)
    //    {
    //        case "Door":
    //            objParams.Add("openAmount", 0f);
    //            objParams.Add("isDoorOpening", false);
    //            objParams.Add("hasPassedOver", false);
    //            objParams.Add("doorOpenTime", 0.5f);
    //            break;
    //        case "Shop":
    //            objParams.Add("ShopScript", new Shop(Shop.Type.General, ShopItems(Shop.Type.General)) );
    //            break;
    //    }

    //    if(objParams.Count > 0)
    //    {
    //        return objParams;
    //    }

    //    return null;
    //}

    //private static Dictionary<string, Func<InstalledObject, object>> getInstalledObjectFunctions(string objType)
    //{
    //    Dictionary<string, Func<InstalledObject, object>> funcs = new Dictionary<string, Func<InstalledObject, object>>();

    //    switch (objType)
    //    {
    //        case "Door":
    //            funcs.Add("IsEnterable", IsEnterable);
    //            break;
    //    }

    //    return funcs;
    //}

    //#region InstalledObjectFuncs
    //private static object IsEnterable(InstalledObject obj)
    //{
    //    float openAmount = (float)obj.Paramaters["openAmount"];
    //    if (openAmount == 1)
    //    {
    //        return true;
    //    }

    //    return false;
    //}
    //#endregion
}
