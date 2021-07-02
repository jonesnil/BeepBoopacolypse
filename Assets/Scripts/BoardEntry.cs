using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "You want dat board entry boss?")]
public class BoardEntry : ScriptableObject, IComparable<BoardEntry>
{
    [SerializeField] public string colonyName;
    [SerializeField] public int colonyDays;

    public void SetEntry(string name, int score) 
    {
        colonyName = name;
        colonyDays = score;
    }

    public int CompareTo(BoardEntry entry)
    {
        if (this.colonyDays > entry.colonyDays)
        {
            return -1;
        }
        if (this.colonyDays < entry.colonyDays)
        {
            return 1;
        }
        else
            return 0;
    }
}
