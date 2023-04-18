using System;

namespace disguys.Gameplay.GameplayObjects
{
    [Serializable]
    public enum MovementStatus
    {
        Idle,         // not trying to move
        Walking,      // character should appear to be "walking" rather than normal running (e.g. for cut-scenes)
    }
}

