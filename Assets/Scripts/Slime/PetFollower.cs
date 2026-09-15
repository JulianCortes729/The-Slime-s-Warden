using UnityEngine;

/// <summary>
/// Hace que el objeto siga al jugador con suavizado, respetando el flip de sprite.
/// </summary>
public class PetFollower : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offset = new Vector3(-1f, 1f, 0f);

    [Header("Movement")]
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 _currentVelocity;

    private void Update()
    {
        // Invierte X del offset si el jugador mira a la izquierda (localScale.x = -1)
        Vector3 dynamicOffset = new Vector3(offset.x * playerTransform.localScale.x, offset.y, offset.z);
        Vector3 targetPosition = playerTransform.position + dynamicOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            smoothTime
        );
    }
}

