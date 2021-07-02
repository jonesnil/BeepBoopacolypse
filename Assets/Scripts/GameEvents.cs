using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;

// Might have gone a bit overboard with events in this game but believe it or not I actually found them really
// convenient. I had used them before but didn't really understand them until this class and used this project to 
// explore using them. I'd rather not catalogue each event and what it does here but I tried to name them intuitively
// and I do describe what they effect in the code when they're called in the comments on other classes

public class BuildingEventArgs : EventArgs
{
    public Building buildingPayload;
}

public class TaskEventArgs : EventArgs
{
    public Task taskPayload;
}

public class IntEventArgs : EventArgs 
{
    public int intPayload;
}

public class ColonistEventArgs : EventArgs
{
    public Colonist colonistPayload;
}

public class BooleanEventArgs : EventArgs
{
    public Boolean booleanPayload;
}

public class AlertEventArgs : EventArgs 
{
    public string alertString;
    public string buttonString;
    public int happinessDiff;
}

public class GameOverEventArgs : EventArgs
{
    public int daysPassed;
    public Boolean gameWon;
}

public static class GameEvents
{

    public static event EventHandler<BuildingEventArgs> BuildingClicked;
    public static event EventHandler BuildingUIClosing;
    public static event EventHandler<BuildingEventArgs> BuildingReclaimed;
    public static event EventHandler<TaskEventArgs> TaskUIStarted;
    public static event EventHandler TaskUIClosing;
    public static event EventHandler DayAdvanced;
    public static event EventHandler<TaskEventArgs> TaskStarted;
    public static event EventHandler<TaskEventArgs> TaskCompleted;
    public static event EventHandler AddColonist;
    public static event EventHandler<IntEventArgs> FoodAdded;
    public static event EventHandler RemoveRandomColonist;
    public static event EventHandler<ColonistEventArgs> RemoveColonist;
    public static event EventHandler<GameOverEventArgs> GameOver;
    public static event EventHandler<BuildingEventArgs> TaskCancelled;
    public static event EventHandler<BooleanEventArgs> RoboAttack;
    public static event EventHandler<ColonistEventArgs> RoboAttackUIStarted;
    public static event EventHandler AlertConcluded;
    public static event EventHandler<AlertEventArgs> AlertStarted;
    public static event EventHandler<IntEventArgs> HappinessChanged;
    public static event EventHandler MusicToggle;

    public static void InvokeBuildingClicked(Building building)
    {
        BuildingClicked(null, new BuildingEventArgs { buildingPayload = building });
    }

    public static void InvokeBuildingUIOver()
    {
        BuildingUIClosing(null, EventArgs.Empty);
    }

    public static void InvokeBuildingReclaimed(Building building)
    {
        BuildingReclaimed(null, new BuildingEventArgs { buildingPayload = building });
    }

    public static void InvokeTaskUIStarted(Task task)
    {
        TaskUIStarted(null, new TaskEventArgs { taskPayload = task });
    }

    public static void InvokeTaskUIClosing()
    {
        TaskUIClosing(null, EventArgs.Empty);
    }

    public static void InvokeDayAdvanced()
    {
        DayAdvanced(null, EventArgs.Empty);
    }

    public static void InvokeTaskStarted(Task task)
    {
        TaskStarted(null, new TaskEventArgs { taskPayload = task });
    }

    public static void InvokeTaskCompleted(Task task)
    {
        TaskCompleted(null, new TaskEventArgs { taskPayload = task });
    }

    public static void InvokeAddColonist()
    {
        AddColonist(null, EventArgs.Empty);
    }

    public static void InvokeFoodAdded(int sentInt)
    {
        FoodAdded(null, new IntEventArgs { intPayload = sentInt });
    }

    public static void InvokeRemoveRandomColonist()
    {
        RemoveRandomColonist(null, EventArgs.Empty);
    }

    public static void InvokeRemoveColonist(Colonist colonist)
    {
        RemoveColonist(null, new ColonistEventArgs { colonistPayload = colonist });
    }

    public static void InvokeGameOver(int daysGone, bool win)
    {
        GameOver(null, new GameOverEventArgs { daysPassed = daysGone, gameWon = win});
    }

    public static void InvokeTaskCancelled(Building building) 
    {
        TaskCancelled(null, new BuildingEventArgs { buildingPayload = building });
    }

    public static void InvokeRoboAttack(Boolean boolean)
    {
        RoboAttack(null, new BooleanEventArgs { booleanPayload = boolean });
    }

    public static void InvokeRoboAttackUIStarted(Colonist colonist)
    {
        RoboAttackUIStarted(null, new ColonistEventArgs { colonistPayload = colonist });
    }

    public static void InvokeAlertConcluded()
    {
        AlertConcluded(null, EventArgs.Empty);
    }

    public static void InvokeAlertStarted(String alert, String button, int happinessDifference)
    {
        AlertStarted(null, new AlertEventArgs {alertString = alert, buttonString = button, happinessDiff = happinessDifference });
    }

    public static void InvokeHappinessChanged(int happiness)
    {
        HappinessChanged(null, new IntEventArgs { intPayload = happiness });
    }


    public static void InvokeMusicToggle()
    {
        MusicToggle(null, EventArgs.Empty);
    }
}