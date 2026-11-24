using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Capturas del nivel para comparar cambios de arte (p. ej. la compresión de las texturas):
/// coloca al Brujo a varias alturas, espera a que la cámara lo alcance, oculta la interfaz,
/// congela el tiempo y guarda un PNG por altura. Solo la usa FrameTimeRecorder con --capture.
/// </summary>
public static class LevelCapture
{
    // Alturas del Brujo que cubren las 3 zonas: verde (y < 1), roja (de 1 a 17) y azul (> 17).
    // La cámara va de y −16,3 a 38,3 (CameraBounds) y su vista mide justo el ancho del nivel: x no importa
    public static readonly float[] Heights = { -14f, -6f, 4f, 12f, 24f, 35f };

    private const float SettleSeconds = 2.5f; // lo que tarda la cámara de Cinemachine en alcanzarlo

    public static IEnumerator Run(string folder)
    {
        PlayerMovement player = Object.FindAnyObjectByType<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("[LevelCapture] No hay PlayerMovement en el nivel: no hay capturas.");
            yield break;
        }
        Directory.CreateDirectory(folder);

        // Fuera de la física: queda quieto en el aire, no toca triggers (ni el final del nivel) y las balas lo atraviesan
        player.GetComponent<Rigidbody2D>().simulated = false;

        // Solo el mundo: fuera todos los Canvas (diálogo de entrada, vidas y cualquiera en el mundo)
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            canvas.enabled = false;

        // Las capturas solo se comparan si salen al mismo tamaño: se anota en el log del juego
        Debug.Log($"[LevelCapture] Capturas a {Screen.width}x{Screen.height} en '{folder}'.");

        for (int i = 0; i < Heights.Length; i++)
        {
            Vector3 position = player.transform.position;
            player.transform.position = new Vector3(position.x, Heights[i], position.z);
            yield return new WaitForSecondsRealtime(SettleSeconds);

            // Animaciones y partículas quietas durante la foto
            Time.timeScale = 0f;
            yield return new WaitForEndOfFrame();
            SaveScreenshot(Path.Combine(folder, FileName(i, Heights[i])));
            Time.timeScale = 1f;
        }
    }

    // Si algo falla, se avisa y se sigue: el tiempo vuelve a correr y el juego llega a cerrarse
    private static void SaveScreenshot(string path)
    {
        Texture2D shot = null;
        try
        {
            shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(path, shot.EncodeToPNG());
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[LevelCapture] No se pudo guardar la captura '{path}': {exception.Message}");
        }
        finally
        {
            if (shot != null) Object.Destroy(shot);
        }
    }

    /// <summary>Nombre de cada captura, ordenado y con su altura: 01_y-14.png.</summary>
    public static string FileName(int index, float height)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:00}_y{1}.png", index + 1, height);
    }
}
