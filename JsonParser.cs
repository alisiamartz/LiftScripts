using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif

/// <summary>
/// Parses our JSON script meant to handle all game scenarios
/// The scenarios are vaguely in order in script
///
/// TODO: Script clean up!!! We are having issues with Quest serialization
/// 
/// by Alisia Martinez, July 2020
/// </summary>

public class JsonParser : MonoBehaviour
{
    public enum ScenariosJSON {Day1Data,PatronData,ScenarioLoadData}
    
    [Tooltip("JSON to load")]
    public ScenariosJSON scenariosJSON;
    [Tooltip("Will begin the first scenario in the JSON and spawn the relevant patron")]
    public bool spawnFirstNodeOnLoad;

    // All of these strings are what we are looking for
    // in JSON, stored so we can easily adjust
    private string _uniqueNodeID = "scenarioID";
    private string _patrons = "patrons";
    private string _patronID = "patronID";
    private string _fsmID = "fsmID";
    private string _floorSpawn = "floorSpawn";
    private string _floorDestination = "floorDestination";

    private string _nextNodeTimeofDayChange = "timeOfDayChange";
    private string _nextNodeExtrasSpawned = "extrasSpawned";
    private string _nextNodeExtrasDestroyed = "extrasDestroyed";
    private string _nextNodeHotelFunctionsCalled = "hotelFunctionsCalled";

    private string _items = "items";
    private string _itemID = "itemID";
    private string _itemLocation = "itemLocation";

    private string _nextScenarios = "nextScenarios";
    private string _nextScenarioID = "nextScenarioID";

    private string _nextScenarioConditions = "nextScenarioConditions";
    private string _nextScenarioConditionsVarName = "varName";
    private string _nextScenarioConditionsValue = "value";


    void Start(){
        loadJSON(scenariosJSON);
    }

    [ContextMenu("Load Scenarios JSON")]
    public void loadJSON(ScenariosJSON json){
        #if UNITY_ANDROID
            StartCoroutine(androidParseJSON(json));
        #else
            string text = File.ReadAllText(Application.streamingAssetsPath+"/"+json.ToString()+".json");
            ParseJSONToDataType(text);
        #endif
    }

    #if UNITY_ANDROID
    IEnumerator androidParseJSON(ScenariosJSON json){
        UnityWebRequest www = UnityWebRequest.Get (Application.streamingAssetsPath+"/"+json.ToString()+".json");
        yield return www.SendWebRequest ();
        string text = www.downloadHandler.text;
        ParseJSONToDataType(text);
    }
    #endif

    void ParseJSONToDataType(string text)
    {

        JSONObject _jsonObject = new JSONObject(text);

        for (int i = 0; i < _jsonObject.list[0].Count; i++)
        {
            JSONObject node = _jsonObject.list[0][i]; // Patron Interactions, basically

            Scenario scenario = new Scenario(
                                            node[_uniqueNodeID].str,  
                                            ParsetoPatrons(node[_patrons]),
                                            (int)node[_floorSpawn].f,
                                            (int)node[_floorDestination].f,
                                            ParseNextScenarios(node[_nextScenarios]
                                            ));

            //there are patrons in the array
            if (node[_patrons].Count <= 0)
                Debug.LogError("There are no patrons in this interaction!");
            // here we are going to deal with the world state goodies
            ParseToWorldStates(node, scenario);
            PatronManager.Instance.ScenarioList.Add(scenario);
        }

        //Load Scenario Nodes
        PatronManager.Instance.SetNodeToLoad(spawnFirstNodeOnLoad);
    }

    /// <summary>
    /// Simplified node parsing without all the extras
    /// Simple ID and condition assignment
    /// </summary>
    /// <param name="nextNodeArray"></param>
    /// <param name="nextNodeConditions"></param>
    /// <returns></returns>
    List<Scenario> ParseNextScenarios(JSONObject nextNodeArray) 
    {
        List<Scenario> nodes = new List<Scenario>();

        for (int i = 0; i < nextNodeArray.Count; i++)
        {
            Scenario nextScenario = new Scenario(nextNodeArray[i][_nextScenarioID].str);

            if (nextNodeArray[i].HasField(_nextScenarioConditions)) 
                nextScenario.scenarioConditions = ParseNodeConditions(nextNodeArray[i][_nextScenarioConditions]);            
            nodes.Add(nextScenario);
        }
        return nodes;      
    }

  //  void ParseStringList(JSONObject stringList, List<string> scenarioReference) {
    List<string> ParseStringList(JSONObject stringList) {
        // go through the list of strings
        // then we store them all to 
        List<string> parsedList = new List<string>();
        if (stringList != null) {
            for (int i = 0; i <= stringList.Count-1; i++)
            {   
                parsedList.Add(stringList[i].str);
            }
            return parsedList;
        }
        return null;
    }

    string CheckandReturnTimeOfDay(JSONObject _nextNodeTimeofDayChange) {
        if (_nextNodeTimeofDayChange != null) {
            return _nextNodeTimeofDayChange.str;
        }
        return null;
    }

    Dictionary<string, int> ParseNodeConditions(JSONObject nodeConditionArray)
    {
        Dictionary<string, int> nodeConditions = new Dictionary<string, int>();
        for (int i = 0; i < nodeConditionArray.Count; i++)
        {
            nodeConditions.Add(
                nodeConditionArray[i][_nextScenarioConditionsVarName].str,
                (int)nodeConditionArray[i][_nextScenarioConditionsValue].f);
        }
        return nodeConditions;
    }

    List<Patron> ParsetoPatrons(JSONObject patronArray)
    {
        // parse the patrons, then make the interaction
        List<Patron> patrons = new List<Patron>();
        for (int i = 0; i < patronArray.Count; i++)
        {
            Patron patron = new Patron(
                    patronArray[i][_patronID].str,
                    patronArray[i][_fsmID].str,
                    TryParseToItems(patronArray[i][_items])
                    // TODO: for later two patron stuff maybe? :D
                    //(state.floor)patronArray[i][_floorSpawn].f,
                    //(state.floor)patronArray[i][_floorDestination].f
                    );
            patrons.Add(patron);
        }
        return patrons;
    }

    List<Item> TryParseToItems(JSONObject itemArray)
    {
        List<Item> items = new List<Item>();
        if (itemArray== null)
        {
          //  Debug.Log("No items in the scenario"); // ideally add scenario name, but like, this is embedded in
            return items;
        }
        for (int i = 0; i < itemArray.Count; i++) {
            Item item = new Item(itemArray[i][_itemID].str, itemArray[i][_itemLocation].str);
            items.Add(item);
        }
        return items;
    }

    void ParseToWorldStates(JSONObject worldStateArray, Scenario scenario) 
    {
            // first we deal with the strings
            if(worldStateArray.HasField(_nextNodeTimeofDayChange))
                scenario.TimeofDayChange = CheckandReturnTimeOfDay(worldStateArray[_nextNodeTimeofDayChange]);
            // now we deal with the list of strings
            if (worldStateArray.HasField(_nextNodeExtrasSpawned))
                scenario.extrasSpawned = ParseStringList(worldStateArray[_nextNodeExtrasSpawned]);
            if (worldStateArray.HasField(_nextNodeExtrasDestroyed))
                scenario.extrasDestroyed = ParseStringList(worldStateArray[_nextNodeExtrasDestroyed]);
            if (worldStateArray.HasField(_nextNodeHotelFunctionsCalled))
                scenario.hotelFunctionsCalled = ParseStringList(worldStateArray[_nextNodeHotelFunctionsCalled]);
    }


}

