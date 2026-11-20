using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Sonidos de una pantalla con botones: un toque al cambiar de botón (teclado, mando o ratón)
/// y otro al pulsar. Va en el Canvas del menú y de la pantalla final, junto a SelectionKeeper.
/// Pide los sonidos por SFXCueChannel: no conoce al AudioManager.
/// </summary>
public class MenuSounds : MonoBehaviour
{
    [Tooltip("SFXCueChannel.asset: el AudioManager lo escucha.")]
    [SerializeField] private AudioCueChannel _sfxChannel;
    [Tooltip("Suena al pasar de un botón a otro. Cue_UI_Move.")]
    [SerializeField] private AudioCue _moveCue;
    [Tooltip("Suena al pulsar cualquier botón de este Canvas. Cue_UI_Press.")]
    [SerializeField] private AudioCue _pressCue;

    private UnityEngine.UI.Button[] _buttons;
    private GameObject _lastSelected;
    private int _pressFrame = -10;

    private void Awake()
    {
        // true: también los botones de paneles que empiezan ocultos (Cerrar, en Controles)
        _buttons = GetComponentsInChildren<UnityEngine.UI.Button>(true);
    }

    private void OnEnable()
    {
        foreach (UnityEngine.UI.Button button in _buttons)
            button.onClick.AddListener(OnButtonPressed);

        _lastSelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
    }

    private void OnDisable()
    {
        foreach (UnityEngine.UI.Button button in _buttons)
            if (button != null) button.onClick.RemoveListener(OnButtonPressed);
    }

    private void Update()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        GameObject current = eventSystem.currentSelectedGameObject;
        if (ShouldPlayMove(_lastSelected, current, Time.frameCount - _pressFrame))
            Play(_moveCue);

        // Si un clic en el vacío deja sin selección, se recuerda el último botón:
        // cuando SelectionKeeper lo recupera, es el mismo y no suena
        if (current != null) _lastSelected = current;
    }

    // El clic y el salto de foco que provoca pueden caer en el mismo frame o en el siguiente,
    // según quién corra antes ese frame: el EventSystem o este script
    private const int PressFocusFrames = 1;

    /// <summary>
    /// Regla del sonido de moverse: solo cuando el foco pasa de un botón a otro distinto.
    /// No suena con la primera selección de la pantalla, ni sin selección, ni cuando el foco salta
    /// por una pulsación (abrir o cerrar Controles): esa ya sonó como pulsación.
    /// </summary>
    public static bool ShouldPlayMove(GameObject previous, GameObject current, int framesSincePress)
    {
        return previous != null && current != null && previous != current
               && framesSincePress > PressFocusFrames;
    }

    private void OnButtonPressed()
    {
        _pressFrame = Time.frameCount;
        Play(_pressCue);
    }

    private void Play(AudioCue cue)
    {
        // El sonido lo reproduce el AudioManager (DontDestroyOnLoad): se oye entero aunque cambie la escena
        if (_sfxChannel != null && cue != null)
            _sfxChannel.RaiseEvent(cue, Vector3.zero);
    }
}
