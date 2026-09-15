
using UnityEngine;
using UnityEngine.UI;


public class LifeUI : MonoBehaviour
{
    [SerializeField] private IntEventChannel _playerHealthChannel;

    [SerializeField] private Image[] _lifeImages;

    private void OnEnable(){
        _playerHealthChannel.OnRaiseEvent += OnPlayerHealthChanged;
    }
    private void OnDisable(){
        _playerHealthChannel.OnRaiseEvent -= OnPlayerHealthChanged;
    }   

    private void OnPlayerHealthChanged(int currentLives){
        
        for(int i = 0; i < _lifeImages.Length; i++){
            _lifeImages[i].enabled = i < currentLives;
        }
    }
}
