using System;

public static class GameValues
{
    public static class Tag
    {
        public static readonly String wall = "Wall";
        public static readonly String player = "Player";
        public static readonly String attackLMB = "LMB Attack";
        public static readonly String enemy = "Enemy";
        public static readonly String spawner = "Spawner";
        public static readonly String crow = "Crow";
        public static readonly String raven = "Raven";
        public static readonly String bonus = "Bonus";
    }

    public static class Difficulty
    {
        public static readonly byte estimatedDPS = 35;
        public static readonly byte baseHealth = 100;
        public static readonly byte raiseHealthInterval = 4; // interval is 30 seconds
        public static readonly byte raiseHealthGracePeriod = 8;
    }

    public enum EnemyState
    {
        None,
        Dead,
        Spawning,
        Idle,
        Walking,
        Following,
        InertiaRun,
        InertiaStand,
        Dying
    }

    public enum DamageType
    {
        None,
        LMB,
        RMB,
        Raven,
        Crow,
        Contact
    }
}
