using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

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

    // UI elements for VR interaction
    public Slider brushSizeSlider;
    public Slider redSlider; // Slider for Red component
    public Slider greenSlider; // Slider for Green component
    public Slider blueSlider; // Slider for Blue component

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

        // Initialize UI elements for brush customization
        InitializeUI();
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

        if (rightController != null && rightController.selectAction.action != null && rightController.selectAction.action.ReadValue<float>() > 0.1f)
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

        int x = (int)(uv.x * texture.width);
        int y = (int)(uv.y * texture.height);

        int brushRadius = Mathf.CeilToInt(brushSize * texture.width);
        for (int i = -brushRadius; i < brushRadius; i++)
        {
            for (int j = -brushRadius; j < brushRadius; j++)
            {
                int brushX = Mathf.Clamp(x + i, 0, texture.width - 1);
                int brushY = Mathf.Clamp(y + j, 0, texture.height - 1);
                texture.SetPixel(brushX, brushY, paintColor);
            }
        }

        texture.Apply();
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

    // Initialize UI elements for brush customization
    private void InitializeUI()
    {
        if (brushSizeSlider != null)
        {
            brushSizeSlider.value = brushSize;
            brushSizeSlider.onValueChanged.AddListener(SetBrushSize);
        }

        if (redSlider != null)
        {
            redSlider.value = paintColor.r;
            redSlider.onValueChanged.AddListener(SetRedValue);
        }

        if (greenSlider != null)
        {
            greenSlider.value = paintColor.g;
            greenSlider.onValueChanged.AddListener(SetGreenValue);
        }

        if (blueSlider != null)
        {
            blueSlider.value = paintColor.b;
            blueSlider.onValueChanged.AddListener(SetBlueValue);
        }
    }

    // Set brush size
    public void SetBrushSize(float newSize)
    {
        brushSize = Mathf.Max(0.001f, newSize); // Prevent zero or negative size
    }

    // Set Red component of the color
    public void SetRedValue(float value)
    {
        paintColor.r = value;
        UpdateBrushColor();
    }

    // Set Green component of the color
    public void SetGreenValue(float value)
    {
        paintColor.g = value;
        UpdateBrushColor();
    }

    // Set Blue component of the color
    public void SetBlueValue(float value)
    {
        paintColor.b = value;
        UpdateBrushColor();
    }

    // Update the brush color
    private void UpdateBrushColor()
    {
        Debug.Log("Updated brush color: " + paintColor);
    }
}