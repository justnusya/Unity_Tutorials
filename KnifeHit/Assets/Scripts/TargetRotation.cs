using UnityEngine;

public class TargetRotation : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 50f;     
    public float speedStep = 15f;    
    public float maxSpeedLimit = 500f;

    private float currentLevelSpeed;
    private bool isBoss = false;
    private float timer = 0;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        AdjustScale();
    }

    void Update()
    {
        float rotationAmount;
        if (!isBoss)
        {
            rotationAmount = currentLevelSpeed * Time.deltaTime;
        }
        else
        {
            timer += Time.deltaTime;

            float direction = Mathf.Sin(timer * 0.5f);

            rotationAmount = direction * currentLevelSpeed * Time.deltaTime;
        }
        transform.Rotate(0, 0, rotationAmount);
    }
    public void InitTarget(bool bossStatus, int targetIndex)
    {
        isBoss = bossStatus;
        currentLevelSpeed = Mathf.Min(baseSpeed + (targetIndex * speedStep), maxSpeedLimit);

        timer = 0;
    }

    public void AdjustScale()
    {
        if (sr != null && sr.sprite != null)
        {
            float targetSize = 3.5f;
            float scaleFactor = targetSize / sr.sprite.bounds.size.x;
            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
    }
}