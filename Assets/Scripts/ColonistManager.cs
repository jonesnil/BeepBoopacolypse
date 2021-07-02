using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// This class may have a few too many hats. It handles putting up the colonist selection
// screen, but it also keeps track of the colonists. I had to put them somewhere and it 
// seemed convenient to have them when I need to put up the screen.

public class ColonistManager : MonoBehaviour
{
    Image background;
    GameObject exitButton;
    Text typeText;
    GameObject colonistChoiceOneButton;
    GameObject colonistChoiceTwoButton;
    GameObject colonistChoiceThreeButton;
    Text colonistNameOne;
    Text colonistNameTwo;
    Text colonistNameThree;
    GameObject nextColonistsButton;
    GameObject previousColonistsButton;
    Task currentTask;

    Text colonistStatsOne;
    Text colonistStatsTwo;
    Text colonistStatsThree;

    GameObject cancelTaskSelectionButton;
    GameObject confirmTaskSelectionButton;
    Text taskOdds;

    List<Colonist> assignableColonists;
    List<Colonist> allColonists;

    // Set this in the editor, I don't initialize it here. It's just
    // the number of colonists the player starts with.
    [SerializeField] int startingColonists;

    int UISlideNumber;


    // Start is called before the first frame update
    void Start()
    {
        // Don't let this huge block intimidate you, I'm just grabbing all the components
        // and stuff from colonistManagers children so I can manipulate it later. I tried
        // to name things intuitively, hopefully you can tell what things I'm getting.

        GameEvents.TaskUIStarted += OpenUI;
        background = this.GetComponent<Image>();
        exitButton = transform.GetChild(0).gameObject;
        typeText = transform.GetChild(1).GetComponent<Text>();

        colonistChoiceOneButton = transform.GetChild(2).gameObject;
        colonistNameOne = colonistChoiceOneButton.transform.GetChild(0).GetComponent<Text>();
        colonistStatsOne = transform.GetChild(2).GetChild(1).GetComponent<Text>();

        colonistChoiceTwoButton = transform.GetChild(3).gameObject;
        colonistNameTwo = colonistChoiceTwoButton.transform.GetChild(0).GetComponent<Text>();
        colonistStatsTwo = transform.GetChild(3).GetChild(1).GetComponent<Text>();

        colonistChoiceThreeButton = transform.GetChild(4).gameObject;
        colonistNameThree = colonistChoiceThreeButton.transform.GetChild(0).GetComponent<Text>();
        colonistStatsThree = transform.GetChild(4).GetChild(1).GetComponent<Text>();

        nextColonistsButton = transform.GetChild(5).gameObject;
        previousColonistsButton = transform.GetChild(6).gameObject;

        confirmTaskSelectionButton = transform.GetChild(7).gameObject;
        cancelTaskSelectionButton = transform.GetChild(8).gameObject;
        taskOdds = transform.GetChild(9).GetComponent<Text>();

        GameEvents.TaskCompleted += OnTaskCompleted;
        GameEvents.AddColonist += OnAddColonist;

        GameEvents.RemoveRandomColonist += OnRemoveRandomColonist;
        GameEvents.RemoveColonist += OnRemoveColonist;
        GameEvents.RoboAttack += OnRoboAttack;
        GameEvents.GameOver += OnGameOver;

        // The full colonist list and the list of colonists that aren't in a job are
        // kept separate, to stop the player from assigning someone two jobs at once.
        assignableColonists = new List<Colonist>();
        allColonists = new List<Colonist>();

        // This function is kinda me being lazy. It just spawns a variable number of colonists
        // (you can change the number in the function) using the colonist spawning function 
        // defined elsewhere.
        CreateStartingColonists();

        // This closes the UI stuff for this immediately so it's not on the screen when the game starts.
        CloseUI();
        
    }


    // This function does the tedious work of closing every UI element for this box
    // so I only have to write it once.
    public void CloseUI() 
    {
        colonistStatsOne.enabled = false;
        colonistStatsTwo.enabled = false;
        colonistStatsThree.enabled = false;
        background.enabled = false;
        exitButton.SetActive(false);
        typeText.enabled = false;
        colonistChoiceOneButton.SetActive(false);
        colonistChoiceTwoButton.SetActive(false);
        colonistChoiceThreeButton.SetActive(false);
        nextColonistsButton.SetActive(false);
        previousColonistsButton.SetActive(false);
        confirmTaskSelectionButton.SetActive(false);
        cancelTaskSelectionButton.SetActive(false);
        taskOdds.enabled = false;
        UISlideNumber = 1;
        try
        {
            GameEvents.InvokeTaskUIClosing();
        }
        catch { }
    }

