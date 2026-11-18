using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Pantalla final: muestra la variante de victoria o la de derrota según cómo terminó la partida.
/// Cada variante es un grupo de objetos (escenario y textos), un color de fondo y el texto del botón principal.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Serializable]
    public struct Variant
    {
        [Tooltip("Lo que solo se ve en esta variante: su escenario, su título y su subtítulo")]
        public GameObject[] objects;
        [Tooltip("Color de fondo de la cámara")]
        public Color background;
        [Tooltip("Texto del botón principal (p. ej. \"Jugar de nuevo\" o \"Reintentar\")")]
        public string retryText;
    }

    [Tooltip("Lo anota el GameManager al terminar el nivel")]
    [SerializeField] private GameResult _gameResult;
    [SerializeField] private Camera _camera;
    [Tooltip("Texto del botón principal, que cambia según la variante")]
    [SerializeField] private TMP_Text _retryLabel;
    [SerializeField] private Variant _victory;
    [SerializeField] private Variant _defeat;

    private void Awake()
    {
        // En Awake, antes del primer frame: así nunca se ve un instante la variante equivocada
        Show(OutcomeToShow(_gameResult));
    }

    /// <summary>Qué variante toca: la del resultado anotado o, sin resultado (Play directo en esta escena), la derrota.</summary>
    public static GameOutcome OutcomeToShow(GameResult result) =>
        result != null ? result.Outcome : GameOutcome.Defeat;

    /// <summary>Enciende la variante del resultado y apaga la otra.</summary>
    public void Show(GameOutcome outcome)
    {
        bool victory = outcome == GameOutcome.Victory;
        SetActive(_victory.objects, victory);
        SetActive(_defeat.objects, !victory);

        Variant shown = victory ? _victory : _defeat;
        if (_camera != null) _camera.backgroundColor = shown.background;
        if (_retryLabel != null) _retryLabel.text = shown.retryText;
    }

    private static void SetActive(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        foreach (GameObject target in objects)
        {
            if (target != null) target.SetActive(active);
        }
    }
}
