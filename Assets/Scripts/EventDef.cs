using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDef
{
    public const int Kinect_User_ID_Added = 10;

    //Posing
    public const int New_Pose_Generated = 100;
    public const int User_Pose_Detected = New_Pose_Generated + 1;
    public const int User_Combo_Detected = User_Pose_Detected + 1;
    public const int High_Combo_Detected = User_Combo_Detected + 1;
    public const int Combo_Broken_Detected = High_Combo_Detected + 1;
    public const int Combo_Replay_Start = Combo_Broken_Detected + 1;
    public const int Combo_Replay_End = Combo_Replay_Start + 1;

}
