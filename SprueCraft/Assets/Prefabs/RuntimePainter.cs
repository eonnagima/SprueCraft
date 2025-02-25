using UnityEngine;

public class RuntimePainter : MonoBehaviour {
    public Camera cam;
    public RenderTexture paintTexture;
    public Material paintMaterial;
    public Color paintColor = Color.red;
    public float brushSize = 0.05f;

    private void Update() {
        if (Input.GetMouseButton(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                Paint(hit);
            }
        }
    }

    void Paint(RaycastHit hit) {
        Vector2 uv = hit.textureCoord;
        RenderTexture.active = paintTexture;

        Texture2D brush = new Texture2D(1, 1);
        brush.SetPixel(0, 0, paintColor);
        brush.Apply();

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintTexture.width, paintTexture.height, 0);
        Graphics.DrawTexture(new Rect(uv.x * paintTexture.width - brushSize * 512, uv.y * paintTexture.height - brushSize * 512, brushSize * 1024, brushSize * 1024), brush);
        GL.PopMatrix();

        RenderTexture.active = null;
        Destroy(brush);
    }
}
