using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private VoidEventChannel _restartGameEventChannel;
    [SerializeField] private VoidEventChannel _returnToMenuEventChannel;

    public void RestartGame()
    {
        if (_restartGameEventChannel != null)
        {
            _restartGameEventChannel.RaiseEvent();

        }
    }

    public void ReturnToMenu()
    {
        if (_returnToMenuEventChannel != null)
        {
            _returnToMenuEventChannel.RaiseEvent();
        }
    }
}

    

