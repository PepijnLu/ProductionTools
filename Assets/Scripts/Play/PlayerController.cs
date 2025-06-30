using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour, IInvoker
{
    public bool playing;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [SerializeField] private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    [SerializeField] Animator animator;
    bool facingLeft;
    int timeToComplete;
    Vector2 startPosition;

    //Stats
    int maxHealth;
    int health;
    int coinsCollected;
    float timer;

    void Start()
    {
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
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

        Debug.Log($"linVelX: {rb.linearVelocity.x}");

        if((rb.linearVelocity.x < -0.1f) && (!facingLeft))
        {
            facingLeft = true;
            Vector3 _scale = transform.localScale;
            _scale.x = Mathf.Abs(_scale.x);
            transform.localScale = _scale;
        }
        if((rb.linearVelocity.x > 0.1f) && facingLeft)
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
        float timeRemaining = timeToComplete - timer;
        UIManager.instance.GetTextElementFromDict("TimerText").text = $"{Mathf.CeilToInt(timeRemaining)}";

        if(timeRemaining <= 0) Die();
    }

    public void StartPlaying()
    {
        timeToComplete = SaveAndLoad.instance.LevelData.timeToComplete;
        maxHealth = SaveAndLoad.instance.LevelData.maxPlayerHealth;
        health = maxHealth;
        startPosition = new Vector2(SaveAndLoad.instance.LevelData.playerStartX, SaveAndLoad.instance.LevelData.playerStartY);
        timer = 0;
        coinsCollected = 0;
        playing = true;
    }

    public void StopPlaying()
    {
        transform.position = startPosition;
        health = maxHealth;
        UIManager.instance.DisplayHealthFromInt(health, true);
        UIManager.instance.GetTextElementFromDict("TimerText").text = $"{timeToComplete}";
        animator.SetBool("isDead", false);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        facingLeft = false;
        Vector3 _scale = transform.localScale;
        _scale.x = -Mathf.Abs(_scale.x);
        transform.localScale = _scale;
        
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

    public void Die()
    {
        playing = false;
        animator.SetBool("isDead", true);
        moveInput = 0;
        KillMovement();

        if(facingLeft) transform.rotation = Quaternion.Euler(0, 0, -90);
        else transform.rotation = Quaternion.Euler(0, 0, 90);

        if(SceneData.loadBehaviour == "Clear")
        {
            UIManager.instance.ToggleUIElement("ClearDeathScreen", true);
        }
        else if(SceneData.loadBehaviour == "Play")
        {
            UIManager.instance.ToggleUIElement("PlayDeathScreen", true);
        }
        else throw new Exception($"Player died in unknown behaviour: {SceneData.loadBehaviour}");
    }

    void FixedUpdate()
    {
        // Move the player
        float xForce = moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(xForce, rb.linearVelocity.y);
        UIManager.instance.MoveBackground(xForce);

        animator.SetFloat("velocity", Mathf.Abs(rb.linearVelocity.x));

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if(transform.position.y < -1) Die();
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

    void IInvoker.TakeDamage(int _value)
    {
        health -= _value;
        UIManager.instance.DisplayHealthFromInt(health, false);

        if(health <= 0)
        {
            Die();
        }
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
