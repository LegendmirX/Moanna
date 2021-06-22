using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class InstalledObject
{
    public enum ObjectType
    {
        InstalledObject,
        Storage,
        Door,
        Shop
    }
    public ObjectType Type;

    public string Name { get; protected set; }
    public Vector3 Position { get; protected set; }
    public bool IsWalkable { get; protected set; }
    public List<TileGridObj.TileType> AcceptableTiles { get; protected set; }
    public int2 Size { get; protected set; }
    public GameObject GO { get; protected set; }

    private Action<InstalledObject, float> updateAction;
    public Dictionary<string, Func<InstalledObject, object>> Functions;
    public Dictionary<string, object> Paramaters;

    //public bool SatisfiesNeed { get; protected set; }
    //public Needs.Need NeedSatisfies { get; protected set; }
    //private Action<Player, NPC> interact;

    #region BuildFuncs
    public InstalledObject()
    {

    }

    protected InstalledObject(InstalledObject other)
    {
        this.Type               = other.Type;
        this.Name               = other.Name;
        this.IsWalkable         = other.IsWalkable;
        this.AcceptableTiles    = other.AcceptableTiles;
        this.Size               = other.Size;

        if(other.Paramaters != null)
        {
            this.Paramaters = new Dictionary<string, object>(other.Paramaters);
        }
        if(other.Functions != null)
        {
            this.Functions = new Dictionary<string, Func<InstalledObject, object>>(other.Functions);
        }
        if(other.updateAction != null)
        {
            this.updateAction = (Action<InstalledObject, float>)other.updateAction.Clone();
        }
    }

    virtual public InstalledObject Clone()
    {
        return new InstalledObject(this);
    }

    static public InstalledObject CreatePrototype(ObjectType type, string subType, bool isWalkable, List<TileGridObj.TileType> acceptableTiles, int2 Size, List<Action<InstalledObject, float>> updateActions = null, Dictionary<string, object> objParams = null, Dictionary<string, Func<InstalledObject, object>> objFuncs = null)
    {
        InstalledObject installedObject = new InstalledObject();
        installedObject.Paramaters      = new Dictionary<string, object>();
        installedObject.Type            = type;
        installedObject.Name            = subType;
        installedObject.IsWalkable      = isWalkable;
        installedObject.AcceptableTiles = acceptableTiles;
        installedObject.Size            = Size;
        if(updateActions != null)
        {
            foreach(Action<InstalledObject,float> func in updateActions)
            {
                installedObject.updateAction += func;
            }
        }

        installedObject.Paramaters = objParams;
        installedObject.Functions = objFuncs;
        
        return installedObject;
    }
    #endregion

    static public InstalledObject CreateInstalledObject(InstalledObject proto, Vector3 position)
    {
        InstalledObject installedObject = proto.Clone();
        installedObject.Position = position;

        return installedObject;
    }

    public void Update(float deltaTime)
    {

        if(updateAction != null)
        {
            updateAction(this, deltaTime);
        }
    }

    public void SetGO(GameObject GO)
    {
        this.GO = GO;
    }
    

    #region CallBacks
    public void RegisterUpdateAction(Action<InstalledObject, float> func)
    {
        updateAction += func;
    }
    public void UnregisterUpdateAction(Action<InstalledObject, float> func)
    {
        updateAction -= func;
    }
    #endregion
}
