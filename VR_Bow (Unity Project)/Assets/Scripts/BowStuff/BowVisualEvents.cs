using System;
using UnityEngine;

public static class BowVisualEvents
{
    public static Action<float> OnChargeLevelChanged;  // charge 0–1
    public static Action OnArrowReleased;              // flash white
    public static Action OnBowIdle;                    // back to black
}
