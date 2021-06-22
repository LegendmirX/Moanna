using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class InventoryItemActions
{
    public static Action<float, InventoryItem> GetAction(string name) //When adding new ItemAction dont forget to add it to the switch
    {
        Action<float, InventoryItem> action = null;

        //switch (name)//Gets actions from here
        //{
        //    case "Hoe_OnUse":
        //        action = Hoe_OnUse;
        //        break;
        //    case "Seeds_OnUse":
        //        action = Seeds_OnUse;
        //        break;
        //    case "WateringCan_OnUse":
        //        action = WateringCan_OnUse;
        //        break;
        //}

        if (action != null)
        {
            return action;
        }
        else
        {
            Debug.LogError("GetAction: Has no action for - " + name);
            return null;
        }
    }
}
