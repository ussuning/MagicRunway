using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyingText : MonoBehaviour {

    public float FlyingSpeed = 0.1f;

    private Text Text;

    Vector3 targetPosition;
    bool isFlying = false;
    float curAlpha = 1f;

	public void ActivateFlying(Vector3 target)
    {
        targetPosition = target;
        isFlying = true;

        Destroy(this.gameObject, 2f);
    }

    void Awake()
    {
        Text = GetComponent<Text>();
    }
	
	void Update ()
    {
        if (isFlying)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, FlyingSpeed);

            curAlpha -= Time.deltaTime;
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, curAlpha);
        }
	}
}
