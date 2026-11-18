using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Regla: la pantalla final muestra el resultado de la ÚLTIMA partida.
/// </summary>
public class GameResultTests
{
    private GameResult _result;

    [SetUp]
    public void SetUp() => _result = ScriptableObject.CreateInstance<GameResult>();

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_result);

    [Test]
    public void SinPartidaAnotada_EsDerrota()
    {
        Assert.AreEqual(GameOutcome.Defeat, _result.Outcome);
    }

    [Test]
    public void AlGanar_EsVictoria()
    {
        _result.Record(GameOutcome.Victory);
        Assert.AreEqual(GameOutcome.Victory, _result.Outcome);
    }

    [Test]
    public void GanarYDespuesMorir_EsDerrota()
    {
        // La trampa: un resultado viejo que se queda pegado tras "Jugar de nuevo"
        _result.Record(GameOutcome.Victory);
        _result.Record(GameOutcome.Defeat);
        Assert.AreEqual(GameOutcome.Defeat, _result.Outcome);
    }
}
