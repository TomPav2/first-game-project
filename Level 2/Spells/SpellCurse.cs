using System.Collections;
using UnityEngine;
using static ScenePersistence;
using static VectorUtil;

public class SpellCurse : Spell
{
    [SerializeField] GameObject startObject;
    [SerializeField] GameObject indicator;

    private static readonly float SPEED = 15;
    private Vector3 defaultPos;
    private bool moving = false;

    private void Awake()
    {
        channeled = true;
        manacost = 3;
    }

    private void Start()
    {
        defaultPos = indicator.transform.position;
    }

    public override byte channel()
    {
        return manacost;
    }

    public override void startChanneling()
    {
    }

    public override void stopChanneling()
    {
        invertControls = false;
        indicator.SetActive(false);
    }
    public override void cast(Vector2 startPos)
    {
        // channeled spell
    }

    public void startCurse()
    {
        invertControls = true;
        StartCoroutine(introRoutine());
    }
    
    private IEnumerator introRoutine()
    {
        defaultPos = indicator.transform.position;
        indicator.transform.position = startObject.transform.position;
        (Vector3 movement, float duration) = calculateMovement(indicator.transform.position, defaultPos, SPEED);
        indicator.SetActive(true);
        moving = true;
        StartCoroutine(moveTimer(duration));
        while (moving)
        {
            if (!isPaused) indicator.transform.position += movement * Time.deltaTime;
            yield return null;
        }
        indicator.transform.position = defaultPos;
        yield break;
    }

    private IEnumerator moveTimer(float time)
    {
        yield return new WaitForSeconds(time);
        moving = false;
        yield break;
    }
}
