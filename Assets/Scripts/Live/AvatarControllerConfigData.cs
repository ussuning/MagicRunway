using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

[System.Serializable]
class AvatarControllerEntry
{
    public string avatarControllerName = "";
    public float spineVerticalOffset = 0f;
    public float shoulderCenterVerticalOffset = 0f; //Compensate for when shoulderCenter is not actually (vertically) at same height as left and right arm sockets.
    public float neckVerticalOffset = 0f;
    public float headVerticalOffset = 0f; //Compensate for head is not actually
    public float hipAdjustWidthFactor = 1.0f;
    public float hipZFactor = 1.0f;
    public float shoulderAdjustWidthFactor = 1.0f;
    public float hipUpwardsFactor = 0.0f; // Make legs longer.

    public static AvatarControllerEntry ParseJson(string json)
    {
        AvatarControllerEntry d = null;
        try {
            d = JsonUtility.FromJson<AvatarControllerEntry>(json);
        }
        catch (Exception e) {
            Debug.LogError("AvatarControllerData::ParseJson() Error: " + e.Message);
        }
        return d;
    }

    public AvatarControllerEntry()
    {
    }

    public AvatarControllerEntry(AvatarController avatarController)
    {
        string acName = avatarController.name;
        // Clean up the name in case this is a (Clone) object.
        string cloneStr = "(Clone)";
        int cloneIdx = acName.IndexOf(cloneStr);
        if (cloneIdx >= 0)
            acName = acName.Remove(cloneIdx);

        this.avatarControllerName = acName;
        this.spineVerticalOffset = avatarController.spineVerticalOffset;
        this.shoulderCenterVerticalOffset = avatarController.shoulderCenterVerticalOffset;
        this.neckVerticalOffset = avatarController.neckVerticalOffset;
        this.headVerticalOffset = avatarController.headVerticalOffset;
        this.hipAdjustWidthFactor = avatarController.hipAdjustWidthFactor;
        this.hipZFactor = avatarController.hipZFactor;
        this.shoulderAdjustWidthFactor = avatarController.shoulderAdjustWidthFactor;
        this.hipUpwardsFactor = avatarController.hipUpwardsFactor;
        Debug.LogWarning("Creating new AvatarControllerEntry " + acName);
    }

    public void PopulateTo(AvatarController avatarController)
    {
        if (avatarController.name != this.avatarControllerName)
        {
            Debug.LogWarning("Populating " + avatarController.name + " with data from " + this.avatarControllerName + ". Are you sure this is what you want?");
        }
        avatarController.spineVerticalOffset = this.spineVerticalOffset;
        avatarController.shoulderCenterVerticalOffset = this.shoulderCenterVerticalOffset;
        avatarController.neckVerticalOffset = this.neckVerticalOffset;
        avatarController.headVerticalOffset = this.headVerticalOffset;
        avatarController.hipAdjustWidthFactor = this.hipAdjustWidthFactor;
        avatarController.hipZFactor = this.hipZFactor;
        avatarController.shoulderAdjustWidthFactor = this.shoulderAdjustWidthFactor;
        avatarController.hipUpwardsFactor = this.hipUpwardsFactor;

        Debug.LogWarning("Applied tuning values for " + this.avatarControllerName);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    //public override string ToString()
    //{
    //    return this.avatarControllerName + delimiter
    //        + this.spineVerticalOffset + delimiter
    //        + this.shoulderCenterVerticalOffset + delimiter
    //        + this.neckVerticalOffset + delimiter
    //        + this.headVerticalOffset;
    //}

    //public bool LoadFromString(string line)
    //{
    //    string[] values = line.Split(
    //        new[] { delimiter },
    //        StringSplitOptions.None
    //    );

    //    try
    //    {
    //        this.avatarControllerName = values[0];
    //        this.spineVerticalOffset = float.Parse(values[1]);
    //        this.shoulderCenterVerticalOffset = float.Parse(values[2]);
    //        this.neckVerticalOffset = float.Parse(values[3]);
    //        this.headVerticalOffset = float.Parse(values[4]);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError(e.Message);
    //        return false;
    //    }
    //    return true;
    //}
}

class AvatarControllerConfigData
{
    public Dictionary<string, AvatarControllerEntry> entries = new Dictionary<string, AvatarControllerEntry>();

    public string ConfigDataResourceName
    {
        get
        {
            return "AvatarControllerConfigData";
        }
    }

    public string ConfigDataPathName
    {
        get
        {
            return "Assets/Resources/" + ConfigDataResourceName + ".txt";
        }
    }


    private static AvatarControllerConfigData _instance;
    public static AvatarControllerConfigData Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AvatarControllerConfigData();

            return _instance;
        }
    }

    public AvatarControllerConfigData()
    {
        Load();
    }

    public void Load()
    {
        entries.Clear();
        TextAsset textAsset = Resources.Load<TextAsset>(ConfigDataResourceName);

        string[] lines = textAsset.text.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        );

        foreach (string line in lines)
        {
            if (line.Length > 1)
            {
                AvatarControllerEntry acData = AvatarControllerEntry.ParseJson(line);
                if (acData != null && 
                    acData.avatarControllerName != null && 
                    acData.avatarControllerName.Length > 0)
                    entries.Add(acData.avatarControllerName, acData);
            }
        }
    }

    public void Save()
    {
        StreamWriter writer = new StreamWriter(ConfigDataPathName, false);
        foreach (AvatarControllerEntry data in entries.Values)
            writer.WriteLine(data.ToJSON());
        writer.Close();
        Debug.LogWarning("AvatarControllerConfigData written to file " + ConfigDataPathName);
    }
}