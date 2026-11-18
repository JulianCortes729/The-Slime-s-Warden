using UnityEngine;

public class OutroDialogueTrigger : MonoBehaviour
{
    [Header("Configuración Narrativa")]
    [Tooltip("El libreto final donde el slime encuentra a su familia.")]
    [SerializeField] private DialogueData _outroDialogue;

    [Header("Canales de Comunicación")]
    [SerializeField] private DialogueEventChannel _dialogueChannel;
    [SerializeField] private VoidEventChannel _dialogueEndedChannel;
    [Tooltip("Nivel completado (LevelCompletedChannel): lo escucha el GameManager, que carga la pantalla final en su variante de victoria.")]
    [SerializeField] private VoidEventChannel _levelCompletedChannel;

    //Evita que el jugador dispare esto 2 veces si entra y sale rápido
    private bool _hasTriggered = false; 

    //Se ejecuta mágicamente cuando un Rigidbody entra en este colisionador (Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Guard Clause: Si ya se activó, o si el que entró no es el Player (ej. fue un enemigo), abortamos.
        if (_hasTriggered || !other.CompareTag("Player")) return;

        _hasTriggered = true; // Sellamos la puerta para que no vuelva a ocurrir

        // 1. El soldado enciende el radio (Suscripción efímera)
        if (_dialogueEndedChannel != null)
        {
            _dialogueEndedChannel.OnRaiseEvent += OnOutroDialogueEnded;
        }

        // 2. Le pedimos a la UI que muestre el diálogo final
        if (_dialogueChannel != null && _outroDialogue != null)
        {
            _dialogueChannel.RaiseEvent(_outroDialogue);
        }
    }

    //Este método SOLO se ejecutará cuando la UI toque la campana de fin de diálogo
    private void OnOutroDialogueEnded()
    {
        // 3. El soldado tira el radio a la basura (Desuscripción inmediata)
        if (_dialogueEndedChannel != null)
        {
            _dialogueEndedChannel.OnRaiseEvent -= OnOutroDialogueEnded;
        }

        // 4. El helicóptero de extracción: pedimos la escena final al GameManager, el único que carga
        // escenas (en segundo plano y sin cargas dobles). Sin GameManager (Play directo en SampleScene) no pasa nada.
        if (_levelCompletedChannel != null)
            _levelCompletedChannel.RaiseEvent();
        else
            Debug.LogError("[OutroDialogueTrigger] Falta asignar el canal de nivel completado (LevelCompletedChannel).", this);
    }

    //Medida de seguridad: Si por algún motivo externo este Trigger
    //es destruido mientras el jugador está leyendo, evitamos un Memory Leak.
    private void OnDisable()
    {
        if (_hasTriggered && _dialogueEndedChannel != null)
        {
            _dialogueEndedChannel.OnRaiseEvent -= OnOutroDialogueEnded;
        }
    }
}
