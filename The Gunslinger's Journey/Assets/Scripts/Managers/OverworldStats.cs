using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OverworldStats
{
    public static int TotalEnemies { get; set; } = -1;
    public static int CurrentScene { get; set; } = 1;
    public static bool ExitSet { get; set; } = false;
    public static bool KeySet { get; set; } = false;
    public static bool CanExit { get; set; } = false;
    public static float XCoordinateExit { get; set; }
    public static float YCoordinateExit { get; set; }
    public static float XCoordinateKey { get; set; }
    public static float YCoordinateKey { get; set; }
}
