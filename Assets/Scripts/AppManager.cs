using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public enum Mode { AUTO,LIVE };

public class AppManager : MonoBehaviour
{
    public AutoRunwayManager autoRunwayManager;
    public LiveRunwayManager liveRunwayManager;
    public Image blackout;
    public Animator blackoutAnimator;

    private Mode curMode = Mode.AUTO;

    void Start()
    {
        blackoutAnimator.SetBool("FadeIn", false);
        blackoutAnimator.SetBool("FadeOut", false);
        autoRunwayManager.HideAllLevels();

        if(curMode == Mode.LIVE)
        {
            PlayLiveRunway();
        } else
        {
            PlayAutoRunway();
        }
        
    }

    public void PlayLiveRunway()
    {
        autoRunwayManager.StopAutoRunway();
        autoRunwayManager.HideAllLevels();
        liveRunwayManager.ReadyLiveRunway();
        StartCoroutine(FadeInLive());
    }

    public void StopLiveRunway()
    {
        //DO hard stop without any transition
        liveRunwayManager.StopLiveRunway();
    }

    public void PlayAutoRunway()
    {
        liveRunwayManager.StopLiveRunway();
        autoRunwayManager.ReadyAutoRunway(PickRandomLevel());
        StartCoroutine(FadeInLevel());
    }

    public void StopAutoRunway()
    {
        //DO hard stop without any transition
        autoRunwayManager.StopAutoRunway();
    }

    public void LiveToAuto()
    {
        if(curMode == Mode.AUTO) { return; }

        curMode = Mode.AUTO;

        StartCoroutine(FadeOutLive());
    }

    public void AutoToLive()
    {
        if (curMode == Mode.LIVE) { return; }

        curMode = Mode.LIVE;

        StartCoroutine(FadeOutLLevelToLive());
    }

    public void ChangeLevel()
    {

    }

    private void RunLiveRunway()
    {
        Debug.Log("Run live mode after fade in");
        liveRunwayManager.PlayLiveRunway();
    }

    private void RunAutoRunway()
    {
        autoRunwayManager.PlayAutoRunway();
    }

    private GameObject PickRandomLevel()
    {
        int index = Random.Range(0, autoRunwayManager.levels.Count);
        return autoRunwayManager.levels[index];
    }

    IEnumerator FadeInLevel()
    {
        blackoutAnimator.SetBool("FadeIn", true);
        yield return new WaitUntil(() => blackout.color.a == 0);
        blackoutAnimator.SetBool("FadeIn", false);
        RunAutoRunway();
    }

    IEnumerator FadeOutLLevelToLive()
    {
        blackoutAnimator.SetBool("FadeOut", true);
        yield return new WaitUntil(() => blackout.color.a == 1);
        blackoutAnimator.SetBool("FadeOut", false);
        PlayLiveRunway();
    }

    IEnumerator FadeOutLLevelToLevel()
    {
        blackoutAnimator.SetBool("FadeOut", true);
        yield return new WaitUntil(() => blackout.color.a == 1);
        blackoutAnimator.SetBool("FadeOut", false);
        PlayAutoRunway();
    }

    IEnumerator FadeInLive()
    {
        blackoutAnimator.SetBool("FadeIn", true);
        yield return new WaitUntil(() => blackout.color.a == 0);
        blackoutAnimator.SetBool("FadeIn", false);
        RunLiveRunway();
    }

    IEnumerator FadeOutLive()
    {
        blackoutAnimator.SetBool("FadeOut", true);
        yield return new WaitUntil(() => blackout.color.a == 1);
        blackoutAnimator.SetBool("FadeOut", false);
        PlayAutoRunway();
    }
}


