using UnityEngine;

/// <summary>
/// Hace flotar un elemento de UI arriba y abajo, suave y en bucle
/// (el título del menú y Squishy encima de él).
/// </summary>
public class UIFloat : MonoBehaviour
{
    [SerializeField] private float amplitude = 6f; // píxeles de subida y bajada
    [SerializeField] private float speed = 2f;     // radianes por segundo (2 ≈ un ciclo cada 3 s)
    [SerializeField] private float phase = 0f;     // desfase, para que dos elementos no vayan sincronizados

    private RectTransform _rect;
    private Vector2 _basePosition;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _basePosition = _rect.anchoredPosition;
    }

    private void OnDisable()
    {
        // Vuelve a su sitio para que el Editor no guarde una posición a medio flotar
        if (_rect != null) _rect.anchoredPosition = _basePosition;
    }

    private void Update()
    {
        // unscaledTime: sigue flotando aunque timeScale sea 0 (pausa)
        float offset = Mathf.Sin(Time.unscaledTime * speed + phase) * amplitude;
        _rect.anchoredPosition = _basePosition + new Vector2(0f, offset);
    }
}
