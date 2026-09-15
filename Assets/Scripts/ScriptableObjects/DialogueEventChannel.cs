using System;
using UnityEngine;

//Al igual que con los datos, necesitamos poder instanciar este canal
//como un archivo físico en nuestras carpetas de Unity.
[CreateAssetMenu(fileName = "DialogueEventChannel", menuName = "RiseWithoutSky/Events/Dialogue Channel")]
public class DialogueEventChannel : ScriptableObject
{
    //Action es un delegado de C#. Es, literalmente, un "Evento".
    //El <DialogueData> significa que este evento no viaja vacío; cuando se dispare,
    //transportará consigo el "libreto" (DialogueData) del personaje.
    public event Action<DialogueData> OnDialogueRequested;

    //Este método es el micrófono del locutor. Quien quiera iniciar un diálogo
    //(ej. el LevelManager o un Trigger), llamará a este método y le pasará el libreto.
    public void RaiseEvent(DialogueData dialogueToPlay)
    {
        //El operador '?.' (Null-conditional) es vital aquí.
        //Significa: "¿Hay alguien suscrito escuchando este evento? Si la respuesta es sí, dispáralo".
        //Si nadie está escuchando (ej. la UI está apagada), no hace nada y evita un crasheo.
        OnDialogueRequested?.Invoke(dialogueToPlay);
    }


    // 📖 Idem VoidEventChannel. Sin esto, el LockPlayer del PlayerDialogueListener
    // de la sesión anterior sigue enganchado y te congela un jugador que ya no existe.
    private void OnDisable()
    {
        OnDialogueRequested = null;
    }
}