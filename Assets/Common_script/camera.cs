using UnityEngine;

public class camera : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    
    private Camera cam;
    private float xRotation = 0f;

    [HideInInspector]
    public float currentTilt = 0f; // Will be controlled by the WallRun script

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply rotation, including the dynamic tilt (Z-axis) from wall running
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        }
        
        transform.Rotate(Vector3.up * mouseX);
    }
}