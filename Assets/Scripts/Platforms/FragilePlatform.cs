using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum FragilePlatformState
{
    Intact,
    Shaking,
    Broken
}

public class FragilePlatform : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField] private float _shakeDuration = 1f;
    [SerializeField] private float _shakeIntensity = 0.1f;

    [Header("Referencias")]
    [Tooltip("Dejalo vacío: se autocompleta en Awake con TODOS los SpriteRenderer hijos.")]
    [SerializeField] private SpriteRenderer[] _spriteRenderers;
    [SerializeField] private Collider2D _collider;

    private FragilePlatformState _currentState = FragilePlatformState.Intact;
    private float _shakeTimer;
    private Vector2 _originalPosition;


    private Rigidbody2D _rb;

    // Start is called before the first frame update
    private void Awake()
    {
        _originalPosition = transform.position;

        // 📖 GetComponentsInChildren (plural) y NO GetComponent: los sprites ya no
        // viven en la raíz sino en Part1/Part2. Este era el null que reventaba.
        // 📖 El 'true' incluye hijos DESACTIVADOS. Sin él, cualquier parte que
        // arranque apagada queda fuera del array y no la volvés a controlar nunca.
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (_spriteRenderers.Length == 0)
            Debug.LogWarning($"[FragilePlatform] '{name}' no tiene ningún SpriteRenderer hijo.", this);

        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_currentState == FragilePlatformState.Intact && collision.gameObject.CompareTag("Player"))
        {
            _currentState = FragilePlatformState.Shaking;
            _shakeTimer = _shakeDuration;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (_currentState == FragilePlatformState.Shaking)
        {
            _shakeTimer -= Time.deltaTime;

            if (_shakeTimer <= 0f)
            {
                BreakPlatform();
                return;
            }

            // 📖 MovePosition en vez de transform.position: le pedís al motor que
            // mueva el cuerpo, en vez de mover el Transform por afuera y obligarlo
            // a darse cuenta después. Esto es lo que deja Sync Colliders en 0.
            _rb.MovePosition(_originalPosition + Random.insideUnitCircle * _shakeIntensity);
        }
    }

    private void BreakPlatform()
    {
        _currentState = FragilePlatformState.Broken;

        // 📖 Volvemos a la posición original ANTES de ocultar. Sin esto la plataforma
        // queda congelada en el último desplazamiento random del shake, y al resetearla
        // reaparece corrida unos píxeles respecto de donde la pusiste en la escena.
        transform.position = _originalPosition;
        _rb.position = _originalPosition;

        _collider.enabled = false;
        SetSpritesVisible(false);
    }

    // 📖 public: antes era private y NADIE la llamaba — código muerto. Ahora un
    // checkpoint, un respawn o un GameManager pueden restaurar la plataforma.
    public void ResetPlatform()
    {
        _currentState = FragilePlatformState.Intact;
        _shakeTimer = 0f;
        transform.position = _originalPosition;
        _rb.position = _originalPosition;
        _collider.enabled = true;
        SetSpritesVisible(true);
    }

    // 📖 Un solo punto de control para la visibilidad. Break y Reset lo comparten,
    // así es imposible que se desincronicen (apagar 2 sprites y prender 1).
    private void SetSpritesVisible(bool visible)
    {
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            // 📖 Null-check por elemento: si borrás un hijo en el Inspector después
            // de que el array quedó serializado, ese slot queda en null.
            if (_spriteRenderers[i] == null) continue;
            _spriteRenderers[i].enabled = visible;
        }
    }
}
