using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour {
    public static string CROWD_SHORT = "crowdshort";
    public static string CROWD_LONG = "crowdlong";
    public static string APPLAUSE_1 = "applause1";
    public static string APPLAUSE_2 = "applause2";

    public static AudioClip LoadClip(string sfxName)
    {
        AudioClip clip = Resources.Load<AudioClip>(sfxName);

        return clip;
    }
}
