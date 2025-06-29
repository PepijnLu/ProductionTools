using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour, IInvoker
{
    bool playing;

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

    [Header("Stats")]
    int coinsCollected;
    float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInputs();
        RunTimer();
    }

    public void HandleInputs()
    {
        if (!playing)
        {
            if (rb.linearVelocity != Vector2.zero) KillMovement();
            return;
        }
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

    public void RunTimer()
    {
        if (!playing) return;
        timer += Time.deltaTime;

        //Set to level time
        float timeRemaining = 300 - timer;
        UIManager.instance.GetTextElementFromDict("TimerText").text = $"{Mathf.CeilToInt(timeRemaining)}";
    }

    public void StartPlaying()
    {
        timer = 0;
        coinsCollected = 0;
        playing = true;
    }

    public void StopPlaying()
    {
        playing = false;
    }

    public float GetTime()
    {
        return timer;
    }
    public int GetCoins()
    {
        return coinsCollected;
    }

    public void KillMovement()
    {
        rb.linearVelocity = Vector2.zero;
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

    void IInvoker.CollectCoin(Tilemap _tilemap, Vector3Int _cellPos)
    {
        coinsCollected++;
        _tilemap.SetTile(_cellPos, null);
        UIManager.instance.GetTextElementFromDict("CoinText").text = $"{coinsCollected}";
    }

    bool IInvoker.IsPlayer()
    {
        return true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out Tilemap tilemap))
        {
            // Get overlapping area between the player and the tilemap trigger
            Bounds overlapBounds = GetComponent<Collider2D>().bounds;
            
            Vector3Int min = tilemap.WorldToCell(overlapBounds.min);
            Vector3Int max = tilemap.WorldToCell(overlapBounds.max);
            
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPos);

                    if (tile != null)
                    {
                        Debug.Log($"Tile touched at {cellPos}: {tile.name}");
                        TileLogic.instance.InvokeTileAction(tile, tilemap, cellPos, this);
                    }
                }
            }

            Debug.Log($"No tile found in bounds: {min} to {max}");
        }
    }
}
