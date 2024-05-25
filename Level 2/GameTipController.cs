using UnityEngine;

public class GameTipController : MonoBehaviour
{
    [SerializeField] private GameObject text;
    [SerializeField] private GameObject tooltip1;
    [SerializeField] private GameObject tooltip2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (text != null) text.SetActive(true);
        else
        {
            Destroy(tooltip1);
            Destroy(tooltip2);
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (text != null) text.SetActive(false);
    }
}
