using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDef
{
    //User
    public const int User_Gender_Selected = 10;

    //Posing
    public const int New_Pose_Generated = 100;
    public const int User_Pose_Matched = New_Pose_Generated + 1;
    public const int User_Combo_Detected = User_Pose_Matched + 1;
    public const int High_Combo_Detected = User_Combo_Detected + 1;
    public const int Combo_Broken_Detected = High_Combo_Detected + 1;
    public const int Combo_Replay_Start = Combo_Broken_Detected + 1;
    public const int Combo_Replay_End = Combo_Replay_Start + 1;

}
