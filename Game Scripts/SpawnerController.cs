using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnerController : MonoBehaviour
{
    [SerializeField] private GameObject skellyPrefab;
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private GameManager manager;

    // TODO move this to spawn manager when it exists
    //private HashSet<GameObject> usedEnemies = new HashSet<GameObject>();
    //private HashSet<GameObject> availableEnemies = new HashSet<GameObject>();

    public void testSpawn()
    {
        GameObject newSkelly = Instantiate(skellyPrefab);
        SkellyController controller = newSkelly.GetComponent<SkellyController>();
        controller.init(mainCharReference, manager);
        controller.spawn(transform.position, 100);
    }

    private IEnumerator spawnTimer(byte amount, float interval)
    {
        // TODO
        yield break;
    }
}
