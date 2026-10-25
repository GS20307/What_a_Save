using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Target")]
    public Transform destinationPad; // Drag the matching destination pad here

    // Static flag to prevent instant re-trigger loops between pairs
    private static bool isTeleporting = false;

    void Start()
    {
        isTeleporting = false;
        Debug.Log(name + " trigger bounds: " + GetComponent<Collider>().bounds);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit: " + other.name +
              " | isTeleporting=" + isTeleporting +
              " | dest=" + (destinationPad != null ? destinationPad.name : "NONE"));

        if (isTeleporting) return;

        // Check if the object entering the pad is the Player
        if (other.CompareTag("Player") && destinationPad != null)
        {
            StartCoroutine(PerformTeleport(other.gameObject));
        }
    }

    System.Collections.IEnumerator PerformTeleport(GameObject player)
    {
        isTeleporting = true;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        Vector3 targetPosition = destinationPad.position + Vector3.up * 1.2f;

        Debug.Log("Teleporting from " + player.transform.position + " to " + targetPosition);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.position = targetPosition;
        }
        player.transform.position = targetPosition;
        Physics.SyncTransforms();

        yield return new WaitForSeconds(0.4f);
        isTeleporting = false;
    }
}