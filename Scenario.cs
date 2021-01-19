using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

///
/// The Scenario class holds all the information that is parsed from the JSON Data
/// Including:
/// unique scenario ID, patrons, spawn and destination floors, the extra information
/// any possible follow up scenario with the memory condition needed to be satisfied
/// 
/// Most of these are called inside of JSONParser.cs
/// 
// by Alisia Martinez, June 2020
///

[System.Serializable]
public class Scenario
{

    /// <summary>
    /// The conversationNode holds all of the things necessary to load a whole node
    /// Listed in order of appearance in JSON data
    /// </summary>
    [ShowOnly] public string scenarioID;
    [ShowOnly] public List<Patron> patrons;
    [ShowOnly] public int floorSpawn;
    [ShowOnly] public int floorDestination;

    public string TimeofDayChange;
    public List<string> extrasSpawned = new List<string>();
    public List<string> extrasDestroyed = new List<string>();
    public List<string> hotelFunctionsCalled = new List<string>();

    [ShowOnly] public List<Scenario> nextScenarios;

    private string nextScenarioString;
    public Dictionary<string, int> scenarioConditions = new Dictionary<string, int>();

    int hotelFunctionsIndex = 0;

    private bool defaultScenario;

    // Scenario Constructors -----------------------------

    public Scenario() { }

    public Scenario(string _nextNodeID)
    {
        scenarioID = _nextNodeID;
    }

    public Scenario(string _nextNodeID, Dictionary<string, int> _nodeConditions)
    {
        scenarioID = _nextNodeID;
        scenarioConditions = _nodeConditions;
    }
        public Scenario(string _uniqueNodeID,
                        List<Patron> _patrons,
                        int _floorSpawn,
                        int _floorDestination,
                         List<Scenario> _nextNodes
                        )
    {
        scenarioID = _uniqueNodeID;
        patrons = _patrons;
        floorSpawn = _floorSpawn;
        floorDestination = _floorDestination;
        nextScenarios = _nextNodes;
    }

    // Scenario Functions -----------------------------

    public string GetScenarioID()
    {
        return scenarioID;
    }

    // Next Scenario Fucntions ---------------------------

    public string CheckNextScenario()
    {
        return nextScenarioString;
    }

    public string GetNextScenario()
    {
        foreach (Scenario node in nextScenarios)
        {
            // if we find the non default node
            // we check to see if conditions are satisfied
            if (!node.CheckIfDefaultNode() && node.CheckMemoryToNodeConditions())
            {
                // get one condition at a time
                // we return whatevr this node is
                nextScenarioString = node.scenarioID;
                return nextScenarioString;
            }
            else if (node.CheckIfDefaultNode())
            {
                nextScenarioString = node.scenarioID;
            }
        }
        return nextScenarioString;
    }

    // If only one scenario possible
    public bool CheckIfDefaultNode()
    {
        GetDefaultNode();
        return defaultScenario;
    }

    private void GetDefaultNode()
    {
        if (NodeConditionsPresent())
            defaultScenario = false;
        else
            defaultScenario = true;
    }

    // We need to count parameters for some parsed data to check if we need to further parse
    // ex: we don't need to parse memory conditions if no memory conditiosn are necessary
    // eg, if the node only has one next scenario possible
    public bool NodeConditionsPresent()
    {
        if (scenarioConditions.Count == 0)
            return false;
        return true;
    }

    // go through conditions to see if theres more than one
    public Dictionary<string, int> ConditionFetch()
    {
        return scenarioConditions;
    }

    // Checking memory condition to see if it is satisfied to calculate correct next scenario
    public bool CheckMemoryToNodeConditions()
    {
        foreach (KeyValuePair<string, int> condition in scenarioConditions)
        {
            if (PatronMemory.fetchPatronMemory(condition.Key) != condition.Value)    
                return false;
        }
        return true;
    }

    public IEnumerator LoadWorldStateQueue()
    {
        yield return new WaitUntil(() => ElevatorManager.Instance.doorOpen == false);
        LoadExtraQueue(); 
    }

    void LoadExtraQueue()
    {
        if (extrasSpawned.Count > 0)
        {
            for (int i = 0; i < extrasSpawned.Count; i++)
            {
                ExtraManager.Instance.setExtraToActive(extrasSpawned[i]);
            }
        }
        if (extrasDestroyed != null)
        {
            for (int i = 0; i < extrasDestroyed.Count; i++)
            {
                ExtraManager.Instance.setExtraInactive(extrasDestroyed[i]);
            }
        }
    }


    ///
    // This starts the functions parsed from JSON data
    // It just receives the string and then translates that to a function call
    ///
    public void invokeHotelFunction(){
        if(hotelFunctionsIndex < hotelFunctionsCalled.Count){
            Type thisType = PatronManager.Instance.hotelFunctions.GetType();
            MethodInfo theMethod = thisType.GetMethod(hotelFunctionsCalled[hotelFunctionsIndex]);
            theMethod.Invoke(PatronManager.Instance.hotelFunctions, new object[0] /* We could support passing arguments to these functions here if we needed */);
            hotelFunctionsIndex++;
        }else{
            Debug.LogError("hotelFunctionsIndex larger than list of functions assigned this Scenario. Index: "+hotelFunctionsIndex);
        }
    }
}
