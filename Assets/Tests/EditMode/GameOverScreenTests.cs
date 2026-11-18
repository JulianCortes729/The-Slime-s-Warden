using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Regla: al ganar se ve la variante de victoria y al morir la de derrota, cada una con su fondo
/// y su texto en el botón principal. Se monta una pantalla mínima sin abrir la escena.
/// </summary>
public class GameOverScreenTests
{
    private static readonly Color VictoryBackground = new Color(0.03f, 0.05f, 0.13f);
    private static readonly Color DefeatBackground = new Color(0.02f, 0.04f, 0.035f);

    private GameObject _root;
    private GameObject _victoryObject;
    private GameObject _defeatObject;
    private Camera _camera;
    private TMP_Text _retryLabel;
    private GameOverScreen _screen;

    [SetUp]
    public void SetUp()
    {
        _root = new GameObject("PantallaDePrueba");
        _victoryObject = Child("Victoria");
        _defeatObject = Child("Derrota");
        _camera = Child("Camara").AddComponent<Camera>();
        _retryLabel = Child("BotonPrincipal").AddComponent<TextMeshProUGUI>();

        // En modo Edit, AddComponent no llama a Awake: la pantalla no hace nada hasta llamar a Show
        _screen = _root.AddComponent<GameOverScreen>();

        // Se rellena como en el Inspector (los campos son privados)
        var serialized = new SerializedObject(_screen);
        serialized.FindProperty("_camera").objectReferenceValue = _camera;
        serialized.FindProperty("_retryLabel").objectReferenceValue = _retryLabel;
        SetVariant(serialized, "_victory", _victoryObject, VictoryBackground, "Jugar de nuevo");
        SetVariant(serialized, "_defeat", _defeatObject, DefeatBackground, "Reintentar");
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(_root);

    [Test]
    public void Victoria_MuestraSuVarianteYOcultaLaDerrota()
    {
        _screen.Show(GameOutcome.Victory);

        Assert.IsTrue(_victoryObject.activeSelf);
        Assert.IsFalse(_defeatObject.activeSelf);
        Assert.AreEqual(VictoryBackground, _camera.backgroundColor);
        Assert.AreEqual("Jugar de nuevo", _retryLabel.text);
    }

    [Test]
    public void Derrota_MuestraSuVarianteYOcultaLaVictoria()
    {
        _screen.Show(GameOutcome.Defeat);

        Assert.IsFalse(_victoryObject.activeSelf);
        Assert.IsTrue(_defeatObject.activeSelf);
        Assert.AreEqual(DefeatBackground, _camera.backgroundColor);
        Assert.AreEqual("Reintentar", _retryLabel.text);
    }

    [Test]
    public void DerrotaDespuesDeVictoria_NoDejaRestosDeLaVictoria()
    {
        _screen.Show(GameOutcome.Victory);
        _screen.Show(GameOutcome.Defeat);

        Assert.IsFalse(_victoryObject.activeSelf);
        Assert.AreEqual("Reintentar", _retryLabel.text);
    }

    [Test]
    public void SinResultadoAnotado_TocaLaDerrota()
    {
        Assert.AreEqual(GameOutcome.Defeat, GameOverScreen.OutcomeToShow(null));
    }

    [Test]
    public void ConVictoriaAnotada_TocaLaVictoria()
    {
        var result = ScriptableObject.CreateInstance<GameResult>();
        result.Record(GameOutcome.Victory);
        Assert.AreEqual(GameOutcome.Victory, GameOverScreen.OutcomeToShow(result));
        Object.DestroyImmediate(result);
    }

    private GameObject Child(string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(_root.transform);
        return child;
    }

    private static void SetVariant(SerializedObject serialized, string field, GameObject target, Color background, string retryText)
    {
        SerializedProperty objects = serialized.FindProperty(field + ".objects");
        objects.arraySize = 1;
        objects.GetArrayElementAtIndex(0).objectReferenceValue = target;
        serialized.FindProperty(field + ".background").colorValue = background;
        serialized.FindProperty(field + ".retryText").stringValue = retryText;
    }
}
