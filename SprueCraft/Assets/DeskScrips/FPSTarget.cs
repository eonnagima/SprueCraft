using UnityEngine;

public class FPSTarget : MonoBehaviour
{
    public int targetFrameRate = 30;
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }
}