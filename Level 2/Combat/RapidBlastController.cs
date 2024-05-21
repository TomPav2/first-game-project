using System.Collections;
using UnityEngine;

public class RapidBlastController : ShooterController
{
    [SerializeField] private GameObject mainChar;
    
    private static readonly Vector3 START_OFFSET = new Vector3(0, 6f, 0);
    private static readonly int TRIGGER_DISTANCE = 20;

    private Vector3 startPos;
    private Coroutine shooting;

    private void Start()
    {
        startPos = transform.position + START_OFFSET;
    }

    public void bossDied()
    {
        if (shooting != null) StopCoroutine(shooting);
    }

    public void startShooting()
    {
        shooting = StartCoroutine(shootingRoutine());
    }

    private IEnumerator shootingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(6.2f);
            if (Vector2.Distance(startPos, mainChar.transform.position) > TRIGGER_DISTANCE) continue;
            byte count = 0;
            float lifetime = 0.05f;
            while (count < 30)
            {
                getProjectile().shootAtTarget(startPos, mainChar.transform.position, lifetime);
                count++;
                lifetime += 0.04f;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
