using UnityEngine;

public class Knife : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 25f;
    private bool isThrown = false;
    private bool isStuck = false; 
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isThrown && !isStuck)
        {
            ThrowKnife();
        }
    }

    void ThrowKnife()
    {
        isThrown = true;
        rb.linearVelocity = Vector2.up * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStuck) return;

        if (collision.gameObject.CompareTag("Target"))
        {
            StickToTarget(collision.transform);
        }
        else if (collision.gameObject.CompareTag("Knife"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f); 
            FindObjectOfType<GameManager>().GameOver();
        }
    }

    void StickToTarget(Transform targetTransform)
    {
        isStuck = true; 

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;

        
        transform.SetParent(targetTransform);

        gameObject.tag = "Knife";

        FindObjectOfType<GameManager>().OnHit();
    }
}