
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class DialogueUI : MonoBehaviour
{
    [Header("Canal de Comunicación")]
    [Tooltip("El 'Camarero' o 'Radio' que nos traerá los textos.")]
    [SerializeField] private DialogueEventChannel _dialogueChannel;

    [Tooltip("El timbre que tocaremos cuando no queden más textos.")]
    [SerializeField] private VoidEventChannel _dialogueEndedChannel;

    [Header("Referencias Visuales")]
    [Tooltip("El panel principal que encendemos y apagamos.")]
    [SerializeField] private GameObject _dialogueBox; 
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _dialogueText;

    //Guardamos temporalmente el diálogo que estamos leyendo
    private DialogueData _currentDialogue;
    private int _currentSentenceIndex = 0;

    // Las mismas acciones del jugador: si se cambian las teclas de saltar o disparar, el diálogo las sigue
    private PlayerInputActions _input;
    private int _startFrame = -1; // frame en que se abrió el diálogo

    private void Awake()
    {
        _input = new PlayerInputActions();
    }

    //Ciclo de vida de Unity.
    //OnEnable se ejecuta justo cuando este script o su GameObject se encienden.
    private void OnEnable()
    {
        //Nos suscribimos
        if (_dialogueChannel != null)
            _dialogueChannel.OnDialogueRequested += StartDialogue;
        _input.Player.Enable();
    }

    // OnDisable se ejecuta justo cuando el GameObject se apaga o se destruye.
    private void OnDisable()
    {
        //Cancelamos la suscripción antes de irnos para evitar Memory Leaks
        if (_dialogueChannel != null)
            _dialogueChannel.OnDialogueRequested -= StartDialogue;
        _input.Player.Disable();
    }

    private void OnDestroy()
    {
        _input.Dispose(); // libera el asset de acciones que crea "new PlayerInputActions()"
    }

    //Leemos el Input directo en la UI para avanzar.
    private void Update()
    {
        // Guard Clause: Si la cajita está invisible, la UI no debe gastar CPU buscando teclas.
        if (!_dialogueBox.activeSelf) return;

        // La tecla que se pulsó en el mismo frame en que se abrió (p. ej. saltando al entrar en el
        // disparador final) no cuenta: si no, la primera frase se saltaría sin poder leerla
        if (Time.frameCount == _startFrame) return;

        if (AdvancePressed())
        {
            DisplayNextSentence();
        }
    }

    // Pasamos de página con las teclas de saltar o disparar (hoy Espacio y Enter) o con clic.
    // El jugador no salta ni dispara: mientras habla, PlayerInput ignora esas teclas.
    private bool AdvancePressed()
    {
        if (_input.Player.Jump.WasPressedThisFrame() || _input.Player.Shoot.WasPressedThisFrame())
            return true;
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private void StartDialogue(DialogueData dialogueData)
    {
        _currentDialogue = dialogueData;
        _currentSentenceIndex = 0; //Nos aseguramos de empezar en la página 0
        _startFrame = Time.frameCount;

        _nameText.text = _currentDialogue.CharacterName;
        _dialogueBox.SetActive(true); //Hacemos visible el cuadro de diálogo

        DisplayNextSentence(); //Mostramos la primera oración
    }

    //Lo hacemos público para que el sistema de Input pueda llamarlo 
    //cuando el jugador presione el botón de "Siguiente" o "Aceptar".
    public void DisplayNextSentence()
    {
        // Verificamos si aún nos quedan páginas (oraciones) por leer
        if (_currentSentenceIndex < _currentDialogue.Sentences.Length)
        {
            _dialogueText.text = _currentDialogue.Sentences[_currentSentenceIndex];
            _currentSentenceIndex++; // Pasamos a la siguiente página para la próxima vez
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        _dialogueBox.SetActive(false); // Ocultamos la UI
        
        if(_dialogueEndedChannel != null)
            _dialogueEndedChannel.RaiseEvent();
    }
}