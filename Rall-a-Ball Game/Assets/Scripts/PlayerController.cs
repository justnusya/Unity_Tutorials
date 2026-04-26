using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private static bool isRestarted = false;

    [Header("UI Panels")]
    public GameObject startMenu;
    public GameObject endMenu;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI countText;

    [Header("Player Settings")]
    public float speed = 10;
    public GameObject pickupEffect;
    private Renderer playerRenderer; 

    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;

    [Header("Jump Settings")]
    public float jumpForce = 4f;
    public float groundDistance = 0.6f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        count = 0;
        SetCountText();

        endMenu.SetActive(false);
        winText.gameObject.SetActive(false); 
        
        if (isRestarted)
        {
            startMenu.SetActive(false);
            countText.gameObject.SetActive(true);
            Time.timeScale = 1;
        }
        else
        {
            startMenu.SetActive(true);
            countText.gameObject.SetActive(false); 
            winText.gameObject.SetActive(true);
            winText.text = "Welcome to Roll-a-Ball Game!";
            Time.timeScale = 0;
        }
    }

    void OnJump()
    {
        bool hitGround = Physics.Raycast(transform.position, Vector3.down, groundDistance);

        if (hitGround && Time.timeScale > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void StartGame()
    {
        startMenu.SetActive(false);
        winText.gameObject.SetActive(false); 
        countText.gameObject.SetActive(true);
        Time.timeScale = 1;
        SetCountText();
    }

    public void RestartGame()
    {
        isRestarted = true;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetColorBlue() => playerRenderer.material.color = new Color(0.2f, 0.5f, 1f);
    public void SetColorPink() => playerRenderer.material.color = new Color(1f, 0.4f, 0.7f);
    public void SetColorYellow() => playerRenderer.material.color = Color.yellow;
    public void SetColorWhite() => playerRenderer.material.color = Color.white;

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            if (pickupEffect != null)
            {
                GameObject effect = Instantiate(pickupEffect, other.transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 9) ShowGameOverMenu("You Win!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) ShowGameOverMenu("You Lose!");
    }

    void ShowGameOverMenu(string message)
    {
        Time.timeScale = 0;
        countText.gameObject.SetActive(false);
        endMenu.SetActive(true);
        winText.gameObject.SetActive(true);
        winText.text = message + "\nFinal Score: " + count;
        
        GetComponent<MeshRenderer>().enabled = false;
        rb.isKinematic = true; 
    }
}