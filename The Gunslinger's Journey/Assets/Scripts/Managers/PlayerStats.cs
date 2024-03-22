using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStats
{
    public static int Health { get; set; } = -1;
    public static int Flow { get; set; } = -1;
    public static int Level { get; set; } = 1;
    public static bool SpawnSet { get; set; } = false;
    public static float XCoordinate { get; set; }
    public static float YCoordinate { get; set; }
}
