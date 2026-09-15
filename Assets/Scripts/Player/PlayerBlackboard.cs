using UnityEngine;

/// Blackboard compartido entre todos los sistemas del jugador.
/// Solo contiene datos — ningún sistema depende de otro directamente.
public class PlayerBlackboard : MonoBehaviour
{
    [Header("Movement")]
    public bool isGrounded;
    public float jumpBufferTime = 0.15f; // Tiempo durante el cual un salto es válido después de perder el contacto con el suelo
    public float jumpBufferTimer; // Temporizador que se resetea al presionar salto y se consume al aterrizar

    public float coyoteTime = 0.15f; // Tiempo durante el cual un salto es válido después de perder el contacto con el suelo
    public float coyoteTimer; // Temporizador que se resetea al perder el contacto
    
    public float moveInput;

    [Header("Combat")]
    public int bulletCount;    // FIX #15/#18: era "contBullet" — nombre no descriptivo
    public bool shootIntent;

    [Header("Health")]
    public int maxLives = 3;
    public int currentLives = 3;

    [Header("Invulnerability")]
    public bool isInvulnerable = false;
    public float invulnerabilityTime = 1.5f;


    [Header("Knockback Stun")]
    //Separamos el aturdimiento físico del escudo visual.
    //Solo quitamos el control por una fracción de segundo para no arruinar el Game Feel.
    public float knockbackDuration = 0.2f;
    public float knockbackTimer = 0f;

    public bool isDead = false;

    [Header("Estados Narrativos")]
    public bool isTalking = false; //Por defecto el Brujo no está hablando

    public bool isKnockedBack;
}