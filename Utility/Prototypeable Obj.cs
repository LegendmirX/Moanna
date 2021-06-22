using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeableObj : MonoBehaviour
{


    #region BuildFuncs
    public PrototypeableObj()
    {

    }

    protected PrototypeableObj(PrototypeableObj other)
    {
        //Make sure all data from other is copied over to this.
    }

    virtual public PrototypeableObj Clone()
    {
        return new PrototypeableObj(this);
    }

    static public PrototypeableObj CreatePrototype(/*Put in build data here to make a prototype*/)
    {
        PrototypeableObj prototypeableObj = new PrototypeableObj();

        //set Data for prototype

        return prototypeableObj;
    }
    #endregion

    static public PrototypeableObj CreateInstalledObject(PrototypeableObj proto)
    {
        PrototypeableObj prototypeableObj = proto.Clone();

        return prototypeableObj;
    }
}
