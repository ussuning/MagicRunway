using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

[System.Serializable]
class AvatarControllerData
{
    public char delimiter = '|';

    public AvatarControllerData()
    {

    }

    public AvatarControllerData(AvatarController avatarController)
    {
        this.avatarControllerName = avatarController.name;
        this.spineVerticalOffset = avatarController.spineVerticalOffset;
        this.shoulderCenterVerticalOffset = avatarController.shoulderCenterVerticalOffset;
        this.neckVerticalOffset = avatarController.neckVerticalOffset;
        this.headVerticalOffset = avatarController.headVerticalOffset;
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
    }

    public override string ToString()
    {
        return this.avatarControllerName + delimiter
            + this.spineVerticalOffset + delimiter
            + this.shoulderCenterVerticalOffset + delimiter
            + this.neckVerticalOffset + delimiter
            + this.headVerticalOffset;
    }

    public bool LoadFromString(string line)
    {
        string[] values = line.Split(
            new[] { delimiter },
            StringSplitOptions.None
        );

        try
        {
            this.avatarControllerName = values[0];
            this.spineVerticalOffset = float.Parse(values[1]);
            this.shoulderCenterVerticalOffset = float.Parse(values[2]);
            this.neckVerticalOffset = float.Parse(values[3]);
            this.headVerticalOffset = float.Parse(values[4]);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
        return true;
    }

    public string avatarControllerName = "";
    public float spineVerticalOffset = 0f;
    public float shoulderCenterVerticalOffset = 0f; //Compensate for when shoulderCenter is not actually (vertically) at same height as left and right arm sockets.
    public float neckVerticalOffset = 0f;
    public float headVerticalOffset = 0f; //Compensate for head is not actually
}

class AvatarControllerConfigData
{
    public Dictionary<string, AvatarControllerData> entries = new Dictionary<string, AvatarControllerData>();

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
                AvatarControllerData acData = new AvatarControllerData();
                if (acData.LoadFromString(line))
                    entries.Add(acData.avatarControllerName, acData);
            }
        }
    }

    public void Save()
    {
        StreamWriter writer = new StreamWriter(ConfigDatatPath, false);
        foreach (AvatarControllerData data in entries.Values)
            writer.WriteLine(data.ToString());
        writer.Close();
    }
}