using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BuildingUI : MonoBehaviour
{
    [SerializeField] CityBuilder city;
    Text typeText;
    Text foodText;
    Text peopleText;
    Text robotText;
    Image uiBox;
    GameObject exitButton;
    GameObject reclaimButton;
    Building building;
    GameObject recruitButton;
    GameObject scavengeButton;
    GameObject trainLeadButton;
    GameObject trainBuildButton;
    GameObject trainKillButton;
    GameObject trainScoutButton;

    // Start is called before the first frame update
    void Start()
    {

        GameEvents.BuildingClicked += OnBuildingClicked;
        GameEvents.TaskUIStarted += OnTaskUIStarted;
        GameEvents.TaskUIClosing += OnTaskUIClosing;
        GameEvents.TaskStarted += OnTaskStarted;
        GameEvents.GameOver += OnGameOver;

        uiBox = this.GetComponent<Image>();
        typeText = transform.GetChild(0).GetComponent<Text>();
        foodText = transform.GetChild(1).GetComponent<Text>();
        exitButton = transform.GetChild(2).gameObject;
        peopleText = transform.GetChild(3).GetComponent<Text>();
        robotText = transform.GetChild(4).GetComponent<Text>();


        scavengeButton = transform.GetChild(1).GetChild(0).gameObject;
        recruitButton = transform.GetChild(3).GetChild(0).gameObject;
        reclaimButton = transform.GetChild(4).GetChild(0).gameObject;

        trainLeadButton = transform.GetChild(5).gameObject;
        trainBuildButton = transform.GetChild(6).gameObject;
        trainKillButton = transform.GetChild(7).gameObject;
        trainScoutButton = transform.GetChild(8).gameObject;

        this.SoftCloseBuildingUI();
    }

    //This function sets up the UI after the player clicks on a building.
    void OnBuildingClicked(object sender, BuildingEventArgs args)
    {
        building = args.buildingPayload;
        OpenBuildingUI();

        WipeText();
        typeText.text = building.typeName.ToString();

        if (!building.inTask)
        {
            if (building.reclaimed)
                SetUpReclaimedUI();
            else
                SetUpWildUI();
        }
        else
            SetUpAlreadyTaskedUI();

    }

    // This is used to close the building UI without telling anyone (invoking the event). 
    // It's only really called at the beginning of the game to prevent weird event side
    // effects.
    void SoftCloseBuildingUI() 
    {
        uiBox.enabled = false;
        typeText.enabled = false;
        foodText.enabled = false;
        exitButton.SetActive(false);
        peopleText.enabled = false;
        robotText.enabled = false;
        reclaimButton.SetActive(false);
        recruitButton.SetActive(false);
        scavengeButton.SetActive(false);
        trainLeadButton.SetActive(false);
        trainBuildButton.SetActive(false);
        trainKillButton.SetActive(false);
        trainScoutButton.SetActive(false);
    }

    //This function disables all the UI components when the player clicks the X button on it.
    //It also activates the closing UI event to let the game know things should be clickable again
    //on the overworld.
    public void CloseBuildingUI()
    {
        uiBox.enabled = false;
        typeText.enabled = false;
        foodText.enabled = false;
        exitButton.SetActive(false);
        peopleText.enabled = false;
        robotText.enabled = false;
        reclaimButton.SetActive(false);
        recruitButton.SetActive(false);
        scavengeButton.SetActive(false);
        trainLeadButton.SetActive(false);
        trainBuildButton.SetActive(false);
        trainKillButton.SetActive(false);
        trainScoutButton.SetActive(false);

        GameEvents.InvokeBuildingUIOver();
    }

    //This function just enables the UI components.
    public void OpenBuildingUI()
    {
        uiBox.enabled = true;
        typeText.enabled = true;
        foodText.enabled = true;
        exitButton.SetActive(true);
        peopleText.enabled = true;
        robotText.enabled = true;
    }

    //This function deletes the text still on the UI from before so certain elements
    //aren't still on there.
    public void WipeText()
    {
        foodText.text = "";
        peopleText.text = "";
        robotText.text = "";
    }

    //This function sets up the UI text and buttons for when the player hasn't reclaimed
    //the building.
    public void SetUpWildUI()
    {
        foodText.text = "Food available: " + building.GetFoodAmountString();
        peopleText.text = "People living here: " + building.peopleCount;

        scavengeButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Scavenge";

        if (building.robotCount == 0)
        {
            robotText.text = "No more robots. Wall it off?";
            reclaimButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Reclaim";
        }
        else
        {
            robotText.text = "Killer robots roaming here: " + building.robotCount;
            reclaimButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Clear out robots";
        }

        if (city.Reclaimable(building))
        {
            if(building.peopleCount > 0 && StatusUI.canAddColonist)
                recruitButton.SetActive(true);
            if(building.food > 0)
                scavengeButton.SetActive(true);
            reclaimButton.SetActive(true);
        }
    }

    //This function sets up the UI text and buttons for when the player has the building in their
    //base already.
    public void SetUpReclaimedUI()
    {

        switch (building.typeName)
        {
            case BuildingType.Hospital:
                foodText.text = "Will help if people get hurt/sick.";
                break;
            case BuildingType.Apartment:
                foodText.text = "Houses two colonists.";
                break;
            case BuildingType.Grocery:
                foodText.text = "Not much here. Coke machine works.";
                break;
            case BuildingType.Farm:
                foodText.text = "Colonists here = food up.";
                scavengeButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Farm";
                scavengeButton.SetActive(true);
                break;
            case BuildingType.PD:
                foodText.text = "Colonists here = defense up.";
                scavengeButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Defend";
                scavengeButton.SetActive(true);
                break;
            case BuildingType.Bar:
                foodText.text = "Bartenders raise happiness.";
                scavengeButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Bartend";
                scavengeButton.SetActive(true);
                break;
            case BuildingType.School:
                foodText.text = "Train colonist skills here.";
                trainLeadButton.SetActive(true);
                trainBuildButton.SetActive(true);
                trainKillButton.SetActive(true);
                trainScoutButton.SetActive(true);
                break;
        }
    }

    // This is called if the building you clicked has a task assigned to it. It allows you to cancel
    // the task if you want, and labels the UI as such.
    public void SetUpAlreadyTaskedUI()
    {
        foodText.text = "Mission Underway.";
        scavengeButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Cancel Task?";
        scavengeButton.SetActive(true);
    }

    //This is just a function to be called by the reclaim button, and it uses an Event to tell
    //cityBuilder to do the work of reclaiming it.
    public void ReclaimBuilding()
    {
        if (building.robotCount == 0)
            GameEvents.InvokeTaskUIStarted(new Task(TaskType.Reclaim, building));
        else
            GameEvents.InvokeTaskUIStarted(new Task(TaskType.Kill, building));
    }

    // This button only shows up if you can recruit someone from an unclaimed spot.
    // On clicking it , it constructs a recruit task and starts up TaskUI to finish it up.
    public void Recruit() 
    {
        GameEvents.InvokeTaskUIStarted(new Task(TaskType.Recruit, building));
    }

    // Similar to the recruit button, but it does different stuff based on the situation because
    // this button is used for all kinds of stuff. It can start farming, protecting, scavenging, 
    // or cancelling the current task depending on what the building is up to (reclaimed or not,
    // tasked or not, its type.) It's tied to a button in the Unity editor.
    public void Scavenge() 
    {
        if (building.inTask)
        {
            GameEvents.InvokeTaskCancelled(building);
            CloseBuildingUI();
        }

        else
        {
            if (building.reclaimed)
            {
                if(building.typeName == BuildingType.Farm)
                    GameEvents.InvokeTaskUIStarted(new Task(TaskType.Farm, building));
                if(building.typeName == BuildingType.PD)
                    GameEvents.InvokeTaskUIStarted(new Task(TaskType.Protect, building));
                if(building.typeName == BuildingType.Bar)
                    GameEvents.InvokeTaskUIStarted(new Task(TaskType.Bartend, building));
            }
            else
                GameEvents.InvokeTaskUIStarted(new Task(TaskType.Scavenge, building));
        }
    }

    public void TrainLeadership() 
    {
        GameEvents.InvokeTaskUIStarted(new Task(TaskType.SchoolLeadership, building));
    }

    public void TrainBuilding()
    {
        GameEvents.InvokeTaskUIStarted(new Task(TaskType.SchoolBuilding, building));
    }

    public void TrainKilling()
    {
        GameEvents.InvokeTaskUIStarted(new Task(TaskType.SchoolDefense, building));
    }

    public void TrainScouting()
    {
        GameEvents.InvokeTaskUIStarted(new Task(TaskType.SchoolScouting, building));
    }

    // So if colonistManagers taskUI is started we don't want to steal its thunder. This shuts down all
    // the buildingUI buttons so the player doesn't click them and make weird stuff happen.
    void OnTaskUIStarted(object sender, TaskEventArgs args) 
    {
        exitButton.GetComponent<Button>().interactable = false;
        recruitButton.GetComponent<Button>().interactable = false;
        reclaimButton.GetComponent<Button>().interactable = false;
        scavengeButton.GetComponent<Button>().interactable = false;
        trainLeadButton.GetComponent<Button>().interactable = false;
        trainBuildButton.GetComponent<Button>().interactable = false;
        trainKillButton.GetComponent<Button>().interactable = false;
        trainScoutButton.GetComponent<Button>().interactable = false;
    }

    // For you indecisive beepboopers, if you cancel the colonist/task menu
    // using the X button on it this will let you do something else on the 
    // selected building.
    void OnTaskUIClosing(object sender, EventArgs args)
    {
        exitButton.GetComponent<Button>().interactable = true;
        recruitButton.GetComponent<Button>().interactable = true;
        reclaimButton.GetComponent<Button>().interactable = true;
        scavengeButton.GetComponent<Button>().interactable = true;
        trainLeadButton.GetComponent<Button>().interactable = true;
        trainBuildButton.GetComponent<Button>().interactable = true;
        trainKillButton.GetComponent<Button>().interactable = true;
        trainScoutButton.GetComponent<Button>().interactable = true;
    }
    // However, if you pick a task in the colonist manager screen, this
    // shuts down the whole building UI. You can't place two tasks in a building
    // anyway so this is probably what the player would want.
    void OnTaskStarted(object sender, TaskEventArgs args) 
    {
        CloseBuildingUI();
    }

    // When the game ends I want to be able to reload the active scene to restart. However, you might 
    // notice that the GameEvents I make are static. Static things don't get destroyed on reload, and 
    // Unity seems to throw a huge fit when an event tries to call a function that doesn't exist (because
    // the thing it was tied to is destroyed.) Thus, every function connected to GameEvents must have the 
    // connection severed before the game is reloaded. The connections will be remade by the new versions
    // of these classes on boot.
    void OnGameOver(object sender, GameOverEventArgs args) 
    {
        GameEvents.BuildingClicked -= OnBuildingClicked;
        GameEvents.TaskUIStarted -= OnTaskUIStarted;
        GameEvents.TaskUIClosing -= OnTaskUIClosing;
        GameEvents.TaskStarted -= OnTaskStarted;
        GameEvents.GameOver -= OnGameOver;
    }
}
