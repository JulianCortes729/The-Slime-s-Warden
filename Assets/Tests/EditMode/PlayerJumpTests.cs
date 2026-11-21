using NUnit.Framework;

/// <summary>
/// Regla del salto del Brujo: salta con un salto pedido si está en el suelo o acaba de dejarlo,
/// y justo después de despegar el suelo no cuenta (ni para saltar otra vez ni para pisar un rebote).
/// </summary>
public class PlayerJumpTests
{
    private const float Pressed = 0.1f;      // buffer de salto aún vivo
    private const float CoyoteLeft = 0.05f;  // margen tras dejar un borde
    private const float IgnoreLeft = 0.08f;  // instante después de despegar

    [Test]
    public void SaltoPedidoAlAterrizar_Salta()
    {
        bool grounded = GroundDetector.CountsAsGround(true, 0f);
        Assert.IsTrue(PlayerMovement.CanJump(Pressed, grounded, 0f));
    }

    [Test]
    public void CoyoteTimeAlDejarUnBorde_Salta()
    {
        Assert.IsTrue(PlayerMovement.CanJump(Pressed, false, CoyoteLeft));
    }

    [Test]
    public void SinSaltoPedido_NoSalta()
    {
        Assert.IsFalse(PlayerMovement.CanJump(0f, true, CoyoteLeft));
    }

    [Test]
    public void DobleToqueJustoDespuesDeSaltar_NoSaltaEnElAire()
    {
        // Los pies aún rozan el suelo, pero el salto gastó el coyote y el suelo no cuenta
        bool grounded = GroundDetector.CountsAsGround(true, IgnoreLeft);
        Assert.IsFalse(grounded);
        Assert.IsFalse(PlayerMovement.CanJump(Pressed, grounded, 0f));
    }

    [Test]
    public void SaltoPedidoTrasElRebote_NoPisaElRebote()
    {
        // Paso de física siguiente al rebote: el círculo aún toca el trampolín (y un suelo al lado)
        bool grounded = GroundDetector.CountsAsGround(true, IgnoreLeft);
        Assert.IsFalse(PlayerMovement.CanJump(Pressed, grounded, 0f));
    }

    [Test]
    public void TrampolinMientrasCaes_NoEsSuelo_YRebota()
    {
        // El círculo toca el trampolín antes que la cápsula: un salto pedido no debe salir ahí
        Assert.IsFalse(GroundDetector.IsSolidGround(true, -5f));
    }

    [Test]
    public void ParadoEncimaDelTrampolinSinRebotar_EsSuelo()
    {
        // Subió de lado o por una esquina: sin esto se quedaría sin poder saltar
        Assert.IsTrue(GroundDetector.IsSolidGround(true, 0f));
    }

    [Test]
    public void SueloNormal_CuentaAunqueCaigas()
    {
        Assert.IsTrue(GroundDetector.IsSolidGround(false, -5f));
    }

    [Test]
    public void PasadoElInstanteDeDespegue_ElSueloVuelveAContar()
    {
        Assert.IsTrue(GroundDetector.CountsAsGround(true, 0f));
        Assert.IsFalse(GroundDetector.CountsAsGround(false, 0f));
    }
}
