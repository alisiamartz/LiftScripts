using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// The Patron class holds all the Patron data that is parsed from the JSON Data
/// Including:
/// name of patron, FSM id to load, items and load locations
/// TODO: maybe add floor spawn/destination to support 2 patron convos later?
/// 
/// Most of these are called inside of JSONParser.cs
/// 
// by Alisia Martinez, June 2020
///


[System.Serializable]
public class Patron
{
    [ShowOnly] public string name;
    [ShowOnly] public string FSM;

    public GameObject gameObject { get; set; }
    [ShowOnly]public List<Item> itemsWorn = new List<Item>();

    // TODO: Set the head and hands stuff here too?
    // Because right now it assigns this in Patron Functions
    GameObject accessoryParent;

    public Transform[] AccLocationTransforms = new Transform[4];

    [System.Serializable]
    public struct PatronPrefabs
    { //Made this a struct so it's collapsible in editor
        public GameObject leftHand;
        public GameObject head;
        public GameObject rightHand;
    }

    public PatronPrefabs patronPrefabs;


    public Patron(string patronName, string FSMName_)
    {
        name = patronName;
        FSM = FSMName_;
    }

    public Patron(string patronName, string FSMName_, List<Item> items_)
    {
        name = patronName;
        FSM = FSMName_;
        itemsWorn = items_;
    }

    public void setGameObject(GameObject patronObject_)
    {
        gameObject = patronObject_;
    }

    public bool hasObjects() {
        if (itemsWorn.Count == 0) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// TODO: Get rid of gross transform magic numbers >:(
    /// THIS IS WHAT WE CALL IN PLAYMAKER TO SPAWN AN ACCESORY AT A CERTAIN POINT
    /// 
    /// Remember to do items w caps
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="parentLocation"></param>
    /// <param name="held"></param>
    public GameObject LoadAccessory(Item.ItemTypes accessory, PatronFunctions.AccLocations parentLocation, bool held, PatronFunctions patronFunctions)
    {
        accessoryParent = PatronManager.Instance.EnableAccessory(accessory, held);
        switch (parentLocation)
        {
            case PatronFunctions.AccLocations.Face:
                accessoryParent.transform.SetParent(patronFunctions.AccLocationTransforms[0].gameObject.transform);
                accessoryParent.transform.localPosition = Vector3.zero;
                break;
            case PatronFunctions.AccLocations.Head:
                accessoryParent.transform.parent = patronFunctions.AccLocationTransforms[1].transform;
                accessoryParent.transform.localPosition = Vector3.zero;
                break;
            case PatronFunctions.AccLocations.HandR:
                accessoryParent.transform.parent = patronFunctions.AccLocationTransforms[2].transform;
                accessoryParent.transform.localPosition = Vector3.zero;
                break;
            case PatronFunctions.AccLocations.HandL:
                accessoryParent.transform.parent = patronFunctions.AccLocationTransforms[3].transform;
                accessoryParent.transform.localPosition = Vector3.zero;
                break;
            default:
                Debug.LogError("Tried to load in " + accessory + " at the " + parentLocation);
                break;
        }
        return accessoryParent;
    }
}

