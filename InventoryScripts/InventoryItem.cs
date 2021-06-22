using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using UnityEngine;
using System;
using UnityEngine.UI;

public class InventoryItem : IXmlSerializable
{
    public enum InventoryType
    {
        TOOL,
        CONSUMABLE,
        SEEDS,
        MATERIAL,
        QUEST,
        Currency
    }
    public Inventory inventory;
    public int Slot;
    public int ItemID;
    public InventoryType Type { get; protected set; }
    public string InventorySubType { get; protected set; }
    public bool IsStackable { get; protected set; }
    public int MaxQuantity { get; protected set; }
    public int Quantity { get; protected set; }
    public Dictionary<string,int> Materials { get; protected set; }
    private string _Name = null;
    public string Name
    {
        get
        {
            if (_Name == null || _Name.Length == 0)
            {
                return InventorySubType;
            }
            return _Name;
        }
        set
        {
            _Name = value;
        }
    }
    public string Description = "Usefull stuff";

    protected List<string> rightClickActions;
    protected List<Action<float, InventoryItem>> cbUseAction;
    Action<InventoryItem> cbOnChanged;
    Action<InventoryItem> cbOnRemoved;

    #region BuildFuncs
    public InventoryItem()
    {
        InventorySubType = null;
        ItemID = -1;
        IsStackable = false;
    }

    protected InventoryItem(InventoryItem other)
    {
        this.ItemID             = other.ItemID;
        this.Type               = other.Type;
        this.InventorySubType   = other.InventorySubType;
        this.IsStackable        = other.IsStackable;
        this.MaxQuantity        = other.MaxQuantity;
        this.Quantity           = other.Quantity;
        this.rightClickActions  = other.rightClickActions;
        this.cbUseAction        = other.cbUseAction;
        this.Materials          = other.Materials;
    }

    virtual public InventoryItem Clone()
    {
        return new InventoryItem(this);
    }

    static public InventoryItem CreatePrototype(InventoryType inventoryType, string inventorySubType, int itemID, bool isStackable, int maxQuantity = 1, Dictionary<string, int> materials = null, string useAction = null)
    {
        InventoryItem inv       = new InventoryItem();
        inv.Type                = inventoryType;
        inv.InventorySubType    = inventorySubType;
        inv.ItemID              = itemID;
        inv.IsStackable         = isStackable;
        inv.MaxQuantity         = maxQuantity;
        inv.Materials           = materials;
        if(useAction != null)
        {
            inv.cbUseAction = new List<Action<float, InventoryItem>>();
            inv.RegisterUseAction(InventoryItemActions.GetAction(useAction));
        }


        return inv;
    }
    #endregion

    static public InventoryItem CreateItem(InventoryItem proto, int amount)
    {
        InventoryItem item = proto.Clone();

        if (amount == 0)
        {
            item.Quantity = UnityEngine.Random.Range(10, item.MaxQuantity);
        }
        else
        {
            item.Quantity = amount;
        }

        return item;
    }

    public void Remove()
    {
        if(cbOnRemoved != null)
        {
            cbOnRemoved(this);
        }
    }

    #region HelperFuncs
    public int QuantityChange(int amount) //send me a negative number to remove quantity
    {
        Quantity += amount;
        if (Quantity > MaxQuantity)
        {
            int overflow = Quantity - MaxQuantity;
            Quantity = MaxQuantity;
            return overflow;
        }
        else if (Quantity == 0)
        {
            return 0;
        }
        else if(Quantity < 0)
        {
            return Quantity;
        }
        return 0;
    }

    public void SetQuantity(int amount)
    {
        Quantity = amount;
        if (Quantity <= 0)
        {
            //TODO:
        }
        if(Quantity > MaxQuantity)
        {
            int dif = Quantity - MaxQuantity;
            Quantity = MaxQuantity;
            Debug.LogError("SetQuantity was higher then the items max stackable." + "/n Items lost = " + dif + "/n is this ok?");
        }
    }

    public bool hasUseAction()
    {
        if(cbUseAction != null)
        {
            return true;
        }
        return false;
    }

    public void UseAction()
    {
        if(cbUseAction == null || cbUseAction.Count == 0)
        {
            Debug.Log("No OnUse Actions");
            return;
        }

        foreach (Action<float,InventoryItem> action in cbUseAction)
        {
            action.Invoke(0f, this);
        }
    }

    public List<string> MaterialTypes()
    {
        List<string> keys = new List<string>();
        foreach(string key in Materials.Keys)
        {
            keys.Add(key);
        }
        return keys;
    }

    public int MaterialQuantity(string type)
    {
        return Materials[type];
    }

    public void PickUp(Player character)
    {
        //if (character.inventory.i == character.maxInventorys)
        //{
        //    Debug.LogError("PickUp called when there is no room for me");
        //    return;
        //}

        //this.player = character;
        //character.inventory.Add(this);
        //tile.inventory = null;
        //tile = null;

        //cbOnChanged(this);
    }

    public void PlaceInventory()
    {
        //TODO: impliment PlaceInventory()
    }
    #endregion

    #region CallBacks
    public void RegisterUseAction(Action<float,InventoryItem> callbackFunc)
    {
        cbUseAction.Add(callbackFunc);
    }
    public void UnregisterUseAction(Action<float, InventoryItem> callbackFunc)
    {
        cbUseAction.Remove(callbackFunc);
    }

    public void RegisterOnChangedCallback(Action<InventoryItem> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback(Action<InventoryItem> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }

    public void RegisterOnRemovedCallback(Action<InventoryItem> callbackFunc)
    {
        cbOnRemoved += callbackFunc;
    }
    public void UnegisterOnRemovedCallback(Action<InventoryItem> callbackFunc)
    {
        cbOnRemoved += callbackFunc;
    }
    #endregion

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
        writer.WriteAttributeString("SubType", InventorySubType);
        writer.WriteAttributeString("Quantity", Quantity.ToString());
    }

    public void ReadXml(XmlReader reader) //Load
    {
        reader.MoveToAttribute("SubType");
        string subType = reader.ReadContentAsString();
        reader.MoveToAttribute("Quantity");
        int amount = reader.ReadContentAsInt();
    }

    public void ReadXmlPrototype(XmlReader readerParent) //Load
    {
        readerParent.MoveToAttribute("InventoryType");
        InventoryType iType = (InventoryType)readerParent.ReadContentAsInt();
        readerParent.MoveToElement();

        Materials = new Dictionary<string, int>();
        this.Type = iType;

        XmlReader reader = readerParent.ReadSubtree();//this will read only the sub tree, then we will be ejected to the parent at the end.

        while (reader.Read()) //this will advance forward and then the switch will keep check if any cases match.
        {
            switch (reader.Name)
            {
                case "SubType":
                    reader.Read(); //advance forward to content
                    InventorySubType = reader.ReadContentAsString();
                    Debug.Log(InventorySubType);
                    break;
                case "ItemID":
                    reader.Read();
                    ItemID = reader.ReadContentAsInt();
                    break;
                case "IsStackable":
                    reader.Read();
                    IsStackable = reader.ReadContentAsBoolean();
                    break;
                case "Max":
                    reader.Read();
                    MaxQuantity = reader.ReadContentAsInt();
                    break;
                case "OnClickAction":
                    reader.Read();
                    string onClickName = reader.ReadContentAsString();
                    //Debug.Log("OnClick Action: " + onClickName);
                    cbUseAction = new List<Action<float, InventoryItem>>();
                    RegisterUseAction(InventoryItemActions.GetAction(onClickName));
                    break;
                case "Material":
                    readerParent.MoveToAttribute("Type");
                    string type = reader.ReadContentAsString();
                    readerParent.MoveToAttribute("Amount");
                    int quantity = reader.ReadContentAsInt();

                    Materials.Add(type, quantity);

                    readerParent.MoveToElement();
                    break;
            }
        }
    }
}

