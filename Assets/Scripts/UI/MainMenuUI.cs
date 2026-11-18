using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private VoidEventChannel _startGameEvent;
    [SerializeField] private VoidEventChannel _quitGameEvent;
    [SerializeField] private GameObject _controlsPanel;

    [Header("Navegación con teclado y mando")]
    [SerializeField] private GameObject _controlsButton; // recibe el foco al cerrar el panel
    [SerializeField] private GameObject _closeButton;    // recibe el foco al abrir el panel

    // Que siempre haya un botón con foco lo hace SelectionKeeper (en el mismo Canvas)

    public void OnPlayButtonClicked()
    {
        if (_startGameEvent != null)
        {
            _startGameEvent.RaiseEvent();
        }
    }

    public void OnQuitButtonClicked()
    {
        if (_quitGameEvent != null)
        {
            _quitGameEvent.RaiseEvent();
        }
    }

    public void OnControlsButtonClicked()
    {
        bool open = !_controlsPanel.activeSelf;
        _controlsPanel.SetActive(open);
        Select(open ? _closeButton : _controlsButton);
    }

    public void OnBackButtonClicked()
    {
        _controlsPanel.SetActive(false);
        Select(_controlsButton);
    }

    private void Select(GameObject target)
    {
        if (target != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
