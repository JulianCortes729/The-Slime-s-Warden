using System;
using UnityEngine;


    /// <summary>
    /// Canal de eventos para pedir un cambio de música.
    /// Quien pide NO conoce al MusicManager, y el MusicManager NO conoce a quien pide.
    /// </summary>
    // 🧩 Patrón: Observer vía ScriptableObject (mismo patrón que tus VoidEventChannel).
    [CreateAssetMenu(fileName = "MusicCueChannel", menuName = "Event Channels/Music Cue Channel")]
    public class MusicCueChannel : ScriptableObject
    {
        /// <summary>cue == null significa "fundí a silencio".</summary>
        public event Action<AudioCue, float> OnRaiseEvent;

        public void RaiseEvent(AudioCue cue, float fadeDuration)
        {
            // 📖 El ?. antes de Invoke evita NullReference si nadie está suscrito todavía.
            OnRaiseEvent?.Invoke(cue, fadeDuration);
        }

        // 📖 FIX del hallazgo #8: los ScriptableObject sobreviven al Stop en el Editor
        // cuando tenés "Reload Domain" desactivado. Si no limpiamos acá, en la segunda
        // sesión de Play el evento todavía apunta al MusicManager de la sesión anterior
        // (ya destruido) → MissingReferenceException fantasma, dificilísimo de debuggear.
        private void OnDisable()
        {
            OnRaiseEvent = null;
        }
    }
