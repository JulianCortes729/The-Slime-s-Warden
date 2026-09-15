using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Poné este componente en CADA escena. Al arrancar la escena, pide el track
    /// que le corresponde. Es la única pieza que sabe "qué música va acá".
    /// </summary>
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private MusicCueChannel musicChannel;

        [Tooltip("Dejalo vacío si esta escena debe quedarse en silencio.")]
        [SerializeField] private AudioCue trackForThisScene;

        [SerializeField] private float fadeDuration = 1.5f;

        // 📖 Start, NO Awake ni OnEnable. Unity ejecuta TODOS los Awake, después
        // TODOS los OnEnable, y recién después todos los Start. Como el MusicManager
        // se suscribe al canal en su OnEnable, usar Start acá garantiza que ya
        // haya alguien escuchando cuando levantamos el evento.
        private void Start()
        {
            if (musicChannel == null)
            {
                Debug.LogError($"[SceneMusic] Falta asignar el MusicCueChannel en '{name}'.", this);
                return;
            }

            // Si todavía no hay MusicManager (ej: apretaste Play directo en SampleScene),
            // esto simplemente no hace nada. Sin errores, sin crashes.
            musicChannel.RaiseEvent(trackForThisScene, fadeDuration);
        }
    }
}