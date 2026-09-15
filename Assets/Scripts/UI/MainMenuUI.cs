using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private VoidEventChannel _startGameEvent;
    [SerializeField] private VoidEventChannel _quitGameEvent;
    [SerializeField] private GameObject _controlsPanel;
    
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
        _controlsPanel.SetActive(!_controlsPanel.activeSelf);
    }

    public void OnBackButtonClicked()
    {
        _controlsPanel.SetActive(false);
    }
}
