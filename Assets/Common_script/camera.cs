using UnityEngine;

public class camera : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    
    private Camera cam;
    private float xRotation = 0f;

    void Start()
    {
        // Find the camera attached to this player
        cam = GetComponentInChildren<Camera>();
        
        // Lock the mouse to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse movement inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Calculate up/down rotation (pitch) and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply rotation to the camera (up/down) and the player body (left/right)
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}