using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstalledObjectActions
{
//    public static Action<Player, NPC> GetInteractAction(string actionName)
//    {
//        switch (actionName)
//        {
//            case "TestAction":
//                return TestAction;
//            case "FridgeInteractAction":
//                return FridgeInteractAction;
//            case "ToiletInteratAction":
//                return ToiletInteratAction;
//            case "BedInteractAction":
//                return BedInteractAction;
//            case "SinkInteractAction":
//                return SinkInteractAction;
//        }

//        return null;
//    }

//    private static void TestAction(Player player, NPC npc)
//    {
//        Debug.Log("TestAction called");
//    }

//    private static void FridgeInteractAction(Player player = null, NPC npc = null)
//    {
//        int value = 50;
//        Needs.Need need = Needs.Need.Hunger;

//        if(player != null)
//        {
//            //TODO Impliment player needs
//            return;
//        }

//        if(npc != null)
//        {
//            npc.needs.ChangeNeed(need, value);
//            return;
//        }
//    }

//    private static void ToiletInteratAction(Player player = null, NPC npc = null)
//    {
//        int value = 50;
//        Needs.Need need = Needs.Need.Toilet;

//        if (player != null)
//        {
//            //TODO Impliment player needs
//            return;
//        }

//        if (npc != null)
//        {
//            npc.needs.ChangeNeed(need, value);
//            return;
//        }
//    }

//    private static void BedInteractAction(Player player = null, NPC npc = null)
//    {
//        int value = 50;
//        Needs.Need need = Needs.Need.Sleep;

//        if (player != null)
//        {
//            //TODO Impliment player needs
//            return;
//        }

//        if (npc != null)
//        {
//            npc.needs.ChangeNeed(need, value);
//            return;
//        }
//    }

//    private static void SinkInteractAction(Player player = null, NPC npc = null)
//    {
//        int value = 50;
//        Needs.Need need = Needs.Need.Thirst;

//        if (player != null)
//        {
//            //TODO Impliment player needs
//            return;
//        }

//        if (npc != null)
//        {
//            npc.needs.ChangeNeed(need, value);
//            return;
//        }
//    }

//    public static void DoorUpdateAction(InstalledObject obj, float deltaTime) //Public for now will see what future holds
//    {
//        //Debug.Log("DoorUpdateAction");
//        bool isOpening      = (bool)obj.Paramaters["isDoorOpening"];
//        bool hasPassedOver  = (bool)obj.Paramaters["hasPassedOver"];
//        float openness      = (float)obj.Paramaters["openAmount"];
//        float openTime      = (float)obj.Paramaters["doorOpenTime"];

//        if(isOpening == true)
//        {
//            if(openness >= 1)
//            {
//                if(hasPassedOver == true)
//                {
//                    obj.Paramaters["isDoorOpening"] = false;
//                }
//                return;
//            }
//            openness += (deltaTime / openTime);
//            Mathf.Clamp01(openness);
//            obj.Paramaters["openAmount"] = openness;
//        }
//        else
//        {
//            if(openness <= 0)
//            {
//                obj.Paramaters["hasPassedOver"] = false;
//                obj.UnregisterUpdateAction(DoorUpdateAction);
//            }
//            openness -= (deltaTime / openTime);
//            Mathf.Clamp01(openness);
//            obj.Paramaters["openAmount"] = openness;
//        }
//        //Debug.Log("Openness = " + openness);
//    }

}
