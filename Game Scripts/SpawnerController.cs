using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using static GameValues;
using Random = UnityEngine.Random;

public class SpawnerController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpawnerManager spawnerManager;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float spawnRange;
    protected ParticleSystem.EmissionModule emission;
    protected ParticleSystem.MainModule particlesMain;

    private readonly byte overloadLimit = 200;
    private byte overloadMeter = 0;

    private byte toSpawn = 0;
    private int enemyHealth = Difficulty.baseHealth;
    private Coroutine spawnTimer;

    protected double particleSpeed = 5;

    private float spawnerYOffset = 2.5f;

    private void Awake()
    {
        emission = particles.emission;
        particlesMain = particles.main;
    }

    public virtual bool engage(int avgTime, float avgInterval, int health)
    {
        if (GetComponent<SpriteRenderer>().enabled) return false;

        overloadMeter = 0;
        GetComponent<SpriteRenderer>().enabled = true;
        animator.enabled = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        emission.rateOverTimeMultiplier = 0;
        particleSpeed = 5;

        enemyHealth = health;
        spawnTimer = StartCoroutine(spawnTimerRoutine(setEnemiesToSpawn(avgTime, avgInterval)));
        return true;
    }

    public void damage(byte amount)
    {
        if (overloadMeter == 0 && amount > 0)
        {
            particles.Play();
            emission.rateOverTimeMultiplier = 1;
            particlesMain.startSpeed = (ParticleSystem.MinMaxCurve)particleSpeed;
        }
        if (overloadMeter < overloadLimit)
        {
            overloadMeter += amount;
            if (overloadMeter % (overloadLimit/10) == 0)
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
        if (toSpawn > 0)
        {
            toSpawn = 0;
            disengage();
        }
    }

    protected virtual void overload()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
        if (spawnTimer != null) StopCoroutine(spawnTimer);

        if (toSpawn < 4)
        {
            spawnerManager.fakeSpawnedEnemies(2);
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
        animator.enabled = false;
        particles.Stop();
        GetComponent<SpriteRenderer>().enabled = false;
        if (spawnerManager != null) spawnerManager.stoppedSpawning();
    }

    private void spawnEnemy(SkellyController enemy)
    {
        if (enemy == null) return; // this is to prevent unpredictable behaviour, more explanation in SpawnerManager.getSkeleton()

        float spawnX = Random.Range(transform.position.x - spawnRange, transform.position.x + spawnRange);
        float spawnY = Random.Range(transform.position.y - spawnRange, transform.position.y + spawnRange) - spawnerYOffset;
        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        enemy.spawn(spawnPos, enemyHealth);
        toSpawn--;
    }

    private float setEnemiesToSpawn(int avgTime, float avgInterval)
    {
        int spawnTime = Random.Range(avgTime - 30, avgTime + 30);
        float interval = Random.Range(avgInterval - 0.5f, avgInterval + 0.5f);
        interval = (float) Math.Round(interval, 1);
        toSpawn = (byte) math.round(spawnTime / interval);
        float minutes = spawnTime / 60f; Debug.Log("Spawning " + toSpawn + " enemies for " + minutes + " minutes every " + interval + " seconds.");
        return interval;
    }

    private IEnumerator overloadTimerRoutine()
    {
        yield return new WaitForSeconds(3);
        while (toSpawn > 0)
        {
            yield return new WaitForSeconds(0.3f);
            spawnEnemy(spawnerManager.getSkeleton(true));
        }
        yield return new WaitForSeconds(1);
        disengage();
        yield break;
    }

    private IEnumerator spawnTimerRoutine(float interval)
    {
        while (toSpawn > 0)
        {
            yield return new WaitForSeconds(interval);
            spawnEnemy(spawnerManager.getSkeleton(false));
        }
        yield return new WaitForSeconds(1);
        disengage();
        yield break;
    }
}