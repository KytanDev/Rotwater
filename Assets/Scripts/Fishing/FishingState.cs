using UnityEngine;

public enum FishingState
{
    Inactive, // Player isn't fishing
    Ready, // Rod equipped, ready to cast
    Charging, // Holding LMB to build casting power
    Casting, // Fly travelling towards water
    Waiting, // Fly landed, waiting for bite
    Bite, // Fish biting, set the hook
    Fighting, // Fish hooked, manage tension
    Landing // Fish caught -> inspection
}
