﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public int teamID;

    // TODO: add unit manager for individual teams
    public GameObject[] units;

    public void Reset()
    {
        foreach(var unit in units)
        {
            unit.GetComponent<PlayerMove>().Reset();
            unit.GetComponent<PlayerCombat>().Reset();
        }
    }
}
