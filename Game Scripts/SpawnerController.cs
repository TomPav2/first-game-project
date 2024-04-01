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
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule particlesMain;

    private readonly byte overloadLimit = 200;
    private byte overloadMeter = 0;

    private byte toSpawn = 0;
    private int enemyHealth = Difficulty.baseHealth;
    private Coroutine spawnTimer;

    private double particleSpeed = 5;

    private float spawnerYOffset = 2.5f;

    private void Awake()
    {
        emission = particles.emission;
        particlesMain = particles.main;
    }

    // TODO remove
    /*private void FixedUpdate()
    {
        Vector2 center = new Vector2(transform.position.x, transform.position.y - spawnerYOffset);
        Vector2 topLeft = new Vector2(center.x - spawnRange, center.y + spawnRange);
        Vector2 topRight = new Vector2(center.x + spawnRange, center.y + spawnRange);
        Vector2 botLeft = new Vector2(center.x - spawnRange, center.y - spawnRange);
        Vector2 botRight = new Vector2(center.x + spawnRange, center.y - spawnRange);
        Debug.DrawLine(center, topLeft);
        Debug.DrawLine(center, topRight);
        Debug.DrawLine(center, botLeft);
        Debug.DrawLine(center, botRight);
    }*/

    public bool engage(int avgTime, float avgInterval, int health)
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

    private void overload()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
        if (spawnTimer != null) StopCoroutine(spawnTimer);

        if (toSpawn < 4) disengage();
        else
        {
            toSpawn = (byte)(toSpawn / 2);
            StartCoroutine(overloadTimerRoutine());
        }
    }

    private void disengage()
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
        spawnerManager.stoppedSpawning();
    }

    private void spawnEnemy(SkellyController enemy)
    {
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
        while (toSpawn > 0)
        {
            yield return new WaitForSeconds(0.2f);
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