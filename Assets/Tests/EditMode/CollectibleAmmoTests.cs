using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Regla: un coleccionable de munición solo lo recoge el Brujo, y una sola vez.
/// </summary>
public class CollectibleAmmoTests
{
    private GameObject _wizard;
    private PlayerBlackboard _blackboard;
    private GameObject _ammoObject;
    private CollectibleAmmo _ammo;
    private GameObject _other;   // lo crea cada prueba que lo necesita; TearDown lo borra aunque falle un Assert

    [SetUp]
    public void SetUp()
    {
        _wizard = new GameObject("Brujo") { tag = "Player" };
        _blackboard = _wizard.AddComponent<PlayerBlackboard>();
        _blackboard.bulletCount = 0;

        _ammoObject = new GameObject("Collectible");
        _ammo = _ammoObject.AddComponent<CollectibleAmmo>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_wizard);
        Object.DestroyImmediate(_ammoObject);
        if (_other != null) Object.DestroyImmediate(_other);
    }

    [Test]
    public void ElBrujoLoToca_SumaSuMunicion()
    {
        Assert.IsTrue(_ammo.TryCollect(_wizard));
        Assert.AreEqual(_ammo.AmmoAmount, _blackboard.bulletCount);
    }

    [Test]
    public void ToqueRepetido_SumaUnaSolaVez()
    {
        // Dos avisos de trigger seguidos (mismo paso de física o con el componente apagado)
        _ammo.TryCollect(_wizard);

        Assert.IsFalse(_ammo.TryCollect(_wizard));
        Assert.AreEqual(_ammo.AmmoAmount, _blackboard.bulletCount);
    }

    [Test]
    public void LoTocaAlgoQueNoEsElBrujo_NoLoRecoge()
    {
        // Un objeto sin el tag Player (Squishy, un enemigo, una bala) aunque tenga blackboard
        _other = new GameObject("Otro");
        var otherBlackboard = _other.AddComponent<PlayerBlackboard>();
        otherBlackboard.bulletCount = 0;

        Assert.IsFalse(_ammo.TryCollect(_other));
        Assert.AreEqual(0, otherBlackboard.bulletCount);
        Assert.IsTrue(_ammoObject.activeSelf);
    }

    [Test]
    public void TagPlayerSinBlackboard_NoLoRecogeNiSeGasta()
    {
        // Un collider hijo con el tag Player, por ejemplo: no hay a quién sumarle la munición
        _other = new GameObject("PlayerSinBlackboard") { tag = "Player" };

        Assert.IsFalse(_ammo.TryCollect(_other));
        Assert.IsTrue(_ammoObject.activeSelf);
        Assert.IsTrue(_ammo.TryCollect(_wizard), "El Brujo todavía debe poder recogerlo");
    }

    [Test]
    public void AlRecogerlo_Desaparece()
    {
        _ammo.TryCollect(_wizard);

        Assert.IsFalse(_ammoObject.activeSelf);
    }
}
