using UnityEngine;
using System.Collections;

public class UITestAppManager : MonoBehaviour
{
    private Collection m_currentCollection;
    private Collection m_nextCollection;

    private void Awake()
    {
        UIEvents.OnUpNextCompleteCallback += UIEvents_OnUpNextComplete;
    }

    // Use this for initialization
    void Start()
    {
        MRData.Instance.LoadEverything();
        UIManager.Instance.ShowOutfit(MRData.Instance.outfits.outfits[0]);
        RunCollection();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void RunCollection () {
        m_currentCollection = MRData.Instance.collections.collections[0];
        UIManager.Instance.RunUpNextTimer("Billy Bob Hillbilly", 10.0f,5.0f);
    }

    void UIEvents_OnUpNextComplete()
    {
        Debug.Log("DO SOMETHING!");
    }
}
