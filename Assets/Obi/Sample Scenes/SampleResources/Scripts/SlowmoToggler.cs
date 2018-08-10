using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowmoToggler : MonoBehaviour {

	public void Slowmo (bool slowmo) {
        TimeManager.instance.timeScale = slowmo?0.25f:1;
	}
}
