using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Herramienta de diagnóstico: percentiles, línea de comandos y capturas del nivel.
/// </summary>
public class FrameTimeRecorderTests
{
    private static List<float> OneToHundred()
    {
        List<float> values = new List<float>();
        for (int i = 1; i <= 100; i++) values.Add(i);
        return values;
    }

    [TestCase(95f, 95f)]
    [TestCase(99f, 99f)]
    [TestCase(100f, 100f)]
    public void Percentil_DejaPorDebajoEsePorcentajeDeFrames(float percent, float expected)
    {
        Assert.AreEqual(expected, FrameTimeRecorder.Percentile(OneToHundred(), percent));
    }

    [Test]
    public void Percentil_SinFrames_EsCero()
    {
        Assert.AreEqual(0f, FrameTimeRecorder.Percentile(new List<float>(), 95f));
    }

    [Test]
    public void LeerArgumento_DevuelveLaRutaQueSigue()
    {
        string[] args = { "Juego.exe", "--measure", "C:/medicion.txt" };
        Assert.AreEqual("C:/medicion.txt", FrameTimeRecorder.ReadArgument(args, "--measure"));
    }

    [Test]
    public void Capturas_CubrenLasTresZonas()
    {
        // Verde (y < 1), roja (de 1 a 17) y azul (> 17): una comparación de arte debe verlas todas
        Assert.IsTrue(System.Array.Exists(LevelCapture.Heights, y => y < 1f));
        Assert.IsTrue(System.Array.Exists(LevelCapture.Heights, y => y >= 1f && y <= 17f));
        Assert.IsTrue(System.Array.Exists(LevelCapture.Heights, y => y > 17f));
    }

    [Test]
    public void NombreDeCaptura_OrdenadoYConSuAltura()
    {
        Assert.AreEqual("01_y-14.png", LevelCapture.FileName(0, -14f));
        Assert.AreEqual("06_y35.png", LevelCapture.FileName(5, 35f));
    }

    [Test]
    public void LeerArgumento_SinElArgumento_NoMide()
    {
        Assert.IsNull(FrameTimeRecorder.ReadArgument(new[] { "Juego.exe" }, "--measure"));
        Assert.IsNull(FrameTimeRecorder.ReadArgument(new[] { "Juego.exe", "--measure" }, "--measure"));
    }
}
