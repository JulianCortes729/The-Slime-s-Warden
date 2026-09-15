using UnityEngine;

public class CollectibleAmmo : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 1; // cuánta munición otorga este coleccionable

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (collision.TryGetComponent(out PlayerBlackboard blackboard))
        {
            blackboard.bulletCount += ammoAmount;
            gameObject.SetActive(false);
        }
    }
}
