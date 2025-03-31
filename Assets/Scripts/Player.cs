using UnityEngine;
using Mirror;


public class Player : NetworkBehaviour
{
    public Rigidbody2D rb;
    private float horizontal = 0;
    public float speed = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        // Move somewhere
        horizontal = Input.GetAxis("Horizontal");
        rb.AddForce(new Vector2(horizontal * speed, 0));
    }
}
