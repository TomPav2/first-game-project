using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using static SceneLoader;

public class Level1Manager : LevelManager
{
    [SerializeField] private GameObject waypointContainer;
    [SerializeField] private SpawnerManager spawnerManager;
    [SerializeField] private EaselController easel;
    [SerializeField] private GameObject pentagram;

    private List<Area> areas = new List<Area>();
    private List<Waypoint> waypoints = new List<Waypoint>();
    private List<Stage> stages = new List<Stage>();
    private Area centralArea;

    private int score = 0;
    private int totalScore = 0;

    private void Awake()
    {
        if (levelDifficulty != LevelDifficulty.Easy) gameObject.SetActive(false);
        Application.targetFrameRate = 60;
        Time.timeScale = 1;

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
        isPaused = false;
        if (!inTutorial) setupNextStage();
    }

    // ------------ game control ------------

    public override void endScreen(CauseOfLoss cause)
    {
        hudController.lostGameMenu(cause, score);
    }

    public override void playerDied()
    {
        spawnerManager.endStage();
        base.playerDied();
    }

    public void tooManyEnemies()
    {
        spawnerManager.endStage();
        gameLost(CauseOfLoss.Overrun);
    }

    // ------------ navigation ------------

    // If the enemy is in central area, give it a random spot to walk to
    // Otherwise find the highest priority target in line of sight (0 is highest)
    // pos: the position the enemy will move toward
    // doNotWait: enemy will request a new target immediately after reaching it
    // (this is to prevent enemies from piling up when entering the central area)
    public override (Vector2 pos, bool doNotWait) getWaypoint(Transform from, bool inCentralArea)
    {
        if (inCentralArea) return (centralArea.getRandom(), false);

        Waypoint? bestTarget = null;
        foreach (Waypoint waypoint in waypoints)
        {
            if (!bestTarget.HasValue)
            {
                if (checkLoS(from, waypoint.position)) bestTarget = waypoint;
            }
            else
            {
                if ((waypoint.priority < bestTarget.Value.priority) && (checkLoS(from, waypoint.position)))
                {
                    bestTarget = waypoint;
                }
                else if ((waypoint.priority == bestTarget.Value.priority)
                    && Vector2.Distance(waypoint.position, from.position) < Vector2.Distance(bestTarget.Value.position, from.position)
                    && checkLoS(from, waypoint.position))
                {
                    bestTarget = waypoint;
                }
            }
        }
        if (bestTarget.HasValue)
            return (bestTarget.Value.getFuzzyPosition(), true);
        else
        {
            Debug.LogError("Enemy can't see any waypoints from this position: " + from.position);
            return (Vector2.zero, false);
        }
    }

    // ------------ bonus item ------------

    public void showBonusItem()
    {
        Area randomArea = areas[Random.Range(0, areas.Count)];
        bonusItem.transform.position = randomArea.getRandom();
        bonusItem.SetActive(true);
        hudController.popUp("Bonus item spawned", "Find it before finishing the level", null);
    }

    // ------------ stage management ------------
    public override void addScore(DamageType type)
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

    public override void setupNextStage()
    {
        lockControls = false;
        easel.setUp(stages[0].bonusSpawn);
    }

    public void finishStage()
    {
        if (stages.Count > 1)
        {
            totalScore += stages[0].number * (1000 + score);
            score = 0;
            spawnerManager.endStage();
            hideBonusItem();
            stages.RemoveAt(0);
            mainCharacter.heal(4);
            mainCharacter.rechargeLaser();
            hudController.showUpgradeScreen();
        }
        else ending();
    }

    public void ending()
    {
        hudController.popUp("Final painting complete!", "Stand in the pentagram when you're ready", null); // TODO actual endgame screen
        easel.gameObject.SetActive(false);
        pentagram.SetActive(true);
    }

    public override void enteredPentagram()
    {
        hudController.wonGameMenu("Score: " + totalScore.ToString());
    }

    // ------------ game data ------------
    private void initStages()
    {
        Stage stage1 = new Stage().setNumber(1).setMaxSpawners(1).setIntensity(0.5f).setSlowSpawnerInterval(4)
            .setSlowSpawnerLifetime(150).setFastSpawnerInterval(1.5f).setFastSpawnerLifetime(50).setBonusSpawn(0.75f);
        stages.Add(stage1);

        Stage stage2 = new Stage().setNumber(2).setMaxSpawners(1).setIntensity(0.8f).setSlowSpawnerInterval(3.5f)
            .setSlowSpawnerLifetime(180).setFastSpawnerInterval(0.8f).setFastSpawnerLifetime(40).setBonusSpawn(0.5f);
        stages.Add(stage2);

        Stage stage3 = new Stage().setNumber(3).setMaxSpawners(2).setIntensity(0.95f).setSlowSpawnerInterval(3)
            .setSlowSpawnerLifetime(180).setFastSpawnerInterval(2.0f).setFastSpawnerLifetime(90).setBonusSpawn(0.25f);
        stages.Add(stage3);

        Stage stage4 = new Stage().setNumber(4).setMaxSpawners(3).setIntensity(1.3f).setSlowSpawnerInterval(4)
            .setSlowSpawnerLifetime(300).setFastSpawnerInterval(1.5f).setFastSpawnerLifetime(60).setBonusSpawn(1.1f);
        stages.Add(stage4);
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

    public struct Waypoint
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
            float posX = Random.Range(position.x - 2, position.x + 2);
            float posY = Random.Range(position.y - 2, position.y + 2);
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
        public byte number { get; private set; }
        public byte maxSpawners { get; private set; } // the amount of spawners that can be active at a time
        public float intensity { get; private set; } // the fraction of time the player should spend fighting enemies
        public float slowSpawnerInterval { get; private set; } // the average time between two enemies spawning (per spawner)
        public int slowSpawnerLifetime { get; private set; } // the average time a spawner should be active
        public float fastSpawnerInterval { get; private set; } // the average time between two enemies spawning (per spawner)
        public int fastSpawnerLifetime { get; private set; } // the average time a spawner should be active
        public float bonusSpawn { get; private set; } // at what % completion should bonus spawn

        public Stage setNumber(byte number)
        {
            this.number = number;
            return this;
        }

        public Stage setMaxSpawners(byte maxSpawners)
        {
            this.maxSpawners = maxSpawners;
            return this;
        }

        public Stage setIntensity(float intensity)
        {
            this.intensity = intensity;
            return this;
        }

        public Stage setSlowSpawnerInterval(float slowSpawnerInterval)
        {
            this.slowSpawnerInterval = slowSpawnerInterval;
            return this;
        }

        public Stage setSlowSpawnerLifetime(int slowSpawnerLifetime)
        {
            this.slowSpawnerLifetime = slowSpawnerLifetime;
            return this;
        }

        public Stage setFastSpawnerInterval(float fastSpawnerInterval)
        {
            this.fastSpawnerInterval = fastSpawnerInterval;
            return this;
        }

        public Stage setFastSpawnerLifetime(int fastSpawnerLifetime)
        {
            this.fastSpawnerLifetime = fastSpawnerLifetime;
            return this;
        }

        public Stage setBonusSpawn(float bonusSpawn)
        {
            this.bonusSpawn = bonusSpawn;
            return this;
        }
    }



    public override void restrictedAccess()
    {
        // none in easy level
    }
}
