using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Mide el rendimiento en el juego exportado. Solo existe si el ejecutable se abre con
/// --measure &lt;ruta del informe&gt;: pulsa Jugar en el menú, espera a que cargue el nivel,
/// descarta unos segundos de calentamiento, graba cada frame durante 30 s, escribe el informe
/// y cierra el juego. Sin ese argumento no se crea: el juego normal no lo nota.
/// </summary>
public class FrameTimeRecorder : MonoBehaviour
{
    private const string MeasureArgument = "--measure";
    private const string LevelScene = "SampleScene";
    private const float WarmupSeconds = 3f;   // carga de texturas, primeros sonidos, cámara asentándose
    private const float MeasureSeconds = 30f;

    private string _reportPath;
    private readonly List<float> _frameMs = new List<float>(8192);
    private readonly List<float> _cpuMainMs = new List<float>(8192);
    private readonly List<float> _gpuMs = new List<float>(8192);
    private readonly FrameTiming[] _timing = new FrameTiming[1];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIfRequested()
    {
        string path = ReadArgument(System.Environment.GetCommandLineArgs(), MeasureArgument);
        if (path == null) return;

        GameObject recorder = new GameObject(nameof(FrameTimeRecorder));
        DontDestroyOnLoad(recorder);
        recorder.AddComponent<FrameTimeRecorder>()._reportPath = path;
    }

    private IEnumerator Start()
    {
        // Sin vsync ni tope de FPS: medimos lo que tarda el frame, no lo que espera al monitor.
        // runInBackground: si la ventana pierde el foco, el juego no se pausa a mitad de la medición
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        Application.runInBackground = true;

        // Pulsa Jugar como lo haría el jugador: el GameManager carga el nivel
        yield return null;
        MainMenuUI menu = FindAnyObjectByType<MainMenuUI>();
        if (menu != null) menu.OnPlayButtonClicked();
        while (SceneManager.GetActiveScene().name != LevelScene)
            yield return null;

        yield return new WaitForSecondsRealtime(WarmupSeconds);

        bool interrupted = false;
        float end = Time.realtimeSinceStartup + MeasureSeconds;
        while (Time.realtimeSinceStartup < end)
        {
            yield return null;
            if (SceneManager.GetActiveScene().name != LevelScene)
            {
                interrupted = true; // murió el jugador o terminó el nivel
                break;
            }

            _frameMs.Add(Time.unscaledDeltaTime * 1000f);

            // CPU y GPU por separado (requiere Frame Timing Stats en Player Settings)
            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, _timing) > 0)
            {
                _cpuMainMs.Add((float)_timing[0].cpuMainThreadFrameTime);
                _gpuMs.Add((float)_timing[0].gpuFrameTime);
            }
        }

        try
        {
            File.WriteAllText(_reportPath, BuildReport(interrupted));
        }
        catch (System.Exception exception)
        {
            // Ruta mala o sin permiso: se avisa en el log del juego y se cierra igual
            Debug.LogError($"[FrameTimeRecorder] No se pudo escribir el informe en '{_reportPath}': {exception.Message}");
        }
        Application.Quit();
    }

    private string BuildReport(bool interrupted)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine($"Escena: {LevelScene} · {Screen.width}x{Screen.height} · sin vsync · calidad {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        report.AppendLine($"Frames: {_frameMs.Count} · interrumpida: {(interrupted ? "sí (cambió la escena)" : "no")}");
        report.AppendLine("Medida | promedio | p95 | p99 | peor (ms)");
        AppendRow(report, "Frame", _frameMs);
        AppendRow(report, "CPU hilo principal", _cpuMainMs);
        AppendRow(report, "GPU", _gpuMs);
        return report.ToString();
    }

    private static void AppendRow(StringBuilder report, string label, List<float> values)
    {
        if (values.Count == 0)
        {
            report.AppendLine($"{label} | sin datos");
            return;
        }

        List<float> sorted = new List<float>(values);
        sorted.Sort();
        float sum = 0f;
        foreach (float value in sorted) sum += value;

        report.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} | {1:0.00} | {2:0.00} | {3:0.00} | {4:0.00}",
            label, sum / sorted.Count, Percentile(sorted, 95f), Percentile(sorted, 99f), sorted[sorted.Count - 1]));
    }

    /// <summary>
    /// Percentil por rango (nearest-rank) sobre una lista ya ordenada: el p95 es el valor
    /// que deja por debajo al 95 % de los frames. Lista vacía: 0.
    /// </summary>
    public static float Percentile(List<float> sortedValues, float percent)
    {
        if (sortedValues == null || sortedValues.Count == 0) return 0f;
        int rank = Mathf.CeilToInt(percent / 100f * sortedValues.Count);
        return sortedValues[Mathf.Clamp(rank - 1, 0, sortedValues.Count - 1)];
    }

    /// <summary>Devuelve el valor que sigue a <paramref name="flag"/> en la línea de comandos, o null.</summary>
    public static string ReadArgument(string[] args, string flag)
    {
        if (args == null) return null;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == flag) return args[i + 1];
        return null;
    }
}
