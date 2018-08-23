using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Mode { AUTO,LIVE };

//[RequireComponent(typeof(AudioSource))]
public class AppManager : Singleton<AppManager>
{
    public AutoRunwayManager autoRunwayManager;
    public LiveRunwayManager liveRunwayManager;
    public Image blackout;
    public Animator blackoutAnimator;
    public AudioSource music;
    public GameObject stickman;
    public GameObject poseAcademy;

    private Mode curMode = Mode.AUTO;

    private List<string> playList = new List<string>(new string[] { "dream", "5min" });
    private int curSong = 0;

    void Start()
    {
        stickman.SetActive(false);
        blackoutAnimator.SetBool("FadeIn", false);
        blackoutAnimator.SetBool("FadeOut", false);

        MRData.Instance.LoadEverything();

        autoRunwayManager.HideAllLevels();
       
        if (curMode == Mode.LIVE)
        {
            PlayLiveRunway();
        } else
        {
            PlayAutoRunway();
        }
        
    }

    public Mode getMode()
    {
        return curMode;
    }

    public void PlayLiveRunway()
    {
        poseAcademy.SetActive(true);
        UserManager.Instance.initPoseDetection();
        stickman.SetActive(true);
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
        //    liveRunwayManager.StopLiveRunway();
        StartCoroutine(ContinuousPlayMusic());

        //Debug.Log("started");
        poseAcademy.SetActive(false);
        stickman.SetActive(false);
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
        Debug.Log("AppManager: Live to Auto");
        if(curMode == Mode.AUTO) { return; }

        curMode = Mode.AUTO;

        StartCoroutine(FadeOutLive());
    }

    public void AutoToLive()
    {
        if (curMode == Mode.LIVE) { return; }

        curMode = Mode.LIVE;
        //music.Stop();
        IEnumerator fader = AudioFader.FadeOut(music, 3.0f);
        StartCoroutine(fader);

        StartCoroutine(FadeOutLevelToLive());
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

    IEnumerator FadeOutLevelToLive()
    {
        blackoutAnimator.SetBool("FadeOut", true);
        yield return new WaitUntil(() => blackout.color.a == 1);
        blackoutAnimator.SetBool("FadeOut", false);
        PlayLiveRunway();
    }

    IEnumerator FadeOutLevelToLevel()
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

    IEnumerator ContinuousPlayMusic()
    {
        AudioClip clip = SfxManager.LoadClip(playList[curSong]);
        music.volume = 0.8f;
        music.clip = clip;
        music.Play();
        yield return new WaitForSeconds(music.clip.length);
        curSong++;
        if(curSong == playList.Count)
        {
            curSong = 0;
        }
        AudioClip nextClip = SfxManager.LoadClip(playList[curSong]);
        music.clip = clip;
        music.Play();
    }

    public IEnumerator ShouldRestartScene()
    {
        Debug.Log("No Users, wait another 10 seconds for new user or else go back to AutoRunway");
        yield return new WaitForSeconds(10);

        StartCoroutine(UserManager.Instance.getNumberofUsers(RestartScene));
    }

    public void RestartScene(int numUsers)
    {
        if (numUsers == 0)
        {
            Debug.Log("Restarting Scene: Going back to AutoRunway!!");
            Scene loadedLevel = SceneManager.GetActiveScene();
            SceneManager.LoadScene(loadedLevel.buildIndex);
        }
    }
}


