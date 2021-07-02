using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// This class can restart the game when it's over and do that UI stuff, but it also handles general alerts.
// They are all bad at least as of now, so it gives you spooky robot font text and a gray box.

public class GameOverUI : MonoBehaviour
{
    Image backgroundPanel;
    Image mainPanel;
    Text gameOverText;
    Colonist dead;
    GameObject restartButton;
    Text dayDisplay;

    // This is just a variable GameOverUI keeps track of itself, and changes everytime an alert is called.
    // It tells the class what to do when the button attached to it is clicked.
    AlertType alertType;

    void Start()
    {
        backgroundPanel = transform.parent.GetComponent<Image>();
        mainPanel = transform.GetComponent<Image>();
        gameOverText = transform.GetChild(0).GetComponent<Text>();
        restartButton = transform.GetChild(1).gameObject;
        dayDisplay = transform.GetChild(2).GetComponent<Text>();
        dead = null;


        GameEvents.RoboAttackUIStarted += OnRoboAttackUIStarted;
        GameEvents.AlertStarted += OnAlertStarted;
        GameEvents.GameOver += OnGameOver;

        OpenGameOverUI();
        alertType = AlertType.Start;
        gameOverText.text = "You are the leader of a small resistance group against the robot menace. Feed protect and placate your colonists as you expand outwards. If you can reclaim every building in the city you will have saved your people.";
        dayDisplay.text = "";
        restartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Godspeed, General";

    }

    // Does the tedious work of closing the UI box.
    void CloseGameOverUI() 
    {
        dayDisplay.text = "";
        backgroundPanel.enabled = false;
        mainPanel.enabled = false;
        gameOverText.enabled = false;
        restartButton.SetActive(false);
        dayDisplay.enabled = false;
    }

    // Oh boy, this one opens the UI box.
    void OpenGameOverUI()
    {
        backgroundPanel.enabled = true;
        mainPanel.enabled = true;
        gameOverText.enabled = true;
        restartButton.SetActive(true);
        dayDisplay.enabled = true;
    }

    // When you reach game over, this gives the relevant alert text/button text and situates itself to restart
    // the game when you click its button. It also severs the connection between the events and its methods so
    // when you hit restart they won't break the game on reloading the scene.
    void OnGameOver(object sender, GameOverEventArgs args) 
    {
        int daysGone = args.daysPassed;
        bool victory = args.gameWon;
        alertType = AlertType.GameOver;

        if (victory)
        {
            gameOverText.text = "You have vanquished all robots in the city and reclaimed your independence. You've done well for your people, but what of the others?";
            dayDisplay.text = "You reigned: " + daysGone + " days.";
            restartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Relocate?";
        }

        else
        {
            gameOverText.text = "All of your colonists have died, whether by starvation or the robot menace. May the human legacy live on in their beeps.";
            dayDisplay.text = "You survived: " + daysGone + " days.";
            restartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Restart?";
        }

        GameEvents.RoboAttackUIStarted -= OnRoboAttackUIStarted;
        GameEvents.AlertStarted -= OnAlertStarted;
        GameEvents.GameOver -= OnGameOver;

        OpenGameOverUI();
    }

    // When a robot attack happens this sets the text depending on if anyone died, and changes the alert type
    // to the right one.
    void OnRoboAttackUIStarted(object sender, ColonistEventArgs args) 
    {
        alertType = AlertType.RobotAttack;

        OpenGameOverUI();

        if (args.colonistPayload != null)
        {
            dead = args.colonistPayload;
            gameOverText.text = "ROBOT ATTACK: They rushed us bad last night. We held them off for now, but " + dead.name + " was killed.";
            dayDisplay.text = "Happiness -10";
            GameEvents.InvokeHappinessChanged(-10);
            restartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Goodbye, " + dead.name;
        }
        else
        {
            gameOverText.text = "ROBOT ATTACK: They attacked last night, but we were ready. Even went out and popped the heads to stop the beeping.";
            dayDisplay.text = "Happiness +10";
            GameEvents.InvokeHappinessChanged(10);
            restartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Nice.";
        }

        
    }

    // This is like the last two but for miscellaneous generic alerts that kill a colonist. It just displays
    // the strings that come in for the button and alert text.
    void OnAlertStarted(object sender, AlertEventArgs args)
    {
        alertType = AlertType.Misc;
        int happinessDiff = args.happinessDiff;
        OpenGameOverUI();

        gameOverText.text = args.alertString;

        if (happinessDiff <= 0)
            dayDisplay.text = "Happiness " + happinessDiff;
        else
            dayDisplay.text = "Happiness +" + happinessDiff;

        GameEvents.InvokeHappinessChanged(happinessDiff);
        restartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = args.buttonString;
    }

    // This is linked to what's called the Restart button. To be honest at this point it's really just the
    // close alert button though, with how much I reused this UI. It checks what alertType it has when you click 
    // it, and closes the alert in a way that makes sense depending on the type.
    public void RestartGame() 
    {
        switch (alertType) 
        {
            //If the alert is gameover, just restart the game.
            case AlertType.GameOver:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;

            // If the alert is robot attack, check if anyone died, and if so remove the colonist
            // and reset the dead variable. Otherwise just end the alert and dip. 
            case AlertType.RobotAttack:
                CloseGameOverUI();
                GameEvents.InvokeAlertConcluded();
                if (dead != null)
                {
                    GameEvents.InvokeRemoveColonist(dead);
                    dead = null;
                }
                break;

            case AlertType.Start:
                CloseGameOverUI();
                GameEvents.InvokeAlertConcluded();
                break;

            // If this is a miscellaneous colonist killing alert just kill the colonist at random,
            // end the event and dip.
            case AlertType.Misc:
                CloseGameOverUI();
                GameEvents.InvokeAlertConcluded();
                GameEvents.InvokeRemoveRandomColonist();
                break;
        }
    }

}
