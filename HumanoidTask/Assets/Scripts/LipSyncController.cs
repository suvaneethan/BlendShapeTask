using UnityEngine;

/// <summary>
/// Audio-reactive lip sync using spectrum (GetSpectrumData).
/// Attach to any GameObject; assign faceMesh (SkinnedMeshRenderer) and mouthBlendshapeIndex.
/// Call StartLipSync(audioSource) and StopLipSync() from ReactionController.
/// </summary>
public class LipSyncController : MonoBehaviour
{
    [Tooltip("SkinnedMeshRenderer that contains mouth blendshapes (e.g., MTH_DEF)")]
    public SkinnedMeshRenderer faceMesh;

    [Tooltip("Blendshape index for mouth open (e.g., MTH_A index)")]
    public int mouthBlendshapeIndex = 6;

    [Tooltip("Multiplier to adjust sensitivity to the audio (50-120 typical)")]
    public float sensitivity = 65f;

    [Tooltip("Smoothing speed for mouth value")]
    public float smoothSpeed = 12f;

    private AudioSource audioSource;
    private float[] samples = new float[128];
    private float currentValue = 0f;
    private bool active = false;

    public void StartLipSync(AudioSource src)
    {
        if (src == null || faceMesh == null || mouthBlendshapeIndex < 0)
        {
            Debug.LogWarning("LipSyncController: missing setup (audioSource/faceMesh/blendshapeIndex).");
            return;
        }

        audioSource = src;
        active = true;
        enabled = true;
    }

    public void StopLipSync()
    {
        active = false;
        if (faceMesh != null && mouthBlendshapeIndex >= 0)
            faceMesh.SetBlendShapeWeight(mouthBlendshapeIndex, 0f);
        enabled = false;
    }

    void Update()
    {
        if (!active || audioSource == null || !audioSource.isPlaying) return;

        // fetch spectrum data
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        // find peak in low-mid frequency bands (speech energy)
        float peak = 0f;
        int maxIndex = samples.Length / 4; // analyze lower quarter first (voice)
        for (int i = 0; i < maxIndex; i++)
            if (samples[i] > peak) peak = samples[i];

        // convert to blendshape weight
        float target = Mathf.Clamp01(peak * sensitivity) * 100f;

        // smooth interpolation
        currentValue = Mathf.Lerp(currentValue, target, Time.deltaTime * smoothSpeed);
        faceMesh.SetBlendShapeWeight(mouthBlendshapeIndex, currentValue);
    }
}