    // If you enjoyed CloseUI, get ready for OpenUI. It does the exact opposite.
    void OpenUI(object sender, TaskEventArgs args) 
    {

        // This bit is worth noting probably. The system keeps track of what "slide"
        // of colonists you're on for the colonist selection bit. It should always be
        // the first slide when you start, so I start it at 1 here. I concede it should
        // probably be 0 for programmer brain reasons but 1 makes the math easier later.
        //
        // Each slide will show up to 3 colonists you can pick from.
        UISlideNumber = 1;

        background.enabled = true;
        exitButton.SetActive(true);
        typeText.enabled = true;
        currentTask = args.taskPayload;

        colonistStatsOne.enabled = true;
        colonistStatsTwo.enabled = true;
        colonistStatsThree.enabled = true;

        //This sets the text at the top to remind the player what kind of task they chose.
        typeText.text = currentTask.type.ToString();

        if (currentTask.type == TaskType.SchoolLeadership)
            typeText.text = "Leadership +1";
        if (currentTask.type == TaskType.SchoolBuilding)
            typeText.text = "Building +1";
        if (currentTask.type == TaskType.SchoolDefense)
            typeText.text = "Killing +1";
        if (currentTask.type == TaskType.SchoolScouting)
            typeText.text = "Scouting +1";
        

        DisplayUISlide();
    }
    
    // This big function does stuff that's kind of easy to explain but tedious to code. I had trouble
    // getting the scrollRect set up, so my compromise was a window that shows up to 3 colonists and lets
    // you hit a button to go back or forth to look at more. This does just that- It displays up to 3 colonists
    // and lets you scroll through the list. The tedious part is that it doesn't show the bottom 1 or 2 buttons 
    // depending on your colonists divided by 3- if you only have two colonists for example, you shouldn't see the
    // third button.
    //
    // It also doesn't show the next slide or previous slide buttons if they shouldn't exist (you're at the beginning/
    // end), and it doesn't try to display if you have no assignable colonists, lots of irritating edge cases like
    // that.
    void DisplayUISlide() 
    {

        if (assignableColonists.Count > 0)
        {
            previousColonistsButton.SetActive(true);

            nextColonistsButton.SetActive(false);

            colonistChoiceOneButton.SetActive(true);
            colonistChoiceTwoButton.SetActive(true);
            colonistChoiceThreeButton.SetActive(true);

            colonistStatsOne.enabled = true;
            colonistStatsTwo.enabled = true;
            colonistStatsThree.enabled = true;

            if (UISlideNumber == 1)
            {
                previousColonistsButton.SetActive(false);
            }


            if (assignableColonists.Count < UISlideNumber * 3)
            {
                int difference = (UISlideNumber * 3) - assignableColonists.Count;
                if (difference == 2)
                {
                    colonistNameOne.text = assignableColonists[(UISlideNumber * 3) - 3].name;
                    colonistStatsOne.text = assignableColonists[(UISlideNumber * 3) - 3].GiveInfoString();
                    colonistChoiceTwoButton.SetActive(false);
                    colonistChoiceThreeButton.SetActive(false);

                    colonistStatsTwo.enabled = false;
                    colonistStatsThree.enabled = false;
                }
                if (difference == 1)
                {
                    colonistNameOne.text = assignableColonists[(UISlideNumber * 3) - 3].name;
                    colonistStatsOne.text = assignableColonists[(UISlideNumber * 3) - 3].GiveInfoString();
                    colonistNameTwo.text = assignableColonists[(UISlideNumber * 3) - 2].name;
                    colonistStatsTwo.text = assignableColonists[(UISlideNumber * 3) - 2].GiveInfoString();
                    colonistChoiceThreeButton.SetActive(false);

                    colonistStatsThree.enabled = false;
                }
            }
            else
            {
                colonistNameOne.text = assignableColonists[(UISlideNumber * 3) - 3].name;
                colonistNameTwo.text = assignableColonists[(UISlideNumber * 3) - 2].name;
                colonistNameThree.text = assignableColonists[(UISlideNumber * 3) - 1].name;

                colonistStatsOne.text = assignableColonists[(UISlideNumber * 3) - 3].GiveInfoString();
                colonistStatsTwo.text = assignableColonists[(UISlideNumber * 3) - 2].GiveInfoString();
                colonistStatsThree.text = assignableColonists[(UISlideNumber * 3) - 1].GiveInfoString();
            }

            if (assignableColonists.Count > UISlideNumber * 3)
            {
                nextColonistsButton.SetActive(true);
            }
        }
        else 
        {
            typeText.text = "No colonists free.";
            colonistStatsOne.text = "";
            colonistStatsTwo.text = "";
            colonistStatsThree.text = "";
        }
    }

