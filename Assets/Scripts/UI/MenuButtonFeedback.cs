using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Respuesta visual de un botón del menú. Cuando el botón está seleccionado
/// (ratón encima, o elegido con teclado/mando) el texto crece y se vuelve verde,
/// y Squishy aparece botando a su izquierda.
/// </summary>
// 📖 El ratón también "selecciona" al pasar por encima: así ratón y teclado comparten
// un único botón activo y nunca hay dos resaltados a la vez.
public class MenuButtonFeedback : MonoBehaviour,
    ISelectHandler, IDeselectHandler, IPointerEnterHandler,
    IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private RectTransform marker; // Squishy, a la izquierda del texto

    [Header("Colores del texto")]
    [SerializeField] private Color normalColor = new Color(0.949f, 0.969f, 0.882f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.706f, 0.945f, 0.639f, 1f);

    [Header("Escala del texto")]
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float scaleSpeed = 14f;

    [Header("Bote de Squishy")]
    [SerializeField] private float bobHeight = 6f;
    [SerializeField] private float bobSpeed = 7f;

    private bool _selected;
    private bool _pressed;
    private Vector2 _markerBasePosition;

    private void Awake()
    {
        if (marker != null) _markerBasePosition = marker.anchoredPosition;
        ApplyState();
        if (label != null) label.rectTransform.localScale = Vector3.one;
    }

    private void OnDisable()
    {
        // Al ocultarse (p. ej. el botón Cerrar con su panel) vuelve al estado normal
        _selected = false;
        _pressed = false;
        ApplyState();
        if (label != null) label.rectTransform.localScale = Vector3.one;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _selected = true;
        ApplyState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _selected = false;
        _pressed = false;
        ApplyState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData) => _pressed = true;

    public void OnPointerUp(PointerEventData eventData) => _pressed = false;

    public void OnSubmit(BaseEventData eventData)
    {
        // Enter o el botón A del mando: un "apretón" corto que luego vuelve solo
        if (label != null) label.rectTransform.localScale = Vector3.one * pressedScale;
    }

    private void Update()
    {
        if (label == null) return;

        // unscaledDeltaTime: el menú se sigue animando aunque timeScale sea 0 (pausa)
        float target = _pressed ? pressedScale : (_selected ? selectedScale : 1f);
        float t = 1f - Mathf.Exp(-scaleSpeed * Time.unscaledDeltaTime);
        float scale = Mathf.Lerp(label.rectTransform.localScale.x, target, t);
        label.rectTransform.localScale = new Vector3(scale, scale, 1f);

        if (_selected && marker != null)
        {
            float bob = Mathf.Abs(Mathf.Sin(Time.unscaledTime * bobSpeed)) * bobHeight;
            marker.anchoredPosition = _markerBasePosition + new Vector2(0f, bob);
        }
    }

    private void ApplyState()
    {
        if (label != null) label.color = _selected ? selectedColor : normalColor;
        if (marker != null) marker.gameObject.SetActive(_selected);
    }
}
