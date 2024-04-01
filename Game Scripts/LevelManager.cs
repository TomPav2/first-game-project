using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject waypointContainer;
    [SerializeField] private LayerMask pathfindingLayers; // need to see enemies to find waypoints
    [SerializeField] private LayerMask trackPlayerLayers; // see player, not enemies
    [SerializeField] private Transform mainCharTransform;
    [SerializeField] private SpawnerManager spawnerManager;
    [SerializeField] private EaselController easel;

    private List<Area> areas = new List<Area>();
    private List<Waypoint> waypoints = new List<Waypoint>();
    private List<Stage> stages = new List<Stage>();
    private Area centralArea;

    private int score = 0;
    private int totalScore = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        // instantiate waypoints as structs rather than accessing them as gameobjects
        for (int i = 0; i < waypointContainer.transform.childCount; i++)
        {
            GameObject newWayPoint = waypointContainer.transform.GetChild(i).gameObject;
            byte newPriority = System.Convert.ToByte(Variables.Object(newWayPoint).Get("priority"));
            waypoints.Add(new Waypoint(newWayPoint.transform.position, newPriority));
        }
        initAreas();
        initStages();
    }

    private void Start()
    {
        setupNextStage();
    }

    // ------------ navigation ------------

    // If the enemy is in central area, give it a random spot to walk to
    // Otherwise find the highest priority target in line of sight (0 is highest)
    public Vector2 getWaypoint(Transform from, bool inCentralArea)
    {
        if (inCentralArea) return centralArea.getRandom();

        Waypoint? bestTarget = null;
        foreach (Waypoint waypoint in waypoints)
        {
            if (!bestTarget.HasValue)
            {
                if (checkLoS(from, waypoint.position)) bestTarget = waypoint;
            }
            else
            {
                if (waypoint.priority < bestTarget.Value.priority && checkLoS(from, waypoint.position))
                {
                    bestTarget = waypoint;
                }
            }
            if (bestTarget.HasValue && bestTarget.Value.priority == 0) return bestTarget.Value.getFuzzyPosition();
        }
        if (bestTarget.HasValue)
            return bestTarget.Value.getFuzzyPosition();
        else
        {
            Debug.LogError("Enemy can't see any waypoints from this position: " + from.position);
            return Vector2.zero;
        }
    }

    // Check if the enemy is visible from the waypoint
    private bool checkLoS(Transform enemy, Vector2 waypoint)
    {
        Vector2 enemyPos = enemy.position;
        Vector2 direction = enemyPos - waypoint;
        float distance = Vector2.Distance(enemyPos, waypoint);
        RaycastHit2D[] hits = Physics2D.RaycastAll(waypoint, direction, distance, pathfindingLayers);

        if (hits.Length == 0) return false;

        bool foundTarget = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag(Tag.wall)) return false;
            if (hit.collider.transform == enemy) foundTarget = true;
        }
        return foundTarget;
    }

    public bool charLoS(Vector2 from)
    {
        Vector2 charPos = mainCharTransform.position;
        Vector2 direction = charPos - from;
        if (Vector2.Distance(from, charPos) > 80) return false;
        RaycastHit2D hit = Physics2D.Raycast(from, direction, 80f, trackPlayerLayers);
        if (hit.collider != null && hit.collider.CompareTag(Tag.player)) return true;
        return false;
    }

    public Vector2 getCharPos()
    { return mainCharTransform.position; }

    // ------------ stage management ------------
    public void addScore(DamageType type)
    {
        switch (type)
        {
            case DamageType.Raven:
                score += 1;
                break;

            case DamageType.Crow:
                score += 3;
                break;

            case DamageType.None:
                break;

            case DamageType.Contact:
                break;

            default:
                score += 10;
                break;
        }
    }

    public void startStage()
    {
        spawnerManager.startStage(stages[0]);
    }

    private void setupNextStage()
    {
        easel.setUp(stages[0].bonusSpawn);
    }

    public void finishStage()
    {
        totalScore += stages[0].number * (1000 + score);
        stages.RemoveAt(0);
        score = 0;
        spawnerManager.endStage();
        Debug.Log("Score " + totalScore);
        // TODO disable bonus, show level end, add upgrade points

        setupNextStage();
    }

    // ------------ game data ------------
    private void initStages()
    {
        stages.Add(new Stage(1, 1, 0.5f, 3, 150, 0.75f));
    }

    private void initAreas()
    {
        centralArea = new Area(-42, -40, 66, 11);
        areas.Add(new Area(-108, 76, -84, 96));
        areas.Add(new Area(-100, -31, -91, 73));
        areas.Add(new Area(-90, 7, -32, 13));
        areas.Add(new Area(-111, -100, -94, -84));
        areas.Add(new Area(-111, -84, -108, -33));
        areas.Add(new Area(-108, -37, -33, -31));
        areas.Add(new Area(28, 120, 72, 140));
        areas.Add(new Area(60, 16, 68, 118));
        areas.Add(new Area(164, 64, 188, 92));
        areas.Add(new Area(64, 84, 166, 88));
        areas.Add(new Area(86, -36, 174, -31));
        areas.Add(new Area(151, -60, 160, -38));
        areas.Add(new Area(144, -80, 168, -60));
    }

    private struct Waypoint
    {
        public readonly Vector2 position { get; }
        public readonly byte priority { get; }

        public Waypoint(Vector2 position, byte priority)
        {
            this.position = position;
            this.priority = priority;
        }

        public Vector2 getFuzzyPosition()
        {
            float posX = UnityEngine.Random.Range(position.x - 2, position.x + 2);
            float posY = UnityEngine.Random.Range(position.y - 2, position.y + 2);
            return new Vector2(posX, posY);
        }
    }

    private struct Area
    {
        // bottom left
        private readonly float xA;

        private readonly float yA;

        // top right
        private readonly float xB;

        private readonly float yB;

        public Area(float xA, float yA, float xB, float yB)
        {
            this.xA = xA;
            this.yA = yA;
            this.xB = xB;
            this.yB = yB;
        }

        public bool isInside(Vector2 position)
        {
            if (position.x < xA || position.x > xB ||
                position.y < yA || position.y > yB)
                return false;
            return true;
        }

        public Vector2 getRandom()
        {
            float x = Random.Range(xA, xB);
            float y = Random.Range(yA, yB);
            return new Vector2(x, y);
        }
    }

    public record Stage
    {
        public byte number { get; }
        public byte maxSpawners { get; } // the amount of spawners that can be active at a time
        public float intensity { get; } // the fraction of time the player should spend fighting enemies
        public float spawnInterval { get; } // the average time between two enemies spawning (per spawner)
        public int spawnerLifetime { get; } // the average time a spawner should be active
        public float bonusSpawn { get; } // at what % completion should bonus spawn

        public Stage(byte number, byte spawners, float intensity, float spawnInterval, int spawnerLifetime, float bonusSpawn)
        {
            this.number = number;
            this.maxSpawners = spawners;
            this.intensity = intensity;
            this.spawnInterval = spawnInterval;
            this.spawnerLifetime = spawnerLifetime;
            this.bonusSpawn = bonusSpawn;
        }
    }
}