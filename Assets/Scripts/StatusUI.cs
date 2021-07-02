using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

// This class also has a few hats. It basically keeps track of the overall state of the game (food, colonist
// capacity, defense, tasks) and it displays that at the top of the screen. It also places little markers when you
// give a task to make things clear to the player, and it gets rid of them when a task is over.

public class StatusUI : MonoBehaviour
{
    Button advanceDayButton;
    Text foodDisplay;
    Text peopleDisplay;
    Text defenseDisplay;
    Text happinessDisplay;
    Button toggleMusicButton;

    int currentFood;
    int currentColonists;
    int maxColonists;
    int farming;
    int defense;
    int buildingsReclaimed;
    int totalBuildings;
    int daysPassed;
    int currentHappiness;
    int bartending;

    // I admit this is bad game programming practice but I was having some trouble avoiding it. This static
    // variable lets the task class know if a colonist can be added based on the colonists present and the 
    // apartments in the base. This info needs to flow between this class, the tasks, and the colonist manager.
    // Admittedly I could've used scriptable objects to keep a state and do it that way but it just seemed like
    // a weird thing to make so close to finishing the game.
    public static Boolean canAddColonist;

    [SerializeField] GameObject iconPrefab;

    Dictionary<Vector3, GameObject> iconFinder;
    Dictionary<GameObject, Task> iconToTask;
    
    List<Task> taskHolder;

    private void Awake()
    {
        this.advanceDayButton = transform.GetChild(0).GetComponent<Button>();
        this.foodDisplay = transform.GetChild(1).GetComponent<Text>();
        this.peopleDisplay = transform.GetChild(2).GetComponent<Text>();
        this.defenseDisplay = transform.GetChild(3).GetComponent<Text>();
        this.happinessDisplay = transform.GetChild(4).GetComponent<Text>();
        this.toggleMusicButton = transform.GetChild(5).GetComponent<Button>();

        GameEvents.BuildingClicked += OnBuildingClicked;
        GameEvents.BuildingUIClosing += OnBuildingUIClosing;

        this.iconFinder = new Dictionary<Vector3, GameObject>();
        this.iconToTask = new Dictionary<GameObject, Task>();

        this.taskHolder = new List<Task>();


        // Yeah if there's an event this thing pretty well listens to it. Instead of touching
        // everything else in the game to get info it just pays close attention to what they do 
        // and stores the information.

        GameEvents.TaskStarted += OnTaskStarted;
        GameEvents.TaskCompleted += OnTaskCompleted;

        GameEvents.AddColonist += OnAddColonist;
        GameEvents.BuildingReclaimed += OnBuildingReclaimed;
        GameEvents.FoodAdded += OnFoodAdded;
        GameEvents.RemoveColonist += OnRemoveColonist;
        GameEvents.TaskCancelled += OnTaskCancelled;
        GameEvents.RoboAttackUIStarted += OnRoboAttackUIStarted;
        GameEvents.AlertConcluded += OnAlertConcluded;
        GameEvents.AlertStarted += OnAlertStarted;
        GameEvents.HappinessChanged += OnHappinessChanged;
        GameEvents.GameOver += OnGameOver;

        this.currentFood = 20;
        this.currentHappiness = 100;
        this.currentColonists = 0;

        canAddColonist = true;

        this.defense = 0;
        this.farming = 0;
        this.bartending = 0;

        this.buildingsReclaimed = 0;
        this.daysPassed = 0;

        this.totalBuildings = 56;

        // This starts false for the intro message.
        advanceDayButton.interactable = false;

        this.UpdateDisplay();
    }

    void OnAddColonist(object sender, EventArgs args) 
    {
        this.currentColonists += 1;
        if (this.currentColonists >= this.maxColonists)
            canAddColonist = false;

        this.UpdateDisplay();
    }

    // Keeps track of buildings reclaimed, both to note how many apartments and to decide
    // the game difficulty (game gets harder the more buildings you have.)
    void OnBuildingReclaimed(object sender, BuildingEventArgs args) 
    {
        buildingsReclaimed += 1;

        Building sentBuilding = args.buildingPayload;
        switch (sentBuilding.typeName) 
        {
            case BuildingType.Apartment:
                this.maxColonists += 2;
                if (this.currentColonists < this.maxColonists)
                    canAddColonist = true;
                break;
        }

        this.UpdateDisplay();
    }

    // Listens to FoodAdded(which is called by tasks) and does the work of adding the food to the state.
    void OnFoodAdded(object sender, IntEventArgs args) 
    {
        int foodAdded = args.intPayload;
        this.currentFood += foodAdded;

        this.UpdateDisplay();
    }

    // This function is called a ton in this class. It just updates the UI to match whatever changes may have happened
    // state-wise.
    void UpdateDisplay() 
    {
        // This bit puts a + in front of the farming number to indicate you're gaining food,
        // but it doesn't put a sign otherwise because negative numbers come with their sign.
        if((this.farming - this.currentColonists) <= 0)
            this.foodDisplay.text = "Food: " + this.currentFood.ToString() + "/" + (this.farming - this.currentColonists).ToString();
        else
            this.foodDisplay.text = "Food: " + this.currentFood.ToString() + "/+" + (this.farming - this.currentColonists).ToString();

        this.peopleDisplay.text = "Colonists: " + this.currentColonists.ToString() + "/" + this.maxColonists.ToString();
        this.defenseDisplay.text = "Defense: " + this.defense.ToString();
        
        if((this.bartending - this.currentColonists) <= 0)
            this.happinessDisplay.text = "Happiness: " + this.currentHappiness.ToString() + "/" + (this.bartending - this.currentColonists).ToString();
        else
            this.happinessDisplay.text = "Happiness: " + this.currentHappiness.ToString() + "/+" + (this.bartending - this.currentColonists).ToString();

    }

    // Don't click a building and then advance the day, that's weird.
    void OnBuildingClicked(object sender, BuildingEventArgs args) 
    {
        advanceDayButton.interactable = false;
    }


    // Alright yeah you unclicked the building, go ahead and advance the day. You might notice that
    // it doesn't need to check if the taskUI opens and make things uninteractable, by the way. That's 
    // because TaskUI is only open while buildingUI is open. That logic is used by a few other classes for
    // convenience too, so they don't close down their UI twice for no reason.
    void OnBuildingUIClosing(object sender, EventArgs args) 
    {
        advanceDayButton.interactable = true;
    }

    // If the big alert box is on the screen don't click the day.
    void OnAlertStarted(object sender, AlertEventArgs args)
    {
        advanceDayButton.interactable = false;
    }

    public void ToggleMusic() 
    {
        GameEvents.InvokeMusicToggle();
    }

    // This is called when the advance day button is clicked. It does a lot so I'll go over
    // the inside piece by piece, but if you don't feel like reading it lowers the food by your 
    // colonist count and then kills a colonist randomly (by calling a UI event) if you have negative
    // food. It also calls a robot attack randomly (10% of the time) with or without a death depending 
    // on some variables.
    public void AdvanceDay() 
    {
        this.daysPassed += 1;

        // It does try to avoid irritating null reference exceptions if the event isn't assigned.
        try
        {
            GameEvents.InvokeDayAdvanced();
        }
        catch { }

        // Remove food based on colonists
        currentFood += (this.farming - this.currentColonists);
        currentHappiness += (this.bartending - this.currentColonists);

        // You can start an alert that kills a random colonist in a generic way using this event. 
        // It just takes a string to put on the alert itself and one to put on the button to close it.
        // This calls an alert to kill a colonist if they're starving, and give text that matches it to
        // warn them.
        if (currentFood < 0)
        {
            GameEvents.InvokeAlertStarted("One of your colonists died of hunger! Set someone to farm or scavenge to make food.","Noted.", -10);
        }

        if (currentHappiness < 0)
        {
            GameEvents.InvokeAlertStarted("One of your colonists' despair has led them to abandon the colony. Consider assigning bartenders. You gave a speech to raise morale for now but it won't last.", "Noted.", 5);
        }

        // This rolls for a robot attack (10% odds) and then if the defense (set by assigning colonists to protect)
        // is lower than the rolled attack number (1 - the amount of reclaimed buildings) it kills a colonist using
        // the robot attack event. Otherwise it sends a robot attack event that doesn't kill to let the player know
        // they fought them off well.
        float robotAttackRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        if (robotAttackRoll <= .1f)
        {
            int robotAttackSeverity = UnityEngine.Random.Range(1, buildingsReclaimed);
            if (robotAttackSeverity > defense)
            {
                GameEvents.InvokeRoboAttack(true);
            }
            else
            {
                GameEvents.InvokeRoboAttack(false);
            }
        }

        if (this.buildingsReclaimed >= totalBuildings) 
        {
            GameEvents.InvokeGameOver(daysPassed, true);
        }

        // As always, if you change the state update the state display.
        UpdateDisplay();

        //Also update our task icons for the player's convenience.
        UpdateIcons();
    }

