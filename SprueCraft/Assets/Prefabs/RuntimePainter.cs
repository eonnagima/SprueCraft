using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RuntimePainter : MonoBehaviour
{
    public Camera cam;
    public RenderTexture paintTexture;
    public Material paintMaterial; // Assign this material in the Inspector
    public Color paintColor = Color.red;
    public float brushSize = 0.01f; // Adjust brush size

    private Texture2D canvasTexture;
    private Texture2D brushTexture;

    public ActionBasedController leftController;
    public ActionBasedController rightController;

    void Start()
    {
        if (paintTexture == null)
        {
            Debug.LogError("Paint Texture is missing! Assign a Render Texture in the Inspector.");
            return;
        }

        // Ensure the Render Texture is active before use
        RenderTexture.active = paintTexture;

        // Create a blank canvas texture matching Render Texture size
        canvasTexture = new Texture2D(paintTexture.width, paintTexture.height, TextureFormat.RGBA32, false);
        ClearCanvas();

        // Ensure paintMaterial is assigned
        if (paintMaterial == null)
        {
            Debug.LogError("Paint Material is missing! Assign a material in the Inspector.");
            return;
        }

        Debug.Log("Runtime Painter initialized successfully.");

        // Disable the "XR Device Simulator UI(Clone)" canvas if VR is enabled
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name == "XR Device Simulator UI(Clone)" && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas.gameObject.SetActive(false);
                Debug.Log("Disabled XR Device Simulator UI(Clone) canvas to avoid rendering cost in VR.");
            }
        }

        // Create paintable textures for each object with a Renderer component
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material newMaterial = new Material(renderer.sharedMaterial);
            Texture2D newTexture = new Texture2D(paintTexture.width, paintTexture.height, TextureFormat.RGBA32, false);
            ClearTexture(newTexture);
            newMaterial.SetTexture("_PaintTex", newTexture);
            renderer.material = newMaterial;
        }
    }

    void Update()
    {
        if (leftController != null && leftController.selectAction.action != null && leftController.selectAction.action.ReadValue<float>() > 0.1f)
        {
            Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ApplyPaint(hit);
            }
        }
        else
        {
            Debug.LogWarning("Left controller is not assigned or not valid.");
        }

        if (rightController != null && rightController.selectAction.action != null && rightController.selectAction.action.ReadValue<float>() > 0.1f)
        {
            Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ApplyPaint(hit);
            }
        }
        else
        {
            Debug.LogWarning("Right controller is not assigned or not valid.");
        }
    }

    void ApplyPaint(RaycastHit hit)
    {
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Hit object does not have a Renderer component!");
            return;
        }

        Material material = renderer.material;
        Texture2D texture = material.GetTexture("_PaintTex") as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Hit object does not have a paintable texture!");
            return;
        }

        Vector2 uv = hit.textureCoord;

        // Ensure UV coordinates are valid
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            Debug.LogWarning("Invalid UV coordinates detected!");
            return;
        }

        int x = (int)(uv.x * texture.width);
        int y = (int)(uv.y * texture.height);

        for (int i = -8; i < 8; i++)
        {
            for (int j = -8; j < 8; j++)
            {
                int brushX = Mathf.Clamp(x + i, 0, texture.width - 1);
                int brushY = Mathf.Clamp(y + j, 0, texture.height - 1);
                texture.SetPixel(brushX, brushY, paintColor);
            }
        }

        texture.Apply();
        Debug.Log("Applied paint at UV: " + uv);
    }

    public void ClearCanvas()
    {
        if (canvasTexture == null) return;

        Color[] clearColors = new Color[canvasTexture.width * canvasTexture.height];
        for (int i = 0; i < clearColors.Length; i++)
            clearColors[i] = Color.clear;

        canvasTexture.SetPixels(clearColors);
        canvasTexture.Apply();
        Graphics.Blit(canvasTexture, paintTexture);
    }

    private void ClearTexture(Texture2D texture)
    {
        Color[] clearColors = new Color[texture.width * texture.height];
        for (int i = 0; i < clearColors.Length; i++)
            clearColors[i] = Color.clear;

        texture.SetPixels(clearColors);
        texture.Apply();
    }

    // Clears paint when the game stops in the Editor
    void OnApplicationQuit()
    {
        ResetPaintTexture();
    }

#if UNITY_EDITOR
    void OnDisable()
    {
        if (!Application.isPlaying)
        {
            ResetPaintTexture();
        }
    }
#endif

    private void ResetPaintTexture()
    {
        Debug.Log("Resetting paint texture...");
        ClearCanvas();
    }
}