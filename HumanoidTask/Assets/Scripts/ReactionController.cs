using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReactionController : MonoBehaviour
{
    public Button playButton;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip dialogueClip;
    public LipSyncController lipSync;
    public TextMeshProUGUI charState;

    private bool isRunning = false;
    private bool queued = false;

    void Start()
    {
        audioSource.clip = dialogueClip;
        playButton.onClick.AddListener(OnPlayPressed);
        charState.text = "Idle";
    }

    void OnPlayPressed()
    {
        if (isRunning)
        {
            queued = true;
            return;
        }
        StartCoroutine(FullSequence());
    }

    IEnumerator FullSequence()
    {
        isRunning = true;
        queued = false;

        // Reset audio
        audioSource.Stop();
        audioSource.time = 0f;

        // Start audio + lip sync FIRST
        audioSource.Play();
        lipSync.StartLipSync(audioSource);
        charState.text = "Speaking...";

        // While audio plays, run emotion sequence in parallel
        yield return StartCoroutine(RunEmotionSequence());

        // Wait until audio finishes
        while (audioSource.isPlaying)
            yield return null;

        lipSync.StopLipSync();
        charState.text = "Idle";

        isRunning = false;

        if (queued)
            StartCoroutine(FullSequence());
    }

    IEnumerator RunEmotionSequence()
    {
        // Smile
        yield return PlayAnimation("SmileTrigger", "Smile", "Smile");

        // Sad
        yield return PlayAnimation("SadTrigger", "Sad", "Sad");

        // Smile again
        yield return PlayAnimation("SmileTrigger", "Smile", "Smile");

        // Sad again
        yield return PlayAnimation("SadTrigger", "Sad", "Sad");
    }

    IEnumerator PlayAnimation(string trigger, string stateName, string stateText)
    {
        charState.text = stateText;

        animator.ResetTrigger("SmileTrigger");
        animator.ResetTrigger("SadTrigger");

        animator.SetTrigger(trigger);

        // Wait for animation to start
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        // Wait for animation duration
        float len = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(len);
    }
}
