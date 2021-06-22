using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using UnityEngine;

public class Inventory : IXmlSerializable
{
    public enum Type
    {
        Personal,
        Temp,
        Public
    }
    public Type type;

    public string Name { get; protected set; }
    public int Capacity { get; protected set; }

    public bool CanBeTakenFrom { get; protected set; }
    public bool CanBeAddedTo { get; protected set; }
    
    public InventoryItem[] Items;

    public Inventory()
    {
        Items = new InventoryItem[0];
    }

    public Inventory(int capacity, Type type, string name = "NoName", InventoryItem[] items = null)
    {
        Items = new InventoryItem[capacity];
        this.Capacity = capacity;
        Name = name;
        this.type = type;
        switch (type)
        {
            case Type.Personal:
                CanBeTakenFrom = false;
                CanBeAddedTo = true;
                break;
            case Type.Public:
                CanBeTakenFrom = true;
                CanBeAddedTo = true;
                break;
            case Type.Temp:
                CanBeTakenFrom = true;
                CanBeAddedTo = false;
                break;
        }

        if(items == null)
        {
            for (int i = 0; i < capacity; i++)
            {
                Items[i] = new InventoryItem(); 
            }
        }
        else
        {
            Items = items;
        }

    }

    public void ChangeCapacity(int amount)
    {
        Capacity += amount;
        InventoryItem[] tempArray = Items;
        Items = new InventoryItem[Capacity];

        for (int i = 0; i < tempArray.Length; i++)
        {
            Items[i] = tempArray[i];
        }
    }

    void CheckEmpty()
    {
        bool isEmpty = true;
        for (int i = 0; i < Items.Length; i++)
        {
            InventoryItem item = Items[i];

            if(item.Name != null && item.Name != "")
            {
                isEmpty = false;
                break;
            }
        }

        if(isEmpty == true)
        {
            WorldController.current.inventoryManager.RemoveInventory(this);
        }
    }

    public bool LookForItem(string itemSubType, out int amount, out int slotID, int quantityToLookFor = 0)
    { //send a -1 to get how many of that item the inv has
        int amountFound = 0;
        int slot = -1;

        for (int i = 0; i < Items.Length; i++)
        {
            InventoryItem item = Items[i];

            if (item.InventorySubType == itemSubType)
            {
                if (quantityToLookFor > 0)
                {
                    amountFound += item.Quantity;

                    if (amountFound >= quantityToLookFor)
                    {
                        amount = amountFound;
                        slotID = i;
                        return true;
                    }
                }
                else if (quantityToLookFor < 0)
                {
                    amountFound += item.Quantity;
                    slot = i;
                }
                else
                {
                    amount = amountFound;
                    slotID = i;
                    return true;
                }
            }
        }

        if (amountFound > 0)
        {
            amount = amountFound;
            slotID = slot;
            return true;
        }

        amount = amountFound;
        slotID = slot;
        return false;
    }

    public InventoryItem AddItem(InventoryItem item, int slotID = -1)
    {
        if (slotID == -1)
        {

            if (HasSpace(item, out slotID) == false)
            {
                return item;
            }

            InventoryItem slot = Items[slotID];

            if (slot.Name == item.Name)
            {
                InventoryItem itemA = StackItems(item, slotID);

                if (itemA == null)
                {
                    return null;
                }
                else
                {
                    return itemA;
                }
            }
            else if (slot.Name == null || slot.Name == "")
            {
                Items[slotID] = item;
                if (this.type == Type.Temp)
                {
                    CheckEmpty();
                }
                return null;
            }
            else
            {
                Debug.LogError("Given slot was not empty and did not match item given?");
                return null;
            }
        }
        else
        {
            InventoryItem oldItem = Items[slotID];
            Items[slotID] = item;
            if(this.type == Type.Temp)
            {
                CheckEmpty();
            }
            return oldItem;
        }
    }

    InventoryItem StackItems( InventoryItem itemA, int slotID)
    {
        InventoryItem itemB = Items[slotID];

        int overflow = itemB.QuantityChange(itemA.Quantity);

        if (overflow <= 0)
        {
            return null;
        }
        else
        {
            itemA.SetQuantity(overflow);
            return itemA;
        }
    }

    bool HasSpace(InventoryItem item, out int slotID)
    {
        if (item.IsStackable == true)
        {
            int ID;
            if (HasStackableSpace(item, out ID) == true)
            {
                slotID = ID;
                return true;
            }
        }

        for (int i = 0; i < Capacity; i++)
        {
            InventoryItem slot = Items[i]; //check each slot for a null item

            if (slot.Name == null || slot.Name == "")
            {
                slotID = i;
                return true;
            }
        }
        slotID = -1;
        return false;
    }

    bool HasStackableSpace(InventoryItem item, out int slotID)
    {
        for (int i = 0; i < Capacity; i++)
        {
            InventoryItem slot = Items[i];

            if (slot.InventorySubType == item.InventorySubType && slot.Quantity < slot.MaxQuantity)
            {
                slotID = i;
                return true;
            }
        }
        slotID = -1;
        return false;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///                                                                                                        ///
    ///                                     SAVING & LOADING                                                   ///
    ///                                                                                                        ///
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer) //Save
    {        
        writer.WriteAttributeString("Capacity", Capacity.ToString());

        if(Items != null)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                writer.WriteStartElement("Item");
                writer.WriteAttributeString("SlotID", i.ToString());
                Items[i].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }

    public void ReadXml(XmlReader reader) //Load
    {
        do
        {
            reader.MoveToAttribute("SlotID");
            int slotID = reader.ReadContentAsInt();
            reader.MoveToAttribute("SubType");
            string subType = reader.ReadContentAsString();
            reader.MoveToAttribute("Quantity");
            int amount = reader.ReadContentAsInt();

            //InventoryItem item = World.current.CreateInventoryItem(subType, amount);

        }
        while (reader.ReadToNextSibling("Item"));


    }
}
