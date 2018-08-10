using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class PoseFeedback
{
    public float pose_confidence;
    public string feedback_text;
    public float feedback_color_r;
    public float feedback_color_g;
    public float feedback_color_b;
    public int feedback_particles_id;
}

[Serializable]
public class ComboFeedback
{
    public int combo_num;
    public float intensity;
}

[Serializable]
public class FeedbackData
{
    public List<PoseFeedback> pose_feedbacks;
    public List<ComboFeedback> combo_feedbacks;

    public static FeedbackData CreateFromJSON(string JsonPath)
    {
        string jsonString = File.ReadAllText(JsonPath);
        FeedbackData feedback_data = JsonUtility.FromJson<FeedbackData>(jsonString);
        return feedback_data;
    }
}

public class FeedbackMgr : MonoBehaviour {

    public static FeedbackMgr Instance;

    public string feedbackDataFilePath = "/StreamingAssets/feedback_data.json";

    FeedbackData FeedbackData;

    public PoseFeedback GetPoseFeedback(float con)
    {
        List<PoseFeedback> feedbacks = FeedbackData.pose_feedbacks;
        for (int i= feedbacks.Count-1; i>=0; i--)
        {
            if (con >= feedbacks[i].pose_confidence)
                return feedbacks[i];
        }
        return null;
    }

    public ComboFeedback GetComboFeedback(int combo)
    {
        return FeedbackData.combo_feedbacks[combo];
    }

    void Awake ()
    {
        Instance = this;
        LoadFeedbackData();
    }

    private void LoadFeedbackData()
    {
        string filePath = Application.dataPath + feedbackDataFilePath;

        if (File.Exists(filePath))
        {
            FeedbackData = FeedbackData.CreateFromJSON(filePath);
        }
        else
        {
            Debug.Log("Feedback Data doesn't exist!");
        }
    }
}
