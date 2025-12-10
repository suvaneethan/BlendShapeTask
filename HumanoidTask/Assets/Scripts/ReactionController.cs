using System.Collections;
using TMPro;
using UnityChan;
using UnityEngine;
using UnityEngine.UI;

public class ReactionController : MonoBehaviour
{
    [Header("UI / Controls")]
    public Button playButton;
    public TextMeshProUGUI charState;

    [Header("Animation")]
    public Animator animator;

    [Header("Audio / LipSync")]
    public AudioSource audioSource;
    public AudioClip dialogueClip;
    public LipSyncController lipSync;

    [Header("UnityChan Face System")]
    public FaceUpdate faceUpdate;

    // USE REAL NAMES FROM YOUR PROJECT
    string faceDefault = "default@unitychan";
    string faceHappy = "conf@unitychan";             // closest happy animation
    string faceSad = "ASHAMED";                     // sad/upset expression
    string faceSoftSad = "disstract1@unitychan";    // mild sad/low emotion

    [Header("Optional Blendshape Reset")]
    public SkinnedMeshRenderer faceMesh;

    private bool isRunning = false;
    private bool queued = false;

    // Audio emotion timing
    readonly float happyA_end = 1.40f;
    readonly float sadA_end = 3.20f;
    readonly float happyB_end = 5.20f;
    readonly float softSad_end = 7.50f;

    void Start()
    {
        if (dialogueClip != null)
            audioSource.clip = dialogueClip;

        playButton.onClick.AddListener(OnPlayPressed);

        charState.text = "Idle";

        ResetFaceLayer();
    }

    void OnPlayPressed()
    {
        if (isRunning)
        {
            queued = true;
            return;
        }
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        isRunning = true;
        queued = false;

        animator.Play("Idle", 0, 0);
        animator.Update(0);

        ResetFaceLayer();
        ClearBlendshapes();

        audioSource.Stop();
        audioSource.time = 0f;

        // Start audio + lip sync
        audioSource.Play();
        lipSync.StartLipSync(audioSource);

        charState.text = "Speaking...";

        yield return StartCoroutine(EmotionTimeline());

        while (audioSource.isPlaying)
            yield return null;

        lipSync.StopLipSync();
        ResetFaceLayer();
        ClearBlendshapes();

        charState.text = "Idle";

        isRunning = false;

        if (queued)
        {
            queued = false;
            StartCoroutine(PlayRoutine());
        }
    }

    // ---------------- Emotion Timeline ------------------

    IEnumerator EmotionTimeline()
    {
        float start = Time.time;
        float Elapsed() => Time.time - start;

        // HAPPY
        ApplyHappyFace();
        animator.SetTrigger("SmileTrigger");
        while (Elapsed() < happyA_end) yield return null;

        // SAD
        ApplySadFace();
        animator.SetTrigger("SadTrigger");
        while (Elapsed() < sadA_end) yield return null;

        // HAPPY
        ApplyHappyFace();
        animator.SetTrigger("SmileTrigger");
        while (Elapsed() < happyB_end) yield return null;

        // SOFT SAD
        ApplySoftSadFace();
        animator.SetTrigger("SadTrigger");
        while (Elapsed() < softSad_end) yield return null;

        // NEUTRAL
        ResetFaceLayer();
    }

    // ---------------- Facial Expression Controls ------------------

    void ApplyHappyFace()
    {
        ResetFaceLayer();
        faceUpdate.OnCallChangeFace(faceHappy);
        charState.text = "Happy 😊";
    }

    void ApplySadFace()
    {
        ResetFaceLayer();
        faceUpdate.OnCallChangeFace(faceSad);
        charState.text = "Sad 😢";
    }

    void ApplySoftSadFace()
    {
        ResetFaceLayer();
        faceUpdate.OnCallChangeFace(faceSoftSad);
        charState.text = "Soft Sad 😔";
    }

    void ResetFaceLayer()
    {
        if (faceUpdate == null) return;

        faceUpdate.isKeepFace = false;
        animator.SetLayerWeight(1, 0f);

        faceUpdate.OnCallChangeFace(faceDefault);
    }

    // ---------------- Blendshape Cleanup ------------------

    void ClearBlendshapes()
    {
        if (faceMesh == null) return;

        int count = faceMesh.sharedMesh.blendShapeCount;
        for (int i = 0; i < count; i++)
            faceMesh.SetBlendShapeWeight(i, 0);
    }
}
