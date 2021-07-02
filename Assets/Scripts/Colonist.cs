using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist
{
    public String name;
    public Boolean taskAssigned;
    public int scoutingSkill;
    public int fightingSkill;
    public int buildingSkill;
    public int leadershipSkill;
    static String[] firstNames;
    static String[] lastNames;


    public Colonist() 
    {
        taskAssigned = false;
        GenerateSkills();

        if (firstNames == null) firstNames = GenerateFirstNames();
        if (lastNames == null) lastNames = GenerateLastNames();

        name = firstNames[UnityEngine.Random.Range(0, firstNames.Length - 1)] + " " + lastNames[UnityEngine.Random.Range(0, lastNames.Length - 1)];
    }

    private void GenerateSkills() 
    {
        scoutingSkill = UnityEngine.Random.Range(1, 7);
        fightingSkill = UnityEngine.Random.Range(1, 7);
        buildingSkill = UnityEngine.Random.Range(1, 7);
        leadershipSkill = UnityEngine.Random.Range(1, 7);
    }

    private String[] GenerateFirstNames() 
    {
        String[] output = new String[] { "Alex", "James", "Jacob", "Chris", "Chabane", "Dalton", "Nick", "Thaddeus", "Alexander", "Aaron", "Thomas", "Arnold", "John", "Michael", "Billy-Bob",
                                         "Miranda", "Theodosia", "Eliza", "Angelica", "Peggy", "Emily", "Mackenzie", "Tabitha", "Aries", "Chie", "Sammy", "Betty", "Gertrude", "Trudy", "Anna"};
        return output;
    }

    private String[] GenerateLastNames()
    {
        String[] output = new String[] { "Jo", "Jones", "Hamilton", "Burr", "Jefferson", "Schuyler", "Jacobs", "Grant", "Gratzer", "Pattinson", "Salvatore", "Forbes", "Smith", "Gilbert", "Allison",
                                         "Combs", "Narukami", "Bennett", "Hanson", "Klaus", "The Third", "Mikaelson", "Maidi", "Sanchez", "Thompson", "Donovan", "Wesley", "Potter", "Dobrev", "Cranston"};
        return output;
    }

    public String GiveInfoString() 
    {
        return "Kill: " + fightingSkill + " Lead: " + leadershipSkill + " Scout: " + scoutingSkill + " Build: " + buildingSkill;
    
    }

}
