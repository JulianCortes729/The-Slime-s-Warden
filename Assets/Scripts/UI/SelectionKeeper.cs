using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Mantiene siempre un botón con foco: si un clic en el vacío quita la selección, recupera la última.
/// Así el teclado y el mando nunca se quedan sin botón activo. Va en cada pantalla con botones
/// (el menú y la pantalla final), junto al EventSystem con su First Selected.
/// </summary>
public class SelectionKeeper : MonoBehaviour
{
    private GameObject _lastSelected;

    private void Update()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        GameObject current = eventSystem.currentSelectedGameObject;
        if (current != null && current.activeInHierarchy)
            _lastSelected = current;
        else if (_lastSelected != null && _lastSelected.activeInHierarchy)
            eventSystem.SetSelectedGameObject(_lastSelected);
    }
}
