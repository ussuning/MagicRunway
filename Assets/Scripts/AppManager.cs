using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Mode { AUTO,LIVE };

public class AppManager : Singleton<AppManager>
{
    public AutoRunwayManager autoRunwayManager;
    public LiveRunwayManager liveRunwayManager;
    public Image blackout;
    public Animator blackoutAnimator;
    public AudioSource audioSource;

    private Mode curMode = Mode.AUTO;

    private IRunwayMode currentMode;
    private IRunwayMode nextMode;

    private List<string> playList = new List<string>(new string[] { "dream", "5min" });
    private int curSong = 0;
    private float maxSongVolume = 0.8f;
    private float minSongVolume = 0.2f;
    private float reduceSongVolumeTime = 1.0f;
    private byte songVolumeState;

    void Start()
    {
        MRData.Instance.LoadEverything();
        
        currentMode = autoRunwayManager;
        SetUp();
    }

    private void SetUp()
    {
        currentMode.SetUp();
        StartCoroutine(FadeIn());
    }

    private void Begin()
    {
        songVolumeState = 1;
        currentMode.Begin();
    }

    public void End()
    {
        Debug.Log("Ready to End Auto Runway");
        currentMode.End();
        currentMode = nextMode;
        SetUp();
    }

    public void TransitionToAuto()
    {
        Debug.Log("Start Transition to Auto Runway");
        nextMode = autoRunwayManager;

        Transition();
    }

    public void TransitionToLive()
    {
        Debug.Log("Start Transition to Live Runway");
        nextMode = liveRunwayManager;

        Transition();
    }

    private void Transition()
    {
        songVolumeState = 2;

        blackoutAnimator.ResetTrigger("In");
        blackoutAnimator.SetTrigger("Out");

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        blackoutAnimator.ResetTrigger("Out");
        blackoutAnimator.SetTrigger("In");
        //yield return new WaitUntil(() => blackout.color.a == 0);
        yield return new WaitForSeconds(1);
        Begin();
    }

    IEnumerator FadeOut()
    {
        blackoutAnimator.ResetTrigger("In");
        blackoutAnimator.SetTrigger("Out");
        yield return new WaitForSeconds(1);
        End();
    }

    public Mode getMode()
    {
        return curMode;
    }

    public IEnumerator ShouldRestartScene()
    {
        Debug.Log("No Users, wait another 10 seconds for new user or else go back to AutoRunway");
        yield return new WaitForSeconds(30);

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

    void Update()
    {
        if(audioSource.time >= audioSource.clip.length)
        {
            curSong++;
            if (curSong == playList.Count)
                curSong = 0;
            AudioClip clip = Resources.Load<AudioClip>(playList[curSong]);
            audioSource.clip = clip;
            audioSource.Play();
        }

        if (songVolumeState == 1)
        {
            float startVolume = audioSource.volume;

            if (audioSource.volume < maxSongVolume)
            {
                audioSource.volume += startVolume * Time.deltaTime / reduceSongVolumeTime;
            }
        }

        if (songVolumeState == 2) {
            float startVolume = audioSource.volume;

            if (audioSource.volume > minSongVolume)
            {
                audioSource.volume -= startVolume * Time.deltaTime / reduceSongVolumeTime;
            }
        }
    }
}


