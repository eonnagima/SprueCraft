using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketPhysicsHandler : MonoBehaviour
{
    private XRSocketInteractor socket;
    private int noCollisionLayer;
    
    // Flag to prevent physics interference
    private bool isAttached = false;

    // Store the XRGrabInteractable for manual enable/disable
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnPartAttached);
        socket.selectExited.AddListener(OnPartDetached);

        // Store the layer index for the 'NoCollision' layer
        noCollisionLayer = LayerMask.NameToLayer("NoCollision");

        // Get the XRGrabInteractable from the attached part (assuming it has this component)
        grabInteractable = GetComponent<XRGrabInteractable>();
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
        Collider[] partColliders = part.GetComponentsInChildren<Collider>();
        foreach (var collider in partColliders)
        {
            collider.gameObject.layer = noCollisionLayer; // Set to custom 'NoCollision' layer
        }

        // Disable grab interaction while attached
        if (grabInteractable != null)
        {
            grabInteractable.interactionLayerMask = LayerMask.GetMask("None"); // Disable grabbing temporarily
        }

        isAttached = true;

        Debug.Log($"[Socket] Attached: {part.name}");
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
        Collider[] partColliders = part.GetComponentsInChildren<Collider>();
        foreach (var collider in partColliders)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("Default"); // Reset to default layer
        }

        // Enable grab interaction when detaching
        if (grabInteractable != null)
        {
            grabInteractable.interactionLayerMask = LayerMask.GetMask("Default"); // Enable grabbing again
        }

        isAttached = false;

        Debug.Log($"[Socket] Detached: {part.name}");
    }

    // Optional: Raycast-based grabbing control
    public void OnRayHover(bool isHovering)
    {
        if (grabInteractable != null && !isAttached)
        {
            grabInteractable.interactionLayerMask = isHovering ? LayerMask.GetMask("Default") : LayerMask.GetMask("None");
        }
    }
}