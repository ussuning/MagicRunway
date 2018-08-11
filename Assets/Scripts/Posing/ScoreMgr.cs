using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class ComboScoreData
{
    public int combo;
    public int combo_score;
}

[Serializable]
public class ScoreData
{
    public int single_pose_score;
    public List<ComboScoreData> combo_scores;

    public static ScoreData CreateFromJSON(string JsonPath)
    {
        string jsonString = File.ReadAllText(JsonPath);
        ScoreData score_data = JsonUtility.FromJson<ScoreData>(jsonString);
        return score_data;
    }
}

public class ScoreMgr : MonoBehaviour {

    public static ScoreMgr Instance;

    public string scoreDataFilePath = "/StreamingAssets/score_data.json";

    public int SinglePoseScore
    {
        get
        {
            return ScoreInfo.single_pose_score;
        }
    }

    public int GetComboScore(int comboNum)
    {
        if (comboNum > ScoreInfo.combo_scores.Count - 1)
            return ScoreInfo.combo_scores[ScoreInfo.combo_scores.Count - 1].combo_score;
        return ScoreInfo.combo_scores[comboNum].combo_score;
    }

    ScoreData ScoreInfo;

    void Awake()
    {
        Instance = this;
        LoadScoreData();
    }

    private void LoadScoreData()
    {
        string filePath = Application.dataPath + scoreDataFilePath;

        if (File.Exists(filePath))
        {
            ScoreInfo = ScoreData.CreateFromJSON(filePath);
        }
        else
        {
            Debug.Log("Score Data doesn't exist!");
        }
    }
}
