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

    void RunCollection () {
        m_currentCollection = MRData.Instance.collections.collections[0];
        UIManager.Instance.ShowCollection(m_currentCollection);
        UIManager.Instance.RunUpNextTimer("Billy Bob Hillbilly", 300.0f,120.0f);
    }

    void UIEvents_OnUpNextComplete()
    {
        Debug.Log("DO SOMETHING!");
        m_currentCollection = MRData.Instance.collections.collections[1];
        UIManager.Instance.ShowCollection(m_currentCollection);
    }
}
