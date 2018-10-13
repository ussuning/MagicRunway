using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Avatar controller is the component that transfers the captured user motion to a humanoid model (avatar). Avatar controller classic allows manual assignment of model's rigged bones to the Kinect's tracked joints.
/// </summary>
public class AvatarControllerClassic : AvatarController
{	
	// Public variables that will get matched to bones. If empty, the Kinect will simply not track it.
	public Transform HipCenter;
    public Transform Spine;
    public Transform SpineMid;
    public Transform ShoulderCenter;
	public Transform Neck;
	public Transform Head;

	public Transform ClavicleLeft;
	public Transform ShoulderLeft;
	public Transform ElbowLeft; 
	public Transform HandLeft;
	public Transform FingersLeft;
//	private Transform FingerTipsLeft = null;
	public Transform ThumbLeft;

	public Transform ClavicleRight;
	public Transform ShoulderRight;
	public Transform ElbowRight;
	public Transform HandRight;
	public Transform FingersRight;
//	private Transform FingerTipsRight = null;
	public Transform ThumbRight;
	
	public Transform HipLeft;
	public Transform KneeLeft;
	public Transform FootLeft;
	public Transform ToesLeft = null;
	
	public Transform HipRight;
	public Transform KneeRight;
	public Transform FootRight;
	public Transform ToesRight = null;

	[Tooltip("The body root node (optional).")]
	public Transform BodyRoot;

	// Offset node this transform is relative to, if any (optional)
	//public GameObject OffsetNode;


	// If the bones to be mapped have been declared, map that bone to the model.
	protected override void MapBones()
	{
		bones[0] = HipCenter;
		bones[1] = Spine;
		bones[2] = ShoulderCenter;
		bones[3] = Neck;
		bones[4] = Head;
	
		bones[5] = ShoulderLeft;
		bones[6] = ElbowLeft;
		bones[7] = HandLeft;
		bones[8] = FingersLeft;
//		bones[9] = FingerTipsLeft;
		bones[10] = ThumbLeft;
	
		bones[11] = ShoulderRight;
		bones[12] = ElbowRight;
		bones[13] = HandRight;
		bones[14] = FingersRight;
//		bones[15] = FingerTipsRight;
		bones[16] = ThumbRight;
	
		bones[17] = HipLeft;
		bones[18] = KneeLeft;
		bones[19] = FootLeft;
		bones[20] = ToesLeft;
	
		bones[21] = HipRight;
		bones[22] = KneeRight;
		bones[23] = FootRight;
		bones[24] = ToesRight;

		// special bones
		bones[25] = ClavicleLeft;
		bones[26] = ClavicleRight;

		bones[27] = FingersLeft;
		bones[28] = FingersRight;
		bones[29] = ThumbLeft;
		bones[30] = ThumbRight;
		
		// body root and offset
		bodyRoot = BodyRoot;
		//offsetNode = OffsetNode;

//		if(offsetNode == null)
//		{
//			offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
//			offsetNode.transform.position = transform.position;
//			offsetNode.transform.rotation = transform.rotation;
//			offsetNode.transform.parent = transform.parent;
//			
//			transform.parent = offsetNode.transform;
//			transform.localPosition = Vector3.zero;
//			transform.localRotation = Quaternion.identity;
//		}

//		if(bodyRoot == null)
//		{
//			bodyRoot = transform;
//		}
	}

    protected override void ScaleTorso()
    {
        if (hipWidthFactor == 0f || shoulderWidthFactor == 0f)
            return;

        float hipScaleX = hipWidthFactor;
        HipCenter.localScale = new Vector3(hipScaleX, HipCenter.localScale.y, hipScaleX * hipZFactor);
        //Debug.Log("HipCenter.lossyScale " + HipCenter.lossyScale);

        // Unscale so that knee/ankles are normal (Vector3.one)
        resetJointScale(ref KneeLeft);//.localScale = new Vector3(1f / KneeLeft.parent.lossyScale.x, 1f / KneeLeft.parent.lossyScale.y, 1f / KneeLeft.parent.lossyScale.z);
        resetJointScale(ref KneeRight);//.localScale = new Vector3(1f / KneeRight.parent.lossyScale.x, 1f / KneeRight.parent.lossyScale.y, 1f / KneeRight.parent.lossyScale.z);
        //Debug.Log("KneeLeft.lossyScale " + KneeLeft.lossyScale);
        //Spine.localScale = new Vector3(hipWidthFactor, 1, 1);
        float midScaleX = (hipWidthFactor + shoulderWidthFactor) / 2.0f;
        float midScaleZ = (hipWidthFactor * hipZFactor + 1f) / 2.0f;
        resetJointScale(ref SpineMid);
        SpineMid.localScale = new Vector3(SpineMid.localScale.x * midScaleX, SpineMid.localScale.y, SpineMid.localScale.z * midScaleZ);
        //Debug.Log("SpineMid.lossyScale " + SpineMid.lossyScale);
        float shoulderScaleX = shoulderWidthFactor;
        resetJointScale(ref ShoulderCenter);
        ShoulderCenter.localScale = new Vector3(ShoulderCenter.localScale.x * shoulderScaleX, ShoulderCenter.localScale.y, ShoulderCenter.localScale.z);
        //Debug.Log("ShoulderCenter.lossyScale " + ShoulderCenter.lossyScale);
        //Debug.Log("hipWidthFactor " + hipWidthFactor);
        //Debug.Log("shoulderWidthFactor " + shoulderWidthFactor);
        for (int i = 0; i < ShoulderCenter.childCount; i++)
        {
            Transform child = ShoulderCenter.GetChild(i);
            resetJointScale(ref child);
        }
    }
	
}


#if UNITY_EDITOR
[CustomEditor(typeof(AvatarControllerClassic))]
public class AvatarControllerClassicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AvatarControllerClassic myScript = (AvatarControllerClassic)target;
        if (GUILayout.Button("Save Config Data"))
        {
            myScript.SaveConfigData();
        }
        if (GUILayout.Button("Load Config Data"))
        {
            myScript.LoadConfigData();
        }
        GUILayout.Label("hipWidthFactor=" + myScript.hipWidthFactor);
        GUILayout.Label("shoulderWidthFactor=" + myScript.shoulderWidthFactor);
    }

    //void OnSceneGUI()
    //{
    //    AvatarController ac = (AvatarController)target;
    //    //Handles.color = Color.red;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.elbowOutty.normalized);
    //    Handles.color = Color.green;
    //    Handles.DrawLine(ac.shPos, ac.shPos + ac.shOutty.normalized);
    //    //Handles.color = Color.blue;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.spineOutty.normalized);


    //    //Handles.color = Color.magenta;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.finalOutty.normalized);
    //    //Handles.color = Color.cyan;
    //    //Handles.DrawLine(ac.elbowPos, ac.elbowPos + ac.elbowForward.normalized);
    //    //Handles.color = Color.yellow;
    //    //Handles.DrawDottedLine(ac.shPos, ac.shPos + ac.shElbowForward.normalized, 5f);

    //}
}
#endif
