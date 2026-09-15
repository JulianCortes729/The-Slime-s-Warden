using UnityEngine;

public class LevelIntroDialogue : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private DialogueData _introDialogue;
    [SerializeField] private DialogueEventChannel _dialogueChannel;

    private void Start()
    {
        //Invocamos el diálogo del slime en el mismo frame que inicia el nivel.
        if (_introDialogue != null && _dialogueChannel != null)
        {
            _dialogueChannel.RaiseEvent(_introDialogue);
        }
    }
}