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
    public float shoulderAdjustWidthFactor = 1.0f;

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
        this.avatarControllerName = avatarController.name;
        this.spineVerticalOffset = avatarController.spineVerticalOffset;
        this.shoulderCenterVerticalOffset = avatarController.shoulderCenterVerticalOffset;
        this.neckVerticalOffset = avatarController.neckVerticalOffset;
        this.headVerticalOffset = avatarController.headVerticalOffset;
        this.hipAdjustWidthFactor = avatarController.hipAdjustWidthFactor;
        this.shoulderAdjustWidthFactor = avatarController.shoulderAdjustWidthFactor;
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
        avatarController.shoulderAdjustWidthFactor = this.shoulderAdjustWidthFactor;
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

    public string ConfigDatatPath
    {
        get
        {
            return "Assets/Resources/AvatarControllerConfigData.txt";
        }
    }

    public AvatarControllerConfigData()
    {
        Load();
    }

    protected void Load()
    {
        StreamReader reader = new StreamReader(ConfigDatatPath);
        String contents = reader.ReadToEnd();
        //Debug.Log(contents);
        reader.Close();

        string[] lines = contents.Split(
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
        StreamWriter writer = new StreamWriter(ConfigDatatPath, false);
        foreach (AvatarControllerEntry data in entries.Values)
            writer.WriteLine(data.ToJSON());
        writer.Close();
    }
}