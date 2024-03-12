using System;

public static class GameValues
{
    public static class Tag
    {
        public static readonly String wall = "Wall";
        public static readonly String player = "Player";
        public static readonly String AttackLMB = "LMB Attack";
    }

    public enum EnemyState
    {
        None,
        Dead,
        Spawning,
        Idle,
        Walking,
        Following,
        InertiaWalk,
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
