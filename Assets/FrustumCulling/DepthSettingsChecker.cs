using UnityEngine;

public class DepthSettingsChecker : MonoBehaviour
{
    void Start()
    {
        bool isReverseZ = UnityEngine.XR.XRSettings.enabled && UnityEngine.XR.XRSettings.stereoRenderingMode == UnityEngine.XR.XRSettings.StereoRenderingMode.SinglePass;

        Debug.Log("Is unity_reverse_z enabled: " + isReverseZ);
    }
}