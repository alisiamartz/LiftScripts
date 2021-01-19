using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// The Item class holds all the Item data that is parsed from the JSON Data
/// this is all stored inside of the patron
/// 
// by Alisia Martinez, June 2020
///

[System.Serializable]
public class Item {
    [ShowOnly] public string ID;
    [ShowOnly] public PatronFunctions.AccLocations position;
    [ShowOnly] public ItemTypes itemEnum;

    public GameObject itemObject { get; set; }
    public enum ItemTypes {
        None,
        Hat,
        Succulent,
        Money,
        Mustache
    }

    public bool itemWorn; 

    public Item(string name_, string position_)
    {
        ID = name_;
        itemEnum = (ItemTypes)System.Enum.Parse(typeof(ItemTypes), name_);
        position = (PatronFunctions.AccLocations)System.Enum.Parse(typeof(PatronFunctions.AccLocations), position_);
    }

    public bool CheckIfWorn() {
        return itemWorn;
    }
}

