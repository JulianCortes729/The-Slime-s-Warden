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
        // Antes de los return: si habla justo tras un salto, el suelo vuelve a contar a su tiempo
        if(blackboard.groundIgnoreTimer > 0f)
        {
            blackboard.groundIgnoreTimer -= Time.fixedDeltaTime;
        }

        if(blackboard.isDead) return;// No podemos movernos si estamos muertos

        if(blackboard.isTalking)
        {
            // Quieto mientras habla: sin deslizarse con la velocidad que traía
            // y sin un salto pulsado justo antes que salga al cerrar el diálogo
            blackboard.jumpBufferTimer = 0f;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

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
        if (CanJump(blackboard.jumpBufferTimer, blackboard.isGrounded, blackboard.coyoteTimer))
        {
            LeaveGround();
            rb.velocity = new Vector2(rb.velocity.x, 0f); // cancela velocidad vertical residual
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

    }

    /// <summary>
    /// Regla del salto: hay un salto pedido (buffer) y el Brujo está en el suelo o acaba de dejarlo (coyote time).
    /// </summary>
    public static bool CanJump(float jumpBufferTimer, bool isGrounded, float coyoteTimer)
    {
        return jumpBufferTimer > 0f && (isGrounded || coyoteTimer > 0f);
    }

    public void ApplyBounce(float bounceForce)
    {
        // El rebote manda: absorbe el salto pulsado al caer, que si no lo pisaría con un salto normal
        LeaveGround();
        rb.velocity = new Vector2(rb.velocity.x, 0f); // cancela velocidad vertical residual
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
    }

    // Al despegar se gastan el salto pedido y el coyote time, y el suelo deja de contar un instante
    private void LeaveGround()
    {
        blackboard.jumpBufferTimer = 0f;
        blackboard.coyoteTimer = 0f;
        blackboard.isGrounded = false;
        blackboard.groundIgnoreTimer = blackboard.groundIgnoreTime;
    }
}
