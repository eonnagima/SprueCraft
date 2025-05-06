using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RuntimePainter : MonoBehaviour
{
    public Camera vrCamera; // Reference to the VR camera
    public LayerMask paintableLayer; // Layer mask for "Paintable" objects
    public RenderTexture paintTexture; // RenderTexture for painting
    public Color brushColor = Color.red; // Default brush color
    public float brushSize = 0.1f; // Default brush size

    private Material paintMaterial; // Material used for painting
    private int colorIndex = 0; // Index to cycle through colors
    public ActionBasedController rightHandController; // Reference to the right-hand Action-Based Controller

    private readonly Color[] colors = { Color.red, Color.green, Color.blue }; // Available colors

    void Start()
    {
        // Initialize the paint material
        if (paintTexture != null)
        {
            paintMaterial = new Material(Shader.Find("Custom/RuntimePaintShader"));
            paintMaterial.mainTexture = paintTexture;
        }
        else
        {
            Debug.LogError("RenderTexture is not assigned. Please assign a RenderTexture in the inspector.");
        }
    }

    void Update()
    {
        // Check if the right-hand controller is assigned
        if (rightHandController != null)
        {
            // Check if the trigger button is pressed
            if (rightHandController.activateAction.action?.triggered == true)
            {
                Paint();
            }

            // Check if the grip button is pressed
            if (rightHandController.selectAction.action?.triggered == true)
            {
                CycleBrushColor();
            }
        }
        else
        {
            Debug.LogWarning("Right-hand controller is not assigned.");
        }
    }

    void Paint()
    {
        // Cast a ray from the VR camera's forward direction (simulating XR Simulator input)
        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);

        Debug.Log("Ray Origin: " + ray.origin + ", Direction: " + ray.direction);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2.0f); // Visualize the ray

        // Increase the raycast distance to 1000 units
        float raycastDistance = 1000f;

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, paintableLayer))
        {
            Debug.Log("Hit Object: " + hit.collider.gameObject.name);

            // Check for Renderer component
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer == null)
            {
                // Try to find the Renderer on the parent object
                renderer = hit.collider.GetComponentInParent<Renderer>();
            }

            if (renderer != null)
            {
                Debug.Log("Material Shader: " + renderer.material.shader.name);

                // Get UV coordinates of the hit point
                Vector2 uv = hit.textureCoord;
                Debug.Log("UV Coordinates: " + uv);

                if (uv == Vector2.zero)
                {
                    Debug.LogWarning("UV coordinates are (0.00, 0.00). Check the object's UV mapping and collider setup.");
                    return;
                }

                // Set brush properties
                paintMaterial.SetColor("_Color", brushColor);
                paintMaterial.SetFloat("_BrushSize", brushSize);
                Debug.Log("Brush Size: " + brushSize);

                // Paint on the texture
                Debug.Log("Setting Render Target...");
                if (paintTexture == null)
                {
                    Debug.LogError("RenderTexture is not assigned.");
                    return;
                }

                Graphics.SetRenderTarget(paintTexture);
                GL.PushMatrix();
                GL.LoadOrtho();
                paintMaterial.SetPass(0);
                GL.Begin(GL.QUADS);
                GL.Color(brushColor);
                GL.TexCoord2(uv.x - brushSize, uv.y - brushSize); GL.Vertex3(0, 0, 0);
                GL.TexCoord2(uv.x + brushSize, uv.y - brushSize); GL.Vertex3(1, 0, 0);
                GL.TexCoord2(uv.x + brushSize, uv.y + brushSize); GL.Vertex3(1, 1, 0);
                GL.TexCoord2(uv.x - brushSize, uv.y + brushSize); GL.Vertex3(0, 1, 0);
                GL.End();
                GL.PopMatrix();
                Graphics.SetRenderTarget(null);
                Debug.Log("Render Target Cleared.");
            }
            else
            {
                Debug.LogWarning("The object hit by the raycast does not have a Renderer component.");
            }
        }
        else
        {
            Debug.LogWarning("No object hit by the ray.");
        }
    }

    void CycleBrushColor()
    {
        colorIndex = (colorIndex + 1) % colors.Length;
        brushColor = colors[colorIndex];
        Debug.Log("Brush color changed to: " + brushColor);
    }
}