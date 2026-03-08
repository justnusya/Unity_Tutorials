using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Seconds it takes for a full day cycle")]
    public float dayDuration = 60f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // скільки градусів має обертатись світло за секунду
        float rotationSpeed = 360f / dayDuration;

        // обертання світла
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}