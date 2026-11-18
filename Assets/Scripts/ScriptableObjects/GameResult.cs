using System;
using UnityEngine;

/// <summary>Cómo terminó la partida.</summary>
public enum GameOutcome
{
    Defeat,  // el Brujo murió
    Victory  // Squishy llegó a la cima con su familia
}

/// <summary>
/// Resultado de la última partida, compartido entre escenas: el GameManager lo anota al terminar
/// el nivel y la pantalla final lo lee para mostrar la victoria o la derrota.
/// </summary>
// 🧩 Patrón: "Data as Asset" — el dato vive en un asset que las dos escenas referencian,
// así ninguna necesita buscar a la otra ni un estático global.
[CreateAssetMenu(fileName = "GameResult", menuName = "Game/Game Result")]
public class GameResult : ScriptableObject
{
    // [NonSerialized]: es un dato de la partida, no del asset. No se guarda en disco (no ensucia git)
    // y al recargar los scripts vuelve a Defeat.
    [NonSerialized] private GameOutcome _outcome = GameOutcome.Defeat;

    public GameOutcome Outcome => _outcome;

    public void Record(GameOutcome outcome) => _outcome = outcome;
}
