using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Mode { AUTO,LIVE,NONE };

public class AppManager : Singleton<AppManager>
{
    public AutoRunwayManager autoRunwayManager;
    public LiveRunwayManager liveRunwayManager;
    public Image blackout;
    public Animator blackoutAnimator;
    public AudioSource audioSource;

    //depricated
    private Mode curMode = Mode.AUTO;

    private IRunwayMode currentMode;
    private IRunwayMode nextMode;

    private List<string> playList = new List<string>(new string[] { "clearskies", "seeking" });
    private int curSong = 0;
    private float maxSongVolume = 0.4f;
    private float minSongVolume = 0.1f;
    private float reduceSongVolumeTime = 1.0f;
    private byte songVolumeState;

    private byte fadeState = 0;
    private float fadeTime = 1.0f;
    private float fadeCounter = 0;

    public int currentAutoLevel = 0;
    public int currentLiveLevel = 0;
    public int level = 0;

    void Start()
    {
        MRData.Instance.LoadEverything();

        fadeState = 0;
        currentMode = autoRunwayManager;

        SetUp();
    }

    public Mode GetRunwayMode()
    {
        if (currentMode != null)
            return currentMode.GetMode();
        else
            return Mode.NONE;
    }

    private void SetUp()
    {
        currentMode.SetUp(level);

        blackoutAnimator.ResetTrigger("Out");
        blackoutAnimator.SetTrigger("In");

        fadeCounter = 0;
        fadeState = 1;
        //StartCoroutine(FadeIn());
    }

    private void Begin()
    {
        if (audioSource.clip == null)
            StartAudio();

        if (GetRunwayMode() == Mode.AUTO)
            songVolumeState = 1;
        else
            songVolumeState = 2;

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

        currentAutoLevel = 0;
        level = 0;

        Transition();
    }

    public void TransitionToLive()
    {
        Debug.Log("Start Transition to Live Runway");
        nextMode = liveRunwayManager;

        currentLiveLevel = 0;
        level = 0;

        Transition();
    }

    public void TransitionToNextAutoLevel()
    {
        Debug.Log("Start Transition to Auto Runway");
        nextMode = autoRunwayManager;

        currentAutoLevel = GetNextAutoLevel(currentAutoLevel);
        level = currentAutoLevel;

        Transition();
    }

    private void Transition()
    {
        songVolumeState = 2;

        blackoutAnimator.ResetTrigger("In");
        blackoutAnimator.SetTrigger("Out");

        fadeCounter = 0;
        fadeState = 2;
        //StartCoroutine(FadeOut());
    }

    /*
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
    */
    public Mode getMode()
    {
        return curMode;
    }

    public IEnumerator ShouldRestartScene()
    {
        Debug.Log("No Users, wait another 10 seconds for new user or else go back to AutoRunway");
        yield return new WaitForSeconds(30);

        //StartCoroutine(UserManager.Instance.getNumberofUsers(RestartScene));
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

    private int GetNextAutoLevel(int curLevel)
    {
        curLevel++;

        if (curLevel == MRData.Instance.collections.collections.Count)
            curLevel = 0;

        return curLevel;
    }

    private void StartAudio()
    {
        curSong = 0;
        int rand = Random.Range(0, playList.Count);
        AudioClip clip = Resources.Load<AudioClip>(playList[rand]);
        audioSource.clip = clip;
        audioSource.Play();
    }

    void Update()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            if (audioSource.time >= audioSource.clip.length - 0.05)
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

            if (songVolumeState == 2)
            {
                float startVolume = audioSource.volume;

                if (audioSource.volume > minSongVolume)
                {
                    audioSource.volume -= startVolume * Time.deltaTime / reduceSongVolumeTime;
                }
            }
        }

        if (fadeState == 1)
        {
            if (fadeCounter >= fadeTime) {
                fadeState = 0;
                Begin();
            }
            else
            {
                fadeCounter += Time.deltaTime;
            }
        }

        if (fadeState == 2)
        {
            if (fadeCounter >= fadeTime)
            {
                fadeState = 0;
                End();
            }
            else
            {
                fadeCounter += Time.deltaTime;
            }
        }
    }
}


