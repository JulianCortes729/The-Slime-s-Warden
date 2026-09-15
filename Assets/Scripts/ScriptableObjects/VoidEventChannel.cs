using System;
using UnityEngine;

[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Event Channels/Void Event Channel")]
public class VoidEventChannel : ScriptableObject
{
    public event Action OnRaiseEvent;


    public void RaiseEvent()
    {
        OnRaiseEvent?.Invoke();
    }


    // 📖 Los ScriptableObject son assets: sobreviven al Stop del Editor cuando
    // "Reload Domain" está desactivado. Sin este limpiado, en la segunda sesión
    // de Play el evento todavía apunta a los objetos de la sesión anterior
    // (ya destruidos) → MissingReferenceException fantasma, intermitente.
    private void OnDisable()
    {
        OnRaiseEvent = null;
    }
}
