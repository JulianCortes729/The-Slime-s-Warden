
using TMPro;
using UnityEngine;


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

    //Ciclo de vida de Unity. 
    //OnEnable se ejecuta justo cuando este script o su GameObject se encienden.
    private void OnEnable()
    {
        //Nos suscribimos
        if (_dialogueChannel != null)
            _dialogueChannel.OnDialogueRequested += StartDialogue;
    }

    // OnDisable se ejecuta justo cuando el GameObject se apaga o se destruye.
    private void OnDisable()
    {
        //Cancelamos la suscripción antes de irnos para evitar Memory Leaks
        if (_dialogueChannel != null)
            _dialogueChannel.OnDialogueRequested -= StartDialogue;
    }

    //Leemos el Input directo en la UI para avanzar.
    private void Update()
    {
        // Guard Clause: Si la cajita está invisible, la UI no debe gastar CPU buscando teclas.
        if (!_dialogueBox.activeSelf) return; 

        // Si pulsas la tecla 'E' o 'Enter', pasamos de página
        if (Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }

    private void StartDialogue(DialogueData dialogueData)
    {
        _currentDialogue = dialogueData;
        _currentSentenceIndex = 0; //Nos aseguramos de empezar en la página 0

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