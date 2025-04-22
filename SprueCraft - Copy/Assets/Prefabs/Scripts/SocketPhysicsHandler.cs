using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketPhysicsHandler : MonoBehaviour
{
    private XRSocketInteractor socket;

    // Custom layer to disable collisions during attachment
    private int noCollisionLayer;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnPartAttached);
        socket.selectExited.AddListener(OnPartDetached);

        // Store the layer index for the 'NoCollision' layer
        noCollisionLayer = LayerMask.NameToLayer("NoCollision");
    }

    private void OnPartAttached(SelectEnterEventArgs args)
    {
        GameObject attachedObject = args.interactableObject.transform.gameObject;
        HandleAttachment(attachedObject);
    }

    private void OnPartDetached(SelectExitEventArgs args)
    {
        GameObject detachedObject = args.interactableObject.transform.gameObject;
        HandleDetachment(detachedObject);
    }

    private void HandleAttachment(GameObject part)
    {
        // Zero out motion on the attached object
        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Freeze physics motion temporarily
            rb.useGravity = false;  // Disable gravity to prevent falling
        }

        // Temporarily disable collisions by setting the layer to "NoCollision" for both parts
        Collider[] headColliders = part.GetComponentsInChildren<Collider>();
        foreach (var collider in headColliders)
        {
            collider.gameObject.layer = noCollisionLayer; // Set to custom 'NoCollision' layer
            Debug.Log($"[Socket] Collider layer set to: {collider.gameObject.layer}");
        }

        // Prevent grabbing the attached part by disabling its collider temporarily
        foreach (var collider in headColliders)
        {
            collider.enabled = false; // Disable the collider during attachment to prevent interaction
        }

        Debug.Log($"[Socket] Attached and motion frozen: {part.name}");
    }

    private void HandleDetachment(GameObject part)
    {
        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;  // Re-enable physics interactions
            rb.useGravity = true;    // Re-enable gravity

            // Reset velocity and angular velocity on detachment to avoid jitter
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Re-enable colliders and revert the layer to the default layer
        Collider[] colliders = part.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("Default"); // Reset to default layer
            collider.enabled = true; // Re-enable the collider for interactions
            Debug.Log($"[Socket] Collider layer reset to: {collider.gameObject.layer}");
        }

        Debug.Log($"[Socket] Detached: {part.name}");
    }
}
