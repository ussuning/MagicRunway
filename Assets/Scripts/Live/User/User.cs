using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour {

    private int uindex;
    private long uid;
    public string ugender;    // switch to enum later
  
    // Use this for initialization
    public User(long id, int index)
    {
        uid = id;
        uindex = index;
    }
	
    public long getUserId()
    {
        return uid;
    }

    public int getUserIndex()
    {
        return uindex;
    }

    public string getGender()
    {
        return ugender;
    }

    public void setGender(string gender)
    {
        ugender = gender;
    }

}
