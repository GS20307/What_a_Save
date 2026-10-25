using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Target")]
    public Transform destinationPad; // Drag the matching destination pad here

    // Static flag to prevent instant re-trigger loops between pairs
    private static bool isTeleporting = false;

    void OnTriggerEnter(Collider other)
    {
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

        // Clear physics velocity so momentum doesn't fling the player into a wall upon arrival
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Move player to the destination pad's position (plus a slight upward offset so they don't clip into the floor)
        Vector3 targetPosition = destinationPad.position + Vector3.up * 0.5f;

        if (rb != null)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            player.transform.position = targetPosition;
        }

        // Wait a brief moment to let the player exit the trigger zone before resetting the lock
        yield return new WaitForSeconds(0.4f);
        isTeleporting = false;
    }
}