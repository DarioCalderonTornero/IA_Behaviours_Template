using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Necesitamos saber en todo momento el conjunto de obstaculos en el
//mundo donde el NPC se pueda esconder (etiquetados como "hide").
public sealed class World
{
    private static readonly World instance = new World();
    private static GameObject[] hidingSpots;

    static World() //Constructor
    {
        hidingSpots = GameObject.FindGameObjectsWithTag("hide");
    }
    private World() { }

    public static World Instance
    {
        get { return instance; }
    }
    public GameObject[] GetHidingSpots()
    {
        return hidingSpots;
    }
}