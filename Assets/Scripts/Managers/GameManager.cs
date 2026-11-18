using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("Muerte del jugador (DeathChannel): se carga la pantalla final en su variante de derrota")]
    [SerializeField] private VoidEventChannel _gameOverEventChannel;
    [Tooltip("Fin del diálogo final (LevelCompletedChannel): se carga la pantalla final en su variante de victoria")]
    [SerializeField] private VoidEventChannel _levelCompletedEventChannel;
    [Tooltip("Dónde se anota cómo terminó la partida; la pantalla final lo lee")]
    [SerializeField] private GameResult _gameResult;
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
        if (_levelCompletedEventChannel != null) _levelCompletedEventChannel.OnRaiseEvent += OnLevelCompleted;
        _restartGameEventChannel.OnRaiseEvent += OnRestartGame;
        _returnToMenuEventChannel.OnRaiseEvent += OnReturnToMenu;
        _startGameEventChannel.OnRaiseEvent += OnStartGame;
        _quitGameEventChannel.OnRaiseEvent += OnQuitGame;
    }

    private void OnDisable()
    {
        _gameOverEventChannel.OnRaiseEvent -= OnGameOver;
        if (_levelCompletedEventChannel != null) _levelCompletedEventChannel.OnRaiseEvent -= OnLevelCompleted;
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
        RecordOutcome(GameOutcome.Defeat);
        StartCoroutine(LoadSceneAsynchronously(_gameOverSceneName));
    }

    private void OnLevelCompleted()
    {
        if(_isLoading) return;
        RecordOutcome(GameOutcome.Victory);
        StartCoroutine(LoadSceneAsynchronously(_gameOverSceneName));
    }

    // Se anota ANTES de cargar: la pantalla final lo lee en su Awake
    private void RecordOutcome(GameOutcome outcome)
    {
        if (_gameResult != null)
            _gameResult.Record(outcome);
        else
            Debug.LogError("[GameManager] Falta asignar GameResult: la pantalla final mostrará siempre la derrota.", this);
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
