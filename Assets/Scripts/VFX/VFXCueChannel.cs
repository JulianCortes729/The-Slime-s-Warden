using System;
using UnityEngine;


/// <summary>
/// Canal de eventos para pedir un efecto visual puntual.
/// Desacopla a los emisores (Bullet, y lo que venga) del VFXManager:
/// ninguno de los dos lados conoce al otro, solo comparten este asset.
/// </summary>
// 🧩 Patrón: Observer vía ScriptableObject. Mismo patrón que AudioCueChannel.
[CreateAssetMenu(fileName = "VFXCueChannel", menuName = "Event Channels/VFX Cue Channel")]
public class VFXCueChannel : ScriptableObject
{
    // 📖 Viaja el TIPO de efecto, no el prefab. El emisor dice "quiero un
    // EnemyHit"; qué prefab representa a un EnemyHit es problema del VFXManager.
    // Si mañana cambiás el prefab de la explosión, Bullet ni se entera.
    public event Action<VFXType, Vector3> OnRaiseEvent;


    public void RaiseEvent(VFXType vfxType, Vector3 position)
    {
        OnRaiseEvent?.Invoke(vfxType, position);
    }

    private void OnDisable()
    {
        OnRaiseEvent = null;
    }
}
