using Cinemachine;
using UnityEngine;

/// <summary>
/// Capa de pilares de una zona con parallax vertical: sigue a la cámara una parte de su movimiento,
/// así se desplaza más despacio que el nivel y parece más lejana. Cada pilar solo se ve dentro de la
/// franja de su zona: se desvanece entero al acercarse a la frontera, así las zonas no se mezclan.
/// </summary>
public class ZoneParallaxLayer : MonoBehaviour
{
    [Tooltip("Cuánto sigue a la cámara: 0 = quieta como el nivel, 1 = pegada a la cámara (infinitamente lejos)")]
    [SerializeField, Range(0f, 1f)] private float followFactor = 0.3f;

    [Tooltip("Altura (y) de la cámara en la que la capa está donde se ve en el Editor")]
    [SerializeField] private float referenceCameraY;

    [Tooltip("Franja del nivel (y del mundo) donde se ven los pilares de esta zona")]
    [SerializeField] private float bandBottom;
    [SerializeField] private float bandTop;

    [Tooltip("Unidades antes de la frontera a lo largo de las cuales el pilar se desvanece")]
    [SerializeField] private float fadeDistance = 4f;

    private Vector3 _basePosition;
    private SpriteRenderer[] _pillars;
    private float[] _bottoms;    // borde inferior de cada pilar, respecto a la capa
    private float[] _tops;       // borde superior de cada pilar, respecto a la capa
    private float[] _baseAlphas; // transparencia de cada pilar en el Editor
    private float[] _alphas;     // la última aplicada, para no tocar el color si no cambia

    private void Awake()
    {
        _basePosition = transform.position;
        _pillars = GetComponentsInChildren<SpriteRenderer>(true);
        int count = _pillars.Length;
        _bottoms = new float[count];
        _tops = new float[count];
        _baseAlphas = new float[count];
        _alphas = new float[count];
        for (int i = 0; i < count; i++)
        {
            Bounds bounds = _pillars[i].bounds;
            _bottoms[i] = bounds.min.y - _basePosition.y;
            _tops[i] = bounds.max.y - _basePosition.y;
            _baseAlphas[i] = _pillars[i].color.a;
            _alphas[i] = -1f; // fuerza a aplicar el primer frame
        }
    }

    private void OnEnable()
    {
        // Se mueve justo después de que Cinemachine coloque la cámara, en el mismo frame:
        // si fuera un frame atrasada, temblaría
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
        // Solo vertical: el Confiner ya impide que la cámara se mueva en horizontal
        float cameraY = brain.OutputCamera.transform.position.y;
        Vector3 position = _basePosition;
        position.y += (cameraY - referenceCameraY) * followFactor;
        transform.position = position;

        float fade = Mathf.Max(fadeDistance, 0.01f);
        for (int i = 0; i < _pillars.Length; i++)
        {
            // Distancia del pilar a la frontera más cercana: 0 o menos = la toca o está fuera
            float gap = Mathf.Min(position.y + _bottoms[i] - bandBottom, bandTop - (position.y + _tops[i]));
            float alpha = _baseAlphas[i] * Mathf.Clamp01(gap / fade);
            if (alpha == _alphas[i]) continue;
            _alphas[i] = alpha;

            SpriteRenderer pillar = _pillars[i];
            pillar.enabled = alpha > 0f; // si es invisible, ni se dibuja
            Color color = pillar.color;
            color.a = alpha;
            pillar.color = color;
        }
    }
}