    // This is probably an irritating function to read but it listens to the taskStarted event
    // and notes any farmers/defenders added for its state-keeping (and their relevant skills). 
    // It also places a little icon on the map where you started a task so you don't forget where 
    // your people are (and writes the icon into a dictionary with a task so it can find it later
    // to discard it).
    void OnTaskStarted(object sender, TaskEventArgs args) 
    {
        Task startedTask = args.taskPayload;

        Vector3 buildingPos = startedTask.building.worldPosition;
        buildingPos.y += 1f;
        Vector3 iconPos = Camera.main.WorldToScreenPoint(buildingPos);
        Vector3 iconRotation = new Vector3(0, 0, 45);
        GameObject newIcon = Instantiate(iconPrefab, iconPos, Quaternion.Euler(iconRotation), this.transform);
        
        Text iconNumber = newIcon.transform.GetChild(1).GetComponent<Text>();

        // Ten is the number I use internally to say a timer is infinite, and I don't
        // want to give that bad info to the player.
        if (startedTask.durationTimer < 10)
            iconNumber.text = startedTask.durationTimer.ToString();
        else 
        {
            if (startedTask.type == TaskType.Farm)
                iconNumber.text =  "+" + startedTask.colonist.scoutingSkill;

            if (startedTask.type == TaskType.Protect)
                iconNumber.text = "+" + startedTask.colonist.fightingSkill;

            if (startedTask.type == TaskType.Bartend)
                iconNumber.text = "+" + startedTask.colonist.leadershipSkill;
        }

        iconFinder.Add(startedTask.building.worldPosition, newIcon);
        iconToTask.Add(newIcon, startedTask);

        if (startedTask.type == TaskType.Farm)
            farming += startedTask.colonist.scoutingSkill;

        if (startedTask.type == TaskType.Protect)
            defense += startedTask.colonist.fightingSkill;

        if (startedTask.type == TaskType.Bartend)
            bartending += startedTask.colonist.leadershipSkill;

        taskHolder.Add(startedTask);
        UpdateDisplay();
    }

    //This function keeps each task icon updated to its tasks duration.
    void UpdateIcons() 
    {
        foreach (GameObject icon in iconFinder.Values) 
        {
            Text iconNumber = icon.transform.GetChild(1).GetComponent<Text>();
            Task relevantTask = iconToTask[icon];

            // Ten is the number I use internally to say a timer is infinite, and I don't
            // want to give that bad info to the player.
            if (relevantTask.durationTimer < 10)
                iconNumber.text = relevantTask.durationTimer.ToString();
        }
    
    }

    // When the taskCompleted event is called this function gets rid of the icon it placed after finding
    // the task in its Icon-task dictionary. It also takes from your farming/defense task based on the skill
    // of the person you removed.
    void OnTaskCompleted(object sender, TaskEventArgs args) 
    {
        Task finishedTask = args.taskPayload;
       
        finishedTask.building.inTask = false;

        GameObject oldIcon = iconFinder[finishedTask.building.worldPosition];
        iconFinder.Remove(finishedTask.building.worldPosition);
        iconToTask.Remove(oldIcon);
        Destroy(oldIcon);

        if (finishedTask.type == TaskType.Farm)
            farming -= finishedTask.colonist.scoutingSkill;

        if (finishedTask.type == TaskType.Protect)
            defense -= finishedTask.colonist.fightingSkill;

        if (finishedTask.type == TaskType.Bartend)
            bartending -= finishedTask.colonist.leadershipSkill;

        taskHolder.Remove(finishedTask);
        UpdateDisplay();
    }

    // This bit is kind of important. When the RemoveColonist event is called, that colonist
    // obviously should not be able to resolve any tasks assigned to them. This function grabs
    // the dead colonist and searches all the tasks it has for them, and if they have a task it
    // deletes it. 
    //
    // Ideally one colonist should have only one job, and I've never seen that not be
    // the case, but just in case this function checks all tasks for that colonist and can potentially
    // remove multiple..
    void OnRemoveColonist(object sender, ColonistEventArgs args) 
    {
        this.currentColonists -= 1;

        if (this.currentColonists <= this.maxColonists)
            canAddColonist = true;

        if (this.currentColonists <= 0)
            GameEvents.InvokeGameOver(this.daysPassed, false);

        Colonist colonistToRemove = args.colonistPayload;
        int counter = 0;
        int target = taskHolder.Count;
        List<Task> dummyTaskHolder = new List<Task>();

        while (counter < target) 
        {
            if (taskHolder[counter].colonist == colonistToRemove) 
            {
                dummyTaskHolder.Add(taskHolder[counter]);
            }

            counter += 1;
        }

        foreach (Task badTask in dummyTaskHolder) 
        {
            badTask.active = false;
            taskHolder.Remove(badTask);
            GameEvents.InvokeTaskCompleted(badTask);
        }

        dummyTaskHolder = new List<Task>();

        UpdateDisplay();
    
    }

    // If a task is cancelled on a building, this function finds any tasks it might
    // have associated with that building and deletes them so they shouldn't resolve.
    // Again, there shouldn't be two tasks on a building, but for giggles this could
    // potentially remove multiple tasks for a building if they exist.
    void OnTaskCancelled(object sender, BuildingEventArgs args) 
    {
        Building buildingToRemoveTaskFrom = args.buildingPayload;
        int counter = 0;
        int target = taskHolder.Count;
        List<Task> dummyTaskHolder = new List<Task>();

        while (counter < target)
        {
            if (taskHolder[counter].building == buildingToRemoveTaskFrom)
            {
                dummyTaskHolder.Add(taskHolder[counter]);
            }

            counter += 1;
        }

        foreach (Task badTask in dummyTaskHolder)
        {
            badTask.active = false;
            taskHolder.Remove(badTask);
            GameEvents.InvokeTaskCompleted(badTask);
        }
    }

    // Don't try to advance the day on your robot attack alert.
    void OnRoboAttackUIStarted(object sender, ColonistEventArgs args)
    {
        advanceDayButton.interactable = false;
    }

    // Alright alerts over advance the day all you want.
    private void OnAlertConcluded(object sender, EventArgs args)
    {
        advanceDayButton.interactable = true;
    }

    // If some other class wants to change happiness they can call InvokeHappinessChanged with an int
    // and this will add it to the happiness of the town.
    private void OnHappinessChanged(object sender, IntEventArgs happinessDiff) 
    {
        int diff = happinessDiff.intPayload;
        currentHappiness += diff;
        UpdateDisplay();
    }

    // When the game ends I want to be able to reload the active scene to restart. However, you might 
    // notice that the GameEvents I make are static. Static things don't get destroyed on reload, and 
    // Unity seems to throw a huge fit when an event tries to call a function that doesn't exist (because
    // the thing it was tied to is destroyed.) Thus, every function connected to GameEvents must have the 
    // connection severed before the game is reloaded. The connections will be remade by the new versions
    // of these classes on boot.
    void OnGameOver(object sender, GameOverEventArgs args) 
    {
        advanceDayButton.interactable = false;

        GameEvents.BuildingClicked -= OnBuildingClicked;
        GameEvents.BuildingUIClosing -= OnBuildingUIClosing;
        GameEvents.TaskStarted -= OnTaskStarted;
        GameEvents.TaskCompleted -= OnTaskCompleted;
        GameEvents.AddColonist -= OnAddColonist;
        GameEvents.BuildingReclaimed -= OnBuildingReclaimed;
        GameEvents.FoodAdded -= OnFoodAdded;
        GameEvents.RemoveColonist -= OnRemoveColonist;
        GameEvents.TaskCancelled -= OnTaskCancelled;
        GameEvents.RoboAttackUIStarted -= OnRoboAttackUIStarted;
        GameEvents.AlertConcluded -= OnAlertConcluded;
        GameEvents.AlertStarted -= OnAlertStarted;
        GameEvents.HappinessChanged -= OnHappinessChanged;
        GameEvents.GameOver -= OnGameOver;
    }

}
