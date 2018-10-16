using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutfitSelectionTutorialController : MonoBehaviour {

    public GameObject TutorialGO;
    public float MinTutorialTime = 2f;

    private float tutorialTimeElapsed = 0f;

    private bool isTutorialStarted = false;
    private bool isTutorialFinished = false;
    public bool IsTutorialFinished
    {
        get
        {
            return isTutorialFinished;
        }
    }

	void Start ()
    {
        isTutorialStarted = false;
        isTutorialFinished = false;
        tutorialTimeElapsed = 0f;
        TutorialGO.SetActive(false);
    }

    void Update()
    {
        if(isTutorialStarted && !isTutorialFinished)
        {
            tutorialTimeElapsed += Time.deltaTime;
        }
    }

    public void StartTutorial()
    {
        isTutorialStarted = true;
        isTutorialFinished = false;
        tutorialTimeElapsed = 0f;
        TutorialGO.SetActive(true);
    }

    public void EndTutorial()
    {
        if(!isTutorialFinished && tutorialTimeElapsed >= MinTutorialTime)
        {
            isTutorialFinished = true;
            TutorialGO.SetActive(false);
        }
    }

    public void ShowTutorial()
    {
        if (isTutorialStarted && !isTutorialFinished)
        {
            if(!TutorialGO.activeSelf)
                TutorialGO.SetActive(true);
        }
    }

    public void HideTutorial()
    {
        if (TutorialGO.activeSelf)
            TutorialGO.SetActive(false);
    }
	
}
