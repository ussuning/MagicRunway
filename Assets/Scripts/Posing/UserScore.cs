using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserScore : MonoBehaviour {

    public GameObject ScoreTextPrefab;

    public void GenerateScoreText()
    {
        Instantiate(ScoreTextPrefab, this.transform);
    }
}
