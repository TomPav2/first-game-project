using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SpawnerController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject skellyPrefab;
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private GameManager manager;

    private readonly byte overloadLimit = 200;
    private byte overload = 0;

    // TODO move this to spawn manager when it exists
    //private HashSet<GameObject> usedEnemies = new HashSet<GameObject>();
    //private HashSet<GameObject> availableEnemies = new HashSet<GameObject>();

    public void testSpawn()
    {
        // TODO remove
        GameObject newSkelly = Instantiate(skellyPrefab);
        SkellyController controller = newSkelly.GetComponent<SkellyController>();
        controller.init(mainCharReference, manager);
        controller.spawn(transform.position, 100);
    }

    public short engage(float intensity)
    {
        // TODO

        return 0; // return the time this is expected to run
    }

    private void hide()
    {
        animator.enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void disengage()
    {
        GetComponent<CapsuleCollider2D>().enabled = false;
        animator.SetTrigger("destroyAnim");
    }

    public void damage(byte amount)
    {
        if (overload < overloadLimit)
        {
            overload += amount;
        } else
        {
            overload = 0;
            disengage();
        }
    }

    private IEnumerator spawnTimer(byte amount, float interval)
    {
        // TODO
        yield break;
    }
}
