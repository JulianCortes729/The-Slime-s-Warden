using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{

    [SerializeField] private float bounceForce = 15f;

    [SerializeField] private Animator _animator;

    private int _bounceAnimHash;

    void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
        _bounceAnimHash = Animator.StringToHash("Bounce");
    }

     private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovement))
        {
            // Solo aplica el rebote si el contacto es principalmente vertical (evita rebotar al tocar de lado)
            if(collision.GetContact(0).normal.y < -0.5f)
            {
               playerMovement.ApplyBounce(bounceForce);
               _animator.SetTrigger(_bounceAnimHash);  
            } 
        }
    }
}
