using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAnimationManager : MonoBehaviour {
    public static List<string> femalePoses = new List<string>(new string[] { "women_walking_01","women_walking_02" });
    public static List<string> malePoses = new List<string>(new string[] { "male_walking_01" });
   
    public static string GetPoseAnimation(string sex, bool random = true)
    {
        List<string> selected = femalePoses;
        string dir = "Female";
        
        if (sex == "m" || sex.ToLower() == "male")
        {
            selected = malePoses;
            dir = "Male";
        }
        
        int index = Random.Range(0, selected.Count);
        string path = "Animations/" + dir + "/" + selected[index];
     
        return path;
    }
}
