using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Regla: el sonido de moverse suena una vez por cada cambio de botón que hace el jugador, y nunca solo.
/// </summary>
public class MenuSoundsTests
{
    private const int LongAgo = 100; // frames desde la última pulsación: ninguna reciente

    private GameObject _play;
    private GameObject _quit;

    [SetUp]
    public void SetUp()
    {
        _play = new GameObject("ButtonPlay");
        _quit = new GameObject("ButtonQuit");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_play);
        Object.DestroyImmediate(_quit);
    }

    [Test]
    public void PasarDeUnBotonAOtro_Suena()
    {
        Assert.IsTrue(MenuSounds.ShouldPlayMove(_play, _quit, LongAgo));
    }

    [Test]
    public void PrimeraSeleccionDeLaPantalla_NoSuena()
    {
        // El EventSystem elige First Selected al abrir la pantalla: no lo hizo el jugador
        Assert.IsFalse(MenuSounds.ShouldPlayMove(null, _play, LongAgo));
    }

    [Test]
    public void ClicEnElVacio_NoSuena()
    {
        Assert.IsFalse(MenuSounds.ShouldPlayMove(_play, null, LongAgo));
    }

    [Test]
    public void RecuperarElMismoBoton_NoSuena()
    {
        // SelectionKeeper devuelve el foco al último botón tras un clic en el vacío
        Assert.IsFalse(MenuSounds.ShouldPlayMove(_play, _play, LongAgo));
    }

    [TestCase(0)]
    [TestCase(1)]
    public void FocoQueSaltaAlPulsar_NoSuena(int framesSincePress)
    {
        // Pulsar Controles abre el panel y pasa el foco a Cerrar en ese frame o en el siguiente:
        // ya sonó la pulsación
        Assert.IsFalse(MenuSounds.ShouldPlayMove(_play, _quit, framesSincePress));
    }

    [Test]
    public void MoverseDosFramesDespuesDePulsar_Suena()
    {
        Assert.IsTrue(MenuSounds.ShouldPlayMove(_play, _quit, 2));
    }
}
