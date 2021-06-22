using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class InventoryVisualsController : MonoBehaviour
{
    InventoryManager invManager;

    GameObject inventoryVis;
    Text title;
    GameObject InvField;

    GameObject tradeVis;
    List<GameObject> tradeFields;

    GameObject slotPrefab;
    
    Dictionary<int, GameObject> slots;
    Dictionary<string, GameObject> tradeSlots;
    public Dictionary<InventoryItem, GameObject> InventoryItemGOMap;
    Dictionary<Inventory, GameObject> tempInvMap;
    int maxInvs;

    private void OnEnable()
    {

    }

    void Start()
    {

    }

    public void SetUp(int MaxInvs, InventoryManager invManager)
    {
        this.invManager = invManager;
        this.maxInvs = MaxInvs;
        tempInvMap = new Dictionary<Inventory, GameObject>();
    }

    public void OnLoad()
    {
        
    }

    public void OnTempInv(Inventory inv, Transform parent)
    {
        GameObject go = Instantiate(GameAssets.i.TilePrefab);
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Color c = Color.red;
        c.a = 0.5f;
        
        sr.color = c;
        sr.sortingLayerName = "InstalledObjects";

        go.transform.position = parent.position;
        go.transform.SetParent(parent, true);

        tempInvMap.Add(inv, go);
    }

    public void RemoveTempInv(Inventory inv)
    {
        GameObject go = tempInvMap[inv];
        tempInvMap.Remove(inv);
        Destroy(go);
    }

    public void OnInventoryChanged(Inventory inv_Data) 
    {
        if(inventoryVis.activeSelf == true)
        {
            for (int i = 0; i < inv_Data.Capacity; i++)
            {
                Image img = slots[i].transform.Find("Img").GetComponent<Image>();
                GameObject counter = slots[i].transform.Find("Img").Find("Counter").gameObject;
                InventoryItem item = inv_Data.Items[i];
            }
        }
        else if(tradeVis.activeSelf == true)
        {
            int field = invManager.TradeInventories.IndexOf(inv_Data);
            for (int i = 0; i < inv_Data.Capacity; i++)
            {
                Image img = tradeSlots[GetTradeSlotString(field, i)].transform.Find("Image").GetComponent<Image>();
                GameObject counter = tradeSlots[GetTradeSlotString(field, i)].transform.Find("Image").Find("Counter").gameObject;
                InventoryItem item = inv_Data.Items[i];
            }
        }
    }

    string GetTradeSlotString(int tradeField, int slot)
    {
        string s = tradeField + "-" + slot;
        return s;
    }

    public void OpenInventory(Inventory inv_Data)
    {
        inventoryVis.SetActive(true);
        tradeVis.SetActive(false);
        //title.text = inv_Data.Name;

        for (int i = 0; i < slots.Count; i++)
        {
            if(i < inv_Data.Capacity)
            {
                slots[i].SetActive(true);
            }
            else
            {
                slots[i].SetActive(false);
            }
        }
        OnInventoryChanged(inv_Data);
    }

    public void OpenTradeWindow(Inventory invA, Inventory invB)
    {
        tradeVis.SetActive(true);
        inventoryVis.SetActive(false);

        for (int i = 0; i < invManager.TradeInventories.Count; i++)
        {
            Inventory inv = invManager.TradeInventories[i];
            for (int s = 0; s < invManager.MaxInvs; s++) 
            {
                if(s < inv.Capacity)
                {
                    tradeSlots[GetTradeSlotString(i,s)].SetActive(true);
                }
                else
                {
                    tradeSlots[GetTradeSlotString(i,s)].SetActive(false);
                }
            }
            OnInventoryChanged(inv);
        }

    }

    public void CloseInventory(bool isTrade)
    {
        if(isTrade == true)
        {
            tradeVis.SetActive(false);
        }
        else
        {
            inventoryVis.SetActive(false);
        }
    }
    
    void OnInventoryRemoved(InventoryItem inv)
    {
        GameObject obj = InventoryItemGOMap[inv];

        InventoryItemGOMap.Remove(inv);
        Destroy(obj);

        //inv.UnregisterOnChangedCallback(OnInventoryChanged);
        inv.UnegisterOnRemovedCallback(OnInventoryRemoved);
    }
}
