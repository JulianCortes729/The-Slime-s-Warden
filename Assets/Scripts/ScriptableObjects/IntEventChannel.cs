using UnityEngine;
using System;

[CreateAssetMenu(fileName = "IntEventChannel", menuName = "Event Channels/Int Event Channel")]
public class IntEventChannel : ScriptableObject
{
    public event Action<int> OnRaiseEvent;

    public void RaiseEvent(int value)
    {
        OnRaiseEvent?.Invoke(value);
    }


    // 📖 Mismo motivo que en VoidEventChannel: el asset sobrevive al Stop,
    // la lista de suscriptores no debe sobrevivir con él.
    private void OnDisable()
    {
        OnRaiseEvent = null;
    }
}
