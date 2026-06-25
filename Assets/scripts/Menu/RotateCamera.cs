using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [Header("Orbit Targets")]
    public Transform lighthouseTarget;    // Drag your Lighthouse object here

    [Header("Orbit Dimensions")]
    public float radius = 20f;            // Distance away from the lighthouse
    public float cameraHeight = 15f;      // How high above the lighthouse the camera sits

    [Header("Movement")]
    public float speed = 10f;             // Speed of rotation

    [Header("Camera Aim")]
    [Tooltip("0 = looks at lighthouse base. Increase this to look higher up the tower.")]
    public float lookAtHeightOffset = 5f; 

    private float currentAngle = 0f;

    void Start()
    {
        if (lighthouseTarget == null)
        {
            Debug.LogError("Please assign a Lighthouse Target to the RotateCamera script!");
        }
    }

    void Update()
    {
        if (lighthouseTarget == null) return;

        // 1. Calculate the angle over time
        currentAngle += speed * Time.deltaTime;

        // 2. Calculate the X and Z circle coordinates
        float x = lighthouseTarget.position.x + Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius;
        float z = lighthouseTarget.position.z + Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius;
        
        // 3. Set the Y position using our new cameraHeight variable
        float y = lighthouseTarget.position.y + cameraHeight;

        // 4. Apply the position to the camera
        transform.position = new Vector3(x, y, z);

        // 5. Point the camera down at the lighthouse (adjusted by the lookAtHeightOffset)
        Vector3 targetLookPoint = lighthouseTarget.position + (Vector3.up * lookAtHeightOffset);
        transform.LookAt(targetLookPoint); 
    }
}