using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannel _gameOverEventChannel;
    [SerializeField] private VoidEventChannel _restartGameEventChannel;
    [SerializeField] private VoidEventChannel _returnToMenuEventChannel;
    [SerializeField] private VoidEventChannel _startGameEventChannel;
    [SerializeField] private VoidEventChannel _quitGameEventChannel;

    [SerializeField] private string _gameSceneName = "SampleScene";
    [SerializeField] private string _menuSceneName = "SceneMenu";
    [SerializeField] private string _gameOverSceneName = "SceneGameOver";

    private bool _isLoading = false;
    private static GameManager _instance;
    private void Awake()
    {
        if(_instance != null && _instance != this){
            enabled = false;
            Destroy(gameObject);
            return;
        }
        _instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _gameOverEventChannel.OnRaiseEvent += OnGameOver;
        _restartGameEventChannel.OnRaiseEvent += OnRestartGame;
        _returnToMenuEventChannel.OnRaiseEvent += OnReturnToMenu;
        _startGameEventChannel.OnRaiseEvent += OnStartGame;
        _quitGameEventChannel.OnRaiseEvent += OnQuitGame;
    }

    private void OnDisable()
    {
        _gameOverEventChannel.OnRaiseEvent -= OnGameOver;
        _restartGameEventChannel.OnRaiseEvent -= OnRestartGame;
        _returnToMenuEventChannel.OnRaiseEvent -= OnReturnToMenu;
        _startGameEventChannel.OnRaiseEvent -= OnStartGame;
        _quitGameEventChannel.OnRaiseEvent -= OnQuitGame;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void OnGameOver()
    {
        if(_isLoading) return;
        StartCoroutine(LoadSceneAsynchronously(_gameOverSceneName));
    }

    private void OnRestartGame()
    {
        if(_isLoading) return;
        StartCoroutine(LoadSceneAsynchronously(_gameSceneName));
    }

    private void OnReturnToMenu()
    {
        if(_isLoading) return;
        StartCoroutine(LoadSceneAsynchronously(_menuSceneName));
        
    }

    private void OnStartGame()
    {
        if(_isLoading) return;
        StartCoroutine(LoadSceneAsynchronously(_gameSceneName));
    }

    private void OnQuitGame()
    {
        #if UNITY_EDITOR
            // Stops Play Mode in the Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            //Closes the actual built application
            Application.Quit();
        #endif
    }

    private IEnumerator LoadSceneAsynchronously(string sceneName){
        _isLoading = true;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while(!operation.isDone){
            yield return null;
        }
        _isLoading = false;

    }

}
