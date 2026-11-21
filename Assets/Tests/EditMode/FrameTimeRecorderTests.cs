using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Herramienta de medición: los percentiles y la lectura de la línea de comandos.
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
    public void LeerArgumento_SinElArgumento_NoMide()
    {
        Assert.IsNull(FrameTimeRecorder.ReadArgument(new[] { "Juego.exe" }, "--measure"));
        Assert.IsNull(FrameTimeRecorder.ReadArgument(new[] { "Juego.exe", "--measure" }, "--measure"));
    }
}
