using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDef
{
    public const int Kinect_User_ID_Added = 10;
    public const int Kinect_User_Lost = Kinect_User_ID_Added + 1;
    public const int User_Gender_Selected = Kinect_User_Lost + 1;

    public const int Live_Mode_Set_Up = User_Gender_Selected + 1;

    //Posing
    public const int New_Pose_Generated = 100;
    public const int User_Pose_Detected = New_Pose_Generated + 1;
    public const int User_Combo_Detected = User_Pose_Detected + 1;
    public const int High_Combo_Detected = User_Combo_Detected + 1;
    public const int Combo_Broken_Detected = High_Combo_Detected + 1;
    public const int Combo_Replay_Start = Combo_Broken_Detected + 1;
    public const int Combo_Replay_End = Combo_Replay_Start + 1;

}
