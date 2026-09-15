using UnityEngine;

//ScriptableObjects.
//Le dice a Unity que agregue una opción en el menú de click derecho (Create).
[CreateAssetMenu(fileName = "NewDialogue", menuName = "RiseWithoutSky/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    //Los datos reales son privados para protegerlos (Encapsulamiento).
    [Tooltip("El nombre del personaje que está hablando en este diálogo.")]
    [SerializeField] private string _characterName;

    //TextArea obliga al Inspector de Unity a mostrar una caja de texto grande,
    //en lugar de una sola línea estrecha. Mínimo 3 líneas de alto, máximo 5.
    [TextArea(3, 5)]
    [Tooltip("Cada elemento de esta lista es una pantalla/globo de texto nuevo.")]
    [SerializeField] private string[] _sentences;

    //PATRÓN: Getters (Propiedades de solo lectura).
    //Usamos el símbolo => (Lambda) como atajo para devolver el valor de la variable privada.
    public string CharacterName => _characterName;
    
    public string[] Sentences => _sentences;
}
