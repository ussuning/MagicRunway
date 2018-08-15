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
    public float combo_time;
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

    public static PoseMgr Instance;

    public string comboDataFilePath = "/StreamingAssets/combo_data.json";
    float PoseCD = 0.75f;

    private int curPose = 0;
    private int prevPose = 0;
    private bool isInNewPoseCD = false;
    public bool IsInNewPoseCooldown
    {
        get
        {
            return isInNewPoseCD;
        }
    }

    Combos ComboInfo;

    public int ComboCount
    {
        get
        {
            if (ComboInfo == null)
                return 0;
            return ComboInfo.combos.Count;
        }
    }

    public ComboData GetComboInfo(int comboNum)
    {
        if (comboNum > ComboCount - 1)
            return ComboInfo.combos[ComboCount - 1];
        return ComboInfo.combos[comboNum];
    }

    public void GenerateNewPose()
    {
        int newPose = 0;
        do
        {
            newPose = UnityEngine.Random.Range(1, 1 + BrainDataManager.Instance.NumPoses);
        } while (newPose == curPose || newPose == prevPose);

        prevPose = curPose;
        curPose = newPose;

        StartCoroutine(SetNewPoseCooldown());

        EventMsgDispatcher.Instance.TriggerEvent(EventDef.New_Pose_Generated, curPose);
    }

    IEnumerator SetNewPoseCooldown()
    {
        isInNewPoseCD = true;
        yield return new WaitForSeconds(PoseCD);
        isInNewPoseCD = false;
    }

    void Awake ()
    {
        Instance = this;
        LoadComboData();
    }

    //void Start()
    //{
    //    curPose = 0;
    //    prevPose = 0;
    //}

    private void LoadComboData()
    {
        string filePath = Application.dataPath + comboDataFilePath;

        if (File.Exists(filePath))
        {
            ComboInfo = Combos.CreateFromJSON(filePath);
        }
        else
        {
            Debug.Log("Pose Data doesn't exist!");
        }
    }
}
