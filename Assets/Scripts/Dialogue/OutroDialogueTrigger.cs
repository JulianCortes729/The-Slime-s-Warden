using UnityEngine;
using UnityEngine.SceneManagement; //para poder cambiar de escenas

public class OutroDialogueTrigger : MonoBehaviour
{
    [Header("Configuración Narrativa")]
    [Tooltip("El libreto final donde el slime encuentra a su familia.")]
    [SerializeField] private DialogueData _outroDialogue;
    [Tooltip("El nombre EXACTO de tu escena de fin de juego.")]
    [SerializeField] private string _gameOverSceneName = "GameOver";

    [Header("Canales de Comunicación")]
    [SerializeField] private DialogueEventChannel _dialogueChannel;
    [SerializeField] private VoidEventChannel _dialogueEndedChannel;

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

        // 4. El helicóptero de extracción (Cargamos la nueva escena)
        SceneManager.LoadScene(_gameOverSceneName);
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