    // Set your starting colonist number, count to it, invokeAddColonist
    // once per count to set up the colonists. Ironically most of the work for
    // InvokeAddColonist is done in this class as well so I kind of make it nudge
    // itself to work with its own event call. 
    void CreateStartingColonists() 
    {
        int colonistIndex = 0;

        while (colonistIndex < startingColonists) 
        {
            GameEvents.InvokeAddColonist();
            colonistIndex += 1;
        }
    }

    // New colonist? Roll it randomly using the colonist class, no need to give it any info, then
    // just throw it in both assignableColonists and allColonists (obviously it doesn't have a job
    // coming in fresh.) Maybe unsurprisingly, this is listening to the AddColonist Event.
    void OnAddColonist(object sender, EventArgs args) 
    {
        Colonist newColonist = new Colonist();
        assignableColonists.Add(newColonist);
        allColonists.Add(newColonist);
    }

    // This just adds one to the slide number and nudges the class to display the colonist buttons
    // again. It's called directly by the nextSlide button (it's linked to this in Unity) and that
    // button is only available in contexts where this function makes sense (knock on wood for bugs.)
    public void AdvanceSlide() 
    {
        UISlideNumber += 1;
        DisplayUISlide();
    }
     
    // AdvanceSlides turned around brother, this function just lowers the slide number by one and tells
    // the class to refresh the buttons. It's tied to the previousSlide button from the Unity editor.
    public void RegressSlide()
    {
        UISlideNumber -= 1;
        DisplayUISlide();
    }

    // This is tied to the chooseFirstColonist button in the Unity editor, and it just grabs the appropriate
    // colonist based on the slide number and the button and assigns it to the task it's making. It also
    // opens the confirmation menu so you can decide if you like your odds with that colonist. The next two
    // functions are exactly the same but for the different buttons.
    public void ChooseFirstColonist() 
    {
        currentTask.colonist = assignableColonists[(UISlideNumber * 3) - 3];
        OpenConfirmationUI();
    }

    public void ChooseSecondColonist()
    {
        currentTask.colonist = assignableColonists[(UISlideNumber * 3) - 2];
        OpenConfirmationUI();
    }

    public void ChooseThirdColonist()
    {
        currentTask.colonist = assignableColonists[(UISlideNumber * 3) - 1];
        OpenConfirmationUI();
    }

    // When you pick a colonist, it's time to stop pressing those buttons and decide
    // whether you like your odds of success and want to confirm or cancel your selection.
    // this turns off those old buttons so you don't mess anything up or confuse yourself and
    // displays the new buttons/info you need to decide.
    public void OpenConfirmationUI() 
    {
        confirmTaskSelectionButton.SetActive(true);
        cancelTaskSelectionButton.SetActive(true);
        taskOdds.enabled = true;
        if(currentTask.GetTaskOdds() < 1)
            taskOdds.text = (currentTask.GetTaskOdds()*100).ToString() + "%";
        else
            taskOdds.text = "+" + currentTask.GetTaskOdds().ToString();

        colonistChoiceOneButton.GetComponent<Button>().interactable = false;
        colonistChoiceTwoButton.GetComponent<Button>().interactable = false;
        colonistChoiceThreeButton.GetComponent<Button>().interactable = false;
        nextColonistsButton.GetComponent<Button>().interactable = false;
        previousColonistsButton.GetComponent<Button>().interactable = false;
        exitButton.GetComponent<Button>().interactable = false;
    }

    // This is for you indecisive beepboopers out there. If you hit cancel after picking a colonist,
    // this turns off the confirmation buttons/info and lets you hit the old colonist selection buttons
    // again.
    public void CloseConfirmationUI() 
    {
        confirmTaskSelectionButton.SetActive(false);
        cancelTaskSelectionButton.SetActive(false);
        taskOdds.enabled = false;

        colonistChoiceOneButton.GetComponent<Button>().interactable = true;
        colonistChoiceTwoButton.GetComponent<Button>().interactable = true;
        colonistChoiceThreeButton.GetComponent<Button>().interactable = true;
        nextColonistsButton.GetComponent<Button>().interactable = true;
        previousColonistsButton.GetComponent<Button>().interactable = true;
        exitButton.GetComponent<Button>().interactable = true;
    }

    // You might have noticed the UI aspect of this class basically just creates a Task and finally
    // sends it on its way to resolve itself. I found this a convenient way to set up Tasks, but I had
    // to put in a failsafe to make sure that a task doesn't resolve and help the player if it's cancelled,
    // so it has an "active" variable. I set it to true here, so only now that you hit confirm is it actually
    // position to resolve itself. I also grab its building and finally set it to "inTask", so that later I can
    // recognize that someone is positioned there and look up the task (in StatusUI, which really handles the
    // current tasks. This class just makes them).
    //
    // I also have to remove the chosen colonist from assignableColonists, so the player doesn't get any funny ideas
    // about double assigning anyone.
    public void OnConfirmPressed() 
    {
        CloseConfirmationUI();
        assignableColonists.Remove(currentTask.colonist);
        GameEvents.InvokeTaskStarted(currentTask);
        currentTask.building.inTask = true;
        currentTask.active = true;
        CloseUI();
    }

    // This just calls the function to close the extra confirmation buttons. It's tied to the cancel button,
    // which should only be available when the confirmation screen is up.
    public void OnCancelPressed() 
    {
        CloseConfirmationUI();
    }

    // This is tied to the red exit button in the corner, and it just sets the task it's making to active=false
    // (to make absolutely sure it doesn't resolve its effect), deletes the reference to it, and closes the whole
    // UI bit of this class.
    public void OnExitPressed() 
    {
        currentTask.active = false;
        currentTask = null;
        CloseUI();
    }

    // This is tied to the TaskCompleted event. The event sends the task completed, and this grabs it and yanks
    // out the colonist so it can allow it to be assigned a new job. This is accomplished by re-adding it to
    // the assignableColonists list.
    void OnTaskCompleted(object sender, TaskEventArgs args) 
    {
        Task task = args.taskPayload;
        assignableColonists.Add(task.colonist);
    }

    // This is tied to the removeRandomColonist function, and it just grabs a random colonist from its range
    // of allColonists (no, you can't protect your colonists by assigning them to a job) and invokes RemoveColonist
    // on them. The work for that event also mostly comes back to this class, but it's separate because sometimes
    // other classes may want to remove a specific colonist. I will concede this is a theoretical interest at this
    // point though, I don't think I actually do that yet.
    void OnRemoveRandomColonist(object sender, EventArgs args) 
    {
        GameEvents.InvokeRemoveColonist(allColonists[UnityEngine.Random.Range(0, allColonists.Count - 1)]);
    }

    // This function listens to the RemoveColonist event, and it removes the colonist given in the args from
    // both lists (if they're in the lists.) It's mostly important to check if they're in the list because
    // sometimes a colonist will be in assignable and sometimes not depending on if they're given a Task.
    void OnRemoveColonist(object sender, ColonistEventArgs args) 
    {
        Colonist colonistToRemove = args.colonistPayload;
        if (allColonists.Contains(colonistToRemove))
            allColonists.Remove(colonistToRemove);
        
        if(assignableColonists.Contains(colonistToRemove))
            assignableColonists.Remove(colonistToRemove);
    }

    // In the event of a robot attack, I want to tell the player who was killed. That's why the event this
    // function is listening to exists, and this function call is a middleman between statusUI and GameOverUI.
    // 
    // StatusUI says "Ok, I rolled a robot attack. Colonist Manager, tell us who was killed?" and it calls the
    // roboAttack event. This function figures out a random colonist based on its lists, and then it says
    // "So I know who died, now someone put this on the screen" and when it calls RoboAttackUI it gives up the
    // lucky colonist. 
    //
    // However, sometimes no one dies in a roboAttack, so ColonistManager also sends a boolean
    // that tells this function whether or not anyone was killed. If no one was killed this function still opens
    // the UI but it tells GameOverUI there was no casualty by sending null instead of a colonist. Apologies for the
    // novel but this is one of the more convoluted implementations so I wanted to make it clear what's happening.
    void OnRoboAttack(object sender, BooleanEventArgs args) 
    {
        Boolean casualty = args.booleanPayload;

        if (casualty)
        {
            Colonist dead = allColonists[UnityEngine.Random.Range(0, allColonists.Count - 1)];
            GameEvents.InvokeRoboAttackUIStarted(dead);
        }
        else
        {
            GameEvents.InvokeRoboAttackUIStarted(null);
        }
    }

    // When the game ends I want to be able to reload the active scene to restart. However, you might 
    // notice that the GameEvents I make are static. Static things don't get destroyed on reload, and 
    // Unity seems to throw a huge fit when an event tries to call a function that doesn't exist (because
    // the thing it was tied to is destroyed.) Thus, every function connected to GameEvents must have the 
    // connection severed before the game is reloaded. The connections will be remade by the new versions
    // of these classes on boot.
    void OnGameOver(object sender, GameOverEventArgs args) 
    {
        GameEvents.TaskUIStarted -= OpenUI;
        GameEvents.TaskCompleted -= OnTaskCompleted;
        GameEvents.AddColonist -= OnAddColonist;
        GameEvents.RemoveRandomColonist -= OnRemoveRandomColonist;
        GameEvents.RemoveColonist -= OnRemoveColonist;
        GameEvents.RoboAttack -= OnRoboAttack;
        GameEvents.GameOver -= OnGameOver;
    }


}
