using UnityEngine;
using System.Collections;

public class AppManager : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        MRData.Instance.LoadEverything();
        UIManager.Instance.ShowOutfit(MRData.Instance.outfits.outfits[0]);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
