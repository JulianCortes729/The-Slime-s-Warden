using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueListener : MonoBehaviour
{
    [SerializeField] private PlayerBlackboard _blackboard;
    
    [Header("Canales de Escucha")]
    [SerializeField] private DialogueEventChannel _dialogueChannel;
    [SerializeField] private VoidEventChannel _dialogueEndedChannel;

    private void OnEnable()
    {
        // "Nos suscribimos al canal. Si empieza un diálogo, congelamos."
        if (_dialogueChannel != null) 
            _dialogueChannel.OnDialogueRequested += LockPlayer;
            
        // "Si escuchamos el timbre de fin, descongelamos."
        if (_dialogueEndedChannel != null) 
            _dialogueEndedChannel.OnRaiseEvent += UnlockPlayer;
    }

    private void OnDisable()
    {
        //Siempre cancelamos suscripciones para evitar Memory Leaks
        if (_dialogueChannel != null) 
            _dialogueChannel.OnDialogueRequested -= LockPlayer;
            
        if (_dialogueEndedChannel != null) 
            _dialogueEndedChannel.OnRaiseEvent -= UnlockPlayer;
    }

    //Estos métodos simplemente actualizan el blackboard del jugador para reflejar si está hablando o no.
    private void LockPlayer(DialogueData data) => _blackboard.isTalking = true;
    private void UnlockPlayer() => _blackboard.isTalking = false;
}
