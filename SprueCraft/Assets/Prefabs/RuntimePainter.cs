using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RuntimePainter : MonoBehaviour
{
    public Camera cam;
    public RenderTexture paintTexture;
    public Material paintMaterial;
    public Color paintColor = Color.red;
    public float brushSize = 0.01f; // Adjust brush size

    private Texture2D canvasTexture;
    private Texture2D brushTexture;

    public XRController leftController;
    public XRController rightController;

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

        // Assign the RenderTexture to the shader's _PaintTex
        paintMaterial.SetTexture("_PaintTex", paintTexture);

        Debug.Log("Runtime Painter initialized successfully.");
    }

    void Update()
    {
        if (leftController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerPressed) && leftTriggerPressed)
        {
            Ray ray = new Ray(leftController.transform.position, leftController.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ApplyPaint(hit);
            }
        }

        if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerPressed) && rightTriggerPressed)
        {
            Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ApplyPaint(hit);
            }
        }
    }

    void ApplyPaint(RaycastHit hit)
    {
        if (canvasTexture == null)
        {
            Debug.LogError("Canvas Texture is not initialized!");
            return;
        }

        if (paintTexture == null)
        {
            Debug.LogError("Paint Texture is missing!");
            return;
        }

        Vector2 uv = hit.textureCoord;

        // Ensure UV coordinates are valid
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            Debug.LogWarning("Invalid UV coordinates detected!");
            return;
        }

        int x = (int)(uv.x * canvasTexture.width);
        int y = (int)(uv.y * canvasTexture.height);

        for (int i = -8; i < 8; i++)
        {
            for (int j = -8; j < 8; j++)
            {
                int brushX = Mathf.Clamp(x + i, 0, canvasTexture.width - 1);
                int brushY = Mathf.Clamp(y + j, 0, canvasTexture.height - 1);
                canvasTexture.SetPixel(brushX, brushY, paintColor);
            }
        }

        canvasTexture.Apply();
        Graphics.Blit(canvasTexture, paintTexture);
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