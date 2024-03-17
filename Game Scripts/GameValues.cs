using System;

public static class GameValues
{
    public static class Tag
    {
        public static readonly String wall = "Wall";
        public static readonly String player = "Player";
        public static readonly String AttackLMB = "LMB Attack";
        public static readonly String enemy = "Enemy";
        public static readonly String spawner = "Spawner";
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
        Crow
    }
}
