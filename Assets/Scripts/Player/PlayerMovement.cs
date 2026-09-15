using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerBlackboard))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerBlackboard blackboard;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Physics & Game Feel")]
    //Controlamos qué tan rápido acelera o frena el personaje.
    //En el suelo queremos que sea instantáneo (snappy).
    [SerializeField] private float groundAcceleration = 50f;
    // En el aire queremos que resbale un poco más para conservar inercia de saltos y golpes.
    [SerializeField] private float airAcceleration = 15f;

    private void Awake()
    {
        if (blackboard == null) blackboard = GetComponent<PlayerBlackboard>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if(blackboard.isTalking) return; // No podemos movernos si estamos hablando

        if(blackboard.isDead) return;// No podemos movernos si estamos muertos

        if(blackboard.jumpBufferTimer > 0f)
        {
            blackboard.jumpBufferTimer -= Time.fixedDeltaTime; // Consume el buffer con el tiempo
        }

        if(blackboard.coyoteTimer > 0f)
        {
            blackboard.coyoteTimer -= Time.fixedDeltaTime; // Consume el coyote timer con el tiempo
        }

        //Temporizador de estado no bloqueante.
        //Reemplaza al viejo chequeo que requería tocar el suelo.
        if (blackboard.knockbackTimer > 0f)
        {
            // Descontamos tiempo de forma constante independientemente de los FPS
            blackboard.knockbackTimer -= Time.fixedDeltaTime;

            //Salimos del método usando `return`. 
            //Mientras el timer sea mayor a 0, Unity ignorará las líneas de movimiento 
            //que están abajo, dejando al personaje suelto volando por la física.
            return;
        }

        //CÁLCULO DE MOVIMIENTO CON INERCIA
        // ┌─ Qué hace: Frena o acelera al personaje gradualmente hacia la velocidad de la tecla presionada.
        // ├─ Por qué existe: Para evitar frenados en seco cuando el jugador recibe un golpe o suelta el joystick en el aire.
        // ├─ Sin esto, pasaría: El bug que reportaste. El jugador frena en el aire y cae recto como una piedra.
        // └─ Decisión de diseño: Usamos dos valores distintos de aceleración. El aire tiene menos "fricción" que el suelo.

        float targetVelocityX = blackboard.moveInput * moveSpeed;
        float currentAcceleration = blackboard.isGrounded ? groundAcceleration : airAcceleration;

        //Suavizado Matemático
        //Movemos el valor actual (rb.velocity.x) hacia la meta (targetVelocityX) a una velocidad definida.
        float smoothedX = Mathf.MoveTowards(rb.velocity.x, targetVelocityX, currentAcceleration * Time.fixedDeltaTime);

        rb.velocity = new Vector2(smoothedX, rb.velocity.y);

        // El intent solo se consume si el salto se ejecuta efectivamente.
        // Si el jugador presiona salto en el aire, el intent persiste hasta que aterriza.
        if (blackboard.jumpBufferTimer > 0f && (blackboard.isGrounded || blackboard.coyoteTimer > 0f))
        {
            blackboard.jumpBufferTimer = 0f; // Resetea el buffer al aterrizar
            blackboard.coyoteTimer = 0f; // Resetea el coyote timer al aterrizar
            rb.velocity = new Vector2(rb.velocity.x, 0f); // cancela velocidad vertical residual
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
    }



    public void ApplyBounce(float bounceForce)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f); // cancela velocidad vertical residual
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
    }
}
