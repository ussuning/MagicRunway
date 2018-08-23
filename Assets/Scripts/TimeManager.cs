using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager {

    private static TimeManager _instance = null;
    public static TimeManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new TimeManager();
            return _instance;
        }

    }
	
    public float timeScale
    {
        get { return Time.timeScale; }
        set
        {
           // Debug.Log("TimeManager.timeScale = " + value);
            Time.timeScale = value;
        }
    }
}
