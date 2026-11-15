using Cinemachine;
using UnityEngine;

/// <summary>
/// Color de fondo de cada zona del nivel (verde, roja y azul). Decide en qué zona está la cámara,
/// con un margen para que los saltos no la cambien, y funde el color en el tiempo.
/// Los pilares de cada zona están fijos dentro de su franja del nivel, así que no hace falta tocarlos.
/// </summary>
public class BackgroundZones : MonoBehaviour
{
    [System.Serializable]
    public struct Zone
    {
        public string name;       // solo para reconocerla en el Inspector
        public float startHeight; // altura (y) de la cámara donde empieza la zona
        public Color color;       // color de fondo de la cámara en esta zona
    }

    [Tooltip("Zonas ordenadas de abajo arriba")]
    [SerializeField] private Zone[] zones;

    [Tooltip("Unidades que la cámara debe pasar de la frontera para cambiar de zona: así un salto no la cambia y la devuelve")]
    [SerializeField] private float margin = 2f;

    [Tooltip("Segundos que dura el fundido de color entre una zona y otra")]
    [SerializeField] private float fadeDuration = 1.5f;

    private int _current = -1; // -1: la cámara aún no se ha colocado
    private float[] _weights;  // cuánto pesa el color de cada zona, de 0 a 1

    private void Awake()
    {
        _weights = new float[zones.Length];
    }

    private void OnEnable()
    {
        // Se actualiza justo después de que Cinemachine coloque la cámara
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
        Camera camera = brain.OutputCamera;
        float y = camera.transform.position.y;

        if (_current < 0)
        {
            // Primer frame (al empezar o al reintentar): sin fundido, directamente el color de la zona de la cámara
            _current = ZoneAt(y);
            for (int i = 0; i < zones.Length; i++) _weights[i] = i == _current ? 1f : 0f;
            Apply(camera);
            return;
        }

        _current = ZoneWithMargin(y);
        bool changed = false;
        float step = Time.deltaTime / Mathf.Max(fadeDuration, 0.01f); // 0 en el Inspector = cambio casi instantáneo, sin dividir entre 0
        for (int i = 0; i < zones.Length; i++)
        {
            float target = i == _current ? 1f : 0f;
            if (_weights[i] == target) continue;
            _weights[i] = Mathf.MoveTowards(_weights[i], target, step);
            changed = true;
        }
        if (changed) Apply(camera);
    }

    // La zona más alta cuyo inicio está por debajo de la cámara
    private int ZoneAt(float y)
    {
        int zone = 0;
        for (int i = 1; i < zones.Length; i++)
            if (y >= zones[i].startHeight) zone = i;
        return zone;
    }

    // Como ZoneAt, pero solo cambia de zona si la cámara pasa la frontera por más de "margin" unidades
    private int ZoneWithMargin(float y)
    {
        int zone = _current;
        while (zone + 1 < zones.Length && y > zones[zone + 1].startHeight + margin) zone++;
        while (zone > 0 && y < zones[zone].startHeight - margin) zone--;
        return zone;
    }

    private void Apply(Camera camera)
    {
        Color color = Color.clear;
        float total = 0f;
        for (int i = 0; i < zones.Length; i++)
        {
            color += zones[i].color * _weights[i];
            total += _weights[i];
        }
        if (total > 0f) camera.backgroundColor = color / total;
    }
}
