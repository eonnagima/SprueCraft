using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RuntimePainter : MonoBehaviour
{
    public Camera cam;
    public RenderTexture paintTexture; // Used as a template for creating new textures
    public Material paintMaterial; // Assign this material in the Inspector
    public Color paintColor = Color.red;
    public float brushSize = 0.01f; // Adjust brush size

    private Texture2D canvasTexture;

    public InputActionProperty leftTriggerAction;
    public InputActionProperty rightTriggerAction;

    private bool leftTriggerPreviouslyPressed = false;
    private bool rightTriggerPreviouslyPressed = false;

    void Start()
    {
        if (paintTexture == null)
        {
            Debug.LogError("Paint Texture is missing! Assign a Render Texture in the Inspector.");
            return;
        }

        // Create a blank canvas texture matching Render Texture size
        canvasTexture = new Texture2D(paintTexture.width, paintTexture.height, TextureFormat.RGBA32, false);

        Debug.Log("Runtime Painter initialized successfully.");
    }

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        // Left trigger input
        if (leftTriggerAction != null)
        {
            bool leftTriggerPressed = leftTriggerAction.action.ReadValue<float>() > 0.1f;

            if (leftTriggerPressed && !leftTriggerPreviouslyPressed)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    ApplyPaint(hit);
                }
            }

            leftTriggerPreviouslyPressed = leftTriggerPressed;
        }

        // Right trigger input
        if (rightTriggerAction != null)
        {
            bool rightTriggerPressed = rightTriggerAction.action.ReadValue<float>() > 0.1f;

            if (rightTriggerPressed && !rightTriggerPreviouslyPressed)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    ApplyPaint(hit);
                }
            }

            rightTriggerPreviouslyPressed = rightTriggerPressed;
        }
    }

    void ApplyPaint(RaycastHit hit)
    {
        // Get the Renderer component from the hit object
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Hit object does not have a Renderer component!");
            return;
        }

        // Ensure the object has a unique material instance
        Material material = renderer.material;

        // Check if the material has the '_PaintTex' property
        if (!material.HasProperty("_PaintTex"))
        {
            Debug.LogError($"Material on object '{hit.collider.name}' does not have a '_PaintTex' property!");
            return;
        }

        // Check if the material already has a unique RenderTexture
        RenderTexture objectTexture = material.GetTexture("_PaintTex") as RenderTexture;
        if (objectTexture == null)
        {
            // Create a new RenderTexture for this object
            objectTexture = new RenderTexture(paintTexture.width, paintTexture.height, 0, RenderTextureFormat.ARGB32);
            objectTexture.Create();

            // Assign the new RenderTexture to the material
            material.SetTexture("_PaintTex", objectTexture);

            Debug.Log($"Created a new RenderTexture for object '{hit.collider.name}'.");
        }

        // Use the object's unique RenderTexture for painting
        RenderTexture.active = objectTexture;

        // Get the UV coordinates of the hit point
        Vector2 uv = hit.textureCoord;

        // Convert UV coordinates to pixel coordinates
        int x = (int)(uv.x * canvasTexture.width);
        int y = (int)(uv.y * canvasTexture.height);

        // Paint a circular area around the hit point
        int brushRadius = Mathf.CeilToInt(brushSize * canvasTexture.width);
        for (int i = -brushRadius; i < brushRadius; i++)
        {
            for (int j = -brushRadius; j < brushRadius; j++)
            {
                int brushX = Mathf.Clamp(x + i, 0, canvasTexture.width - 1);
                int brushY = Mathf.Clamp(y + j, 0, canvasTexture.height - 1);

                if (Vector2.Distance(new Vector2(x, y), new Vector2(brushX, brushY)) <= brushRadius)
                {
                    canvasTexture.SetPixel(brushX, brushY, paintColor);
                }
            }
        }

        canvasTexture.Apply();

        // Copy the updated canvas texture to the object's RenderTexture
        Graphics.Blit(canvasTexture, objectTexture);

        RenderTexture.active = null;
    }
}
