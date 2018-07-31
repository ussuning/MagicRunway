using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class ComboData
{
    public int combo_num;
    public float pose_time;
}

[Serializable]
public class Combos
{
    public List<ComboData> combos;

    public static Combos CreateFromJSON(string JsonPath)
    {
        string jsonString = File.ReadAllText(JsonPath);
        Combos combo_data = JsonUtility.FromJson<Combos>(jsonString);
        return combo_data;
    }
}

public class PoseMgr : MonoBehaviour {

    public string comboDataFilePath = "/StreamingAssets/combo_data.json";

    public int curPose = 0;
    public int prevPose = 0;
    public int comboNum = 0;
    public int ComboNum
    {
        get
        {
            if (combos == null)
                return 0;
            if (comboNum > combos.combos.Count - 1)
                return combos.combos.Count - 1;
            return comboNum;
        }
    }

    public float poseTimeEllapsed = 0f;

    Combos combos;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    void Awake ()
    {
        LoadComboData();
    }

    void Start ()
    {
        curPose = 0;
        prevPose = 0;
        comboNum = 0;

        poseTimeEllapsed = 0f;
    }

    void Update ()
    {
        poseTimeEllapsed += Time.deltaTime;

        if (poseTimeEllapsed > combos.combos[ComboNum].pose_time)
        {
            GenerateNewPose();
            poseTimeEllapsed = 0f;
            comboNum = 0;
        }
    }

    public void OnUserPoseMatched(object param, object paramEx)
    {
        long userID = (long)param;

        if(poseTimeEllapsed <= combos.combos[ComboNum].pose_time)
        {
            comboNum++;
            GenerateNewPose();
            poseTimeEllapsed = 0;
        }
    }

    private void LoadComboData()
    {
        string filePath = Application.dataPath + comboDataFilePath;

        if (File.Exists(filePath))
        {
            combos = Combos.CreateFromJSON(filePath);
        }
        else
        {
            Debug.Log("Brain Data doesn't exist!");
        }

        Debug.Log(combos);
    }

    private void GenerateNewPose()
    {
        int newPose = 0;
        do
        {
            newPose = UnityEngine.Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
        } while (newPose == curPose || newPose == prevPose);
        prevPose = curPose;
        curPose = newPose;

        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPose);
    }
}
