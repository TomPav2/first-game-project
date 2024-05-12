using System;

public static class GameValues
{
    public static class Tag
    {
        public static readonly String WALL = "Wall";
        public static readonly String WALL_TOP = "WallsTop";
        public static readonly String PLAYER = "Player";
        public static readonly String ATTACK_LMB = "LMB Attack";
        public static readonly String ENEMY = "Enemy";
        public static readonly String SPAWNER = "Spawner";
        public static readonly String CROW = "Crow";
        public static readonly String RAVEN = "Raven";
        public static readonly String BONUS = "Bonus";
        public static readonly String PENTAGRAM = "Pentagram";
        public static readonly String BARRIER = "Barrier";
        public static readonly String SHIELD = "Shield";
    }

    public static class Difficulty
    {
        public static readonly byte ESTIMATED_DPS = 45;
        public static readonly byte BASE_HEALTH = 100;
        public static readonly short BOSS_ENEMY_HEALTH = 500;
        public static readonly byte INTERVAL = 15;
        public static readonly byte RAISE_HEALTH_INTERVAL = 8; // raise every two minutes
        public static readonly byte RAISE_HEALTH_GRACE_PERIOD = 16; // don't raise for the first four minutes
        public static readonly byte MAX_ENEMIES = 200;
        public static readonly short PAINTING_TARGET = 1200;
    }

    public static class Trigger
    {
        public static readonly String FADE_IN = "fadeIn";
        public static readonly String FADE_OUT = "fadeOut";
        public static readonly String FADE_TEXT = "textFade";
        public static readonly String ANIMATION_START = "startAnimation";
        public static readonly String ANIMATION_STOP = "stopAnimation";
        public static readonly String WALK = "walk";
        public static readonly String RUN = "run";
        public static readonly String IDLE = "idle";
        public static readonly String DIE = "die";
        public static readonly String TURN_RIGHT = "turnRight";
    }

    public static class Settings
    {
        public static bool altFont = false;
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
        Contact,
        Spikes
    }

    public enum CauseOfLoss
    {
        None,
        Damage,
        Overrun
    }

    public static bool flipACoin()
    {
        return UnityEngine.Random.Range(0, 2) == 0;
    }
}