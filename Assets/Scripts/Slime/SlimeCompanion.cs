using UnityEngine;

/// <summary>
/// Squishy, el compañero: sigue al jugador con SmoothDamp, en X a un lado según hacia dónde mira
/// y en Y flotando a la altura de sus pies (también cuando el jugador está en el aire).
/// </summary>
public class SlimeCompanion : MonoBehaviour
{
    [Header("Tracking")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothTimeX = 0.15f;
    [SerializeField] private float smoothTimeY = 0.1f;

    [Header("Posición respecto al jugador")]
    [Tooltip("Distancia en X. Positiva = delante del jugador, negativa = detrás; cambia de lado cuando el jugador gira.")]
    [SerializeField] private float xOffset = 0.5f;
    [Tooltip("Cuánto por debajo del pivot del jugador flota: la altura de sus pies.")]
    [SerializeField] private float playerFeetOffset = 0.8f;

    private float _currentVelocityX;
    private float _currentVelocityY;

    // En Update y no en FixedUpdate: el jugador tiene Interpolate, así que su posición
    // en Update ya es la suavizada que se dibuja y Squishy no tiembla.
    private void Update()
    {
        if (playerTransform == null) return; // 📖 sin esto, un prefab sin asignar tira NRE cada frame

        float facing = Mathf.Sign(playerTransform.localScale.x);
        Vector3 position = transform.position;

        position.x = Mathf.SmoothDamp(position.x, playerTransform.position.x + xOffset * facing,
            ref _currentVelocityX, smoothTimeX);
        position.y = Mathf.SmoothDamp(position.y, playerTransform.position.y - playerFeetOffset,
            ref _currentVelocityY, smoothTimeY);

        transform.position = position;
    }
}
