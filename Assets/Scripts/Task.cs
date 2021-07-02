using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Task
{
    public int durationTimer;
    public TaskType type;
    public Colonist colonist;
    public Building building;
    public Boolean active;

    public Task(TaskType inputType, Building inputBuilding) 
    {
        type = inputType;
        building = inputBuilding;

        SetDurationTimer();

        GameEvents.DayAdvanced += OnDayAdvanced;

        active = false;
    }

    void SetDurationTimer() 
    {
        switch (type) 
        {
            case TaskType.Kill:
                durationTimer = 2;
                break;
            case TaskType.Reclaim:
                durationTimer = 2;
                break;
            case TaskType.Recruit:
                durationTimer = 2;
                break;
            case TaskType.Scavenge:
                durationTimer = 2;
                break;
            case TaskType.SchoolLeadership:
                durationTimer = 4;
                break;
            case TaskType.SchoolBuilding:
                durationTimer = 4;
                break;
            case TaskType.SchoolScouting:
                durationTimer = 4;
                break;
            case TaskType.SchoolDefense:
                durationTimer = 4;
                break;
            case TaskType.Farm:
                durationTimer = 10;
                break;
            case TaskType.Protect:
                durationTimer = 10;
                break;
            case TaskType.Bartend:
                durationTimer = 10;
                break;
        }
    }

    void OnDayAdvanced(object sender, EventArgs args) 
    {
        if (this.active)
        {
            if(durationTimer != 10)
                durationTimer -= 1;

            if (durationTimer == 0)
                ResolveTask();
        }
    }

    void ResolveTask() 
    {
        float roll = UnityEngine.Random.Range(0.0f, 1.0f);
        int relevantStat;
        float odds;

        switch (type)
        {
            case TaskType.Kill:
                relevantStat = colonist.fightingSkill - building.robotCount;
                odds = GetSuccessOdds(relevantStat);
                if (odds >= roll) 
                    building.robotCount = 0;
                break;
            case TaskType.Reclaim:
                relevantStat = colonist.buildingSkill;
                odds = GetSuccessOdds(relevantStat);
                if (odds >= roll)
                    GameEvents.InvokeBuildingReclaimed(building);
                break;
            case TaskType.Recruit:
                relevantStat = colonist.leadershipSkill - (building.robotCount / 2);
                odds = GetSuccessOdds(relevantStat);
                if (odds >= roll)
                {
                    int peopleIndex = 0;
                    while (peopleIndex < building.peopleCount && StatusUI.canAddColonist)
                    {
                        GameEvents.InvokeAddColonist();
                        peopleIndex += 1;
                    }
                    building.peopleCount = 0;
                }
                break;
            case TaskType.Scavenge:
                relevantStat = colonist.scoutingSkill - (building.robotCount / 2);
                odds = GetSuccessOdds(relevantStat);
                if (odds >= roll)
                {
                    int food = building.food;
                    int scavenged = 0;

                    if (food < 3) 
                        scavenged = 1;
                    if (food >= 3) 
                        scavenged = 2;
                    if (food >= 7) 
                        scavenged = 3;
                    building.food = 0;

                    GameEvents.InvokeFoodAdded(scavenged);
                }
                break;
            case TaskType.SchoolLeadership:
                colonist.leadershipSkill += 1;
                break;
            case TaskType.SchoolBuilding:
                colonist.buildingSkill += 1;
                break;
            case TaskType.SchoolDefense:
                colonist.fightingSkill += 1;
                break;
            case TaskType.SchoolScouting:
                colonist.scoutingSkill += 1;
                break;
        }

        GameEvents.InvokeTaskCompleted(this);
    }

    public float GetTaskOdds() 
    {
        int relevantStat;

        switch (type)
        {
            case TaskType.Kill:
                relevantStat = colonist.fightingSkill - building.robotCount;
                return GetSuccessOdds(relevantStat);
            case TaskType.Reclaim:
                relevantStat = colonist.buildingSkill;
                return GetSuccessOdds(relevantStat);
            case TaskType.Recruit:
                relevantStat = colonist.leadershipSkill - (building.robotCount / 2);
                return GetSuccessOdds(relevantStat);
            case TaskType.Scavenge:
                relevantStat = colonist.scoutingSkill - (building.robotCount / 2);
                return GetSuccessOdds(relevantStat);
            case TaskType.Farm:
                return colonist.scoutingSkill;
            case TaskType.Protect:
                return colonist.fightingSkill;
            case TaskType.Bartend:
                return colonist.leadershipSkill;
            case TaskType.SchoolScouting:
                return colonist.scoutingSkill + 1;
            case TaskType.SchoolLeadership:
                return colonist.leadershipSkill + 1;
            case TaskType.SchoolBuilding:
                return colonist.buildingSkill + 1;
            case TaskType.SchoolDefense:
                return colonist.fightingSkill + 1;
        }

        return 0f;
    }

    float GetSuccessOdds(int stat) 
    {
        if (stat < 0) return .1f;
        if (stat == 0) return .2f;
        if (stat == 1) return .4f;
        if (stat == 2) return .6f;
        if (stat == 3) return .7f;
        if (stat == 4) return .8f;
        if (stat == 5) return .9f;
        return .95f;
    }
}
