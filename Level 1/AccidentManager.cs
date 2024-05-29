using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScenePersistence;

public class AccidentManager : MonoBehaviour
{
    [SerializeField] private EaselController easel;

    private List<AccidentController> readyAccidents = new List<AccidentController>();
    private List<AccidentController> onScreenAccidents = new List<AccidentController>();

    private void Awake()
    {
        if (levelDifficulty != LevelDifficulty.Easy) Destroy(gameObject);
        else foreach (AccidentController accident in GetComponentsInChildren<AccidentController>(true))
        {
            readyAccidents.Add(accident);
        }
    }

    public void accidentCleared(AccidentController accident)
    {
        if (onScreenAccidents.Count == 0) return;

        onScreenAccidents.Remove(accident);
        readyAccidents.Add((accident));
        easel.setSlow(onScreenAccidents.Count, true);
    }

    public void startAccidents()
    {
        StartCoroutine(randomAccidentTimer());
    }

    public void accidentAppeared()
    {
        if (onScreenAccidents.Count == 0) return;
        easel.setSlow(onScreenAccidents.Count, false);
    }

    public void stopAccidents()
    {
        StopAllCoroutines();
        foreach (AccidentController accident in onScreenAccidents)
        {
            accident.dismiss(false);
            readyAccidents.Add(accident);
        }
        onScreenAccidents.Clear();
        easel.setSlow(0, false);
    }

    private void showAccident()
    {
        int index = Random.Range(0, readyAccidents.Count);
        AccidentController accident = readyAccidents[index];
        onScreenAccidents.Add(accident);
        readyAccidents.RemoveAt(index);
        accident.show();
    }

    private IEnumerator randomAccidentTimer()
    {
        while (easel.isPainting())
        {
            yield return new WaitForSeconds(Random.Range(5, 10));
            if (readyAccidents.Count > 1)
            {
                showAccident();
            }
        }
    }
}