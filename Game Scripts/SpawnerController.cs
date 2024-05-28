using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using static GameValues;
using Random = UnityEngine.Random;

public class SpawnerController : MonoBehaviour
{
    // pointers
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float spawnRange;
    [SerializeField] private String posDescription;
    private ISpawnerHandler spawnerManager;
    protected ParticleSystem.EmissionModule emission;
    protected ParticleSystem.MainModule particlesMain;

    // overload values
    private readonly byte OVERLOAD_LIMIT = 200;
    private byte overloadMeter = 0;

    // spawning
    private byte toSpawn = 0;
    private int enemyHealth = Difficulty.BASE_HEALTH;
    private Coroutine spawnTimer;

    // visual
    protected double particleSpeed = 5;
    private float spawnerYOffset = 2.5f;

    private void Awake()
    {
        emission = particles.emission;
        particlesMain = particles.main;
        spawnerManager = GetComponentInParent<ISpawnerHandler>();
    }

    private void appear()
    {
        overloadMeter = 0;
        animator.enabled = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        emission.rateOverTimeMultiplier = 0;
        particleSpeed = 5;
        animator.SetTrigger(Trigger.ANIMATION_START);
    }

    public virtual bool engage(int avgTime, float avgInterval, int health)
    {
        if (GetComponent<SpriteRenderer>().enabled) return false;

        appear();

        enemyHealth = health;
        spawnTimer = StartCoroutine(spawnTimerRoutine(setEnemiesToSpawn(avgTime, avgInterval), false));
        return true;
    }

    public void spawnSpecificAmount(float interval, int health, byte enemyCount)
    {
        appear();
        enemyHealth = health;
        toSpawn = enemyCount;
        StartCoroutine(spawnTimerRoutine(interval, false));
    }

    public void engageIndefinitely(float interval, int health)
    {
        appear();
        enemyHealth = health;
        toSpawn = 1;
        StartCoroutine(spawnTimerRoutine(interval, true));
    }

    public void damage(byte amount)
    {
        if (overloadMeter == 0 && amount > 0)
        {
            particles.Play();
            emission.rateOverTimeMultiplier = 1;
            particlesMain.startSpeed = (ParticleSystem.MinMaxCurve)particleSpeed;
        }
        if (overloadMeter < OVERLOAD_LIMIT)
        {
            overloadMeter += amount;
            if (overloadMeter % (OVERLOAD_LIMIT / 10) == 0)
            {
                emission.rateOverTimeMultiplier += 1;
                particleSpeed += 0.5;
                particlesMain.startSpeed = (ParticleSystem.MinMaxCurve)particleSpeed;
            }
        }
        else
        {
            overload();
        }
    }

    public void stopSpawning()
    {
        toSpawn = 0;
    }

    public String getLocationDescription()
    {
        return posDescription;
    }

    protected virtual void overload()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
        if (spawnTimer != null) StopCoroutine(spawnTimer);

        if (toSpawn < 4)
        {
            spawnerManager.fakeSpawnedEnemies(2);
            StopAllCoroutines();
            disengage();
        }
        else
        {
            toSpawn = (byte)(toSpawn / 2);
            StartCoroutine(overloadTimerRoutine());
        }
    }

    protected void disengage()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
        animator.SetTrigger("destroyAnim"); // calls hide
    }

    // called by animator
    private void hide()
    {
        particles.Stop();
        GetComponent<SpriteRenderer>().enabled = false;
        if (spawnerManager != null) spawnerManager.stoppedSpawning(this);
    }

    private void spawnEnemy(SkellyController enemy)
    {
        if (enemy == null) return;

        float spawnX = Random.Range(transform.position.x - spawnRange, transform.position.x + spawnRange);
        float spawnY = Random.Range(transform.position.y - spawnRange, transform.position.y + spawnRange) - spawnerYOffset;
        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        enemy.spawn(spawnPos, enemyHealth);
    }

    private float setEnemiesToSpawn(int avgTime, float avgInterval)
    {
        int spawnTime = Random.Range(avgTime - 15, avgTime + 15);
        float interval = Random.Range(avgInterval - 0.5f, avgInterval + 0.5f);
        interval = (float)Math.Round(interval, 1);
        toSpawn = (byte)math.round(spawnTime / interval);
        float minutes = spawnTime / 60f; //Debug.Log("Spawning " + toSpawn + " enemies for " + minutes + " minutes every " + interval + " seconds.");
        return interval;
    }

    private IEnumerator overloadTimerRoutine()
    {
        yield return new WaitForSeconds(3);
        while (toSpawn > 0)
        {
            spawnEnemy(spawnerManager.getSkeleton(true));
            yield return new WaitForSeconds(0.3f);
        }
        disengage();
        yield break;
    }

    private IEnumerator spawnTimerRoutine(float interval, bool forever)
    {
        yield return new WaitForSeconds(interval);
        while (toSpawn > 0)
        {
            spawnEnemy(spawnerManager.getSkeleton(false));
            if (!forever) toSpawn--;
            yield return new WaitForSeconds(interval);
        }
        disengage();
        yield break;
    }
}