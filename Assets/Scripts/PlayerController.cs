using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
     [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    [SerializeField] Animator animator;
    bool facingLeft;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Read movement input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Optional: reset Y velocity
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if((rb.linearVelocity.x < 0) && (!facingLeft))
        {
            facingLeft = true;
            Vector3 _scale = transform.localScale;
            _scale.x = Mathf.Abs(_scale.x);
            transform.localScale = _scale;
        }
        if((rb.linearVelocity.x > 0) && facingLeft)
        {
            facingLeft = false;
            Vector3 _scale = transform.localScale;
            _scale.x *= -1;
            transform.localScale = _scale;
        }

    }

    void FixedUpdate()
    {
        // Move the player
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        animator.SetFloat("velocity", Mathf.Abs(rb.linearVelocity.x));

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only care about Tilemap trigger
        if (other.TryGetComponent(out Tilemap _tilemap))
        {
            Vector3 _contactWorldPos = other.bounds.center;
            Vector3Int _cellPos = _tilemap.WorldToCell(_contactWorldPos);
            TileBase _tile = _tilemap.GetTile(_cellPos);

            if (_tile != null)
            {
                TileLogic.instance.InvokeTileAction(_tile, gameObject);
                Debug.Log("Player touched tile: " + _tile.name);
            }
            else Debug.Log($"No tile found at {_cellPos}");
        }
    }
}
