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
    private static string AUX_PREFIX = "aux_";
    private static bool ENABLE_AUX_BONES = true;

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


    public enum BoneSlot
    {
        HipCenter,
        Spine,
        SpineMid,
        ShoulderCenter,
        Neck,
        Head,

        ClavicleLeft,
        ShoulderLeft,
        ElbowLeft,
        HandLeft,
        FingersLeft,
        //	private Transform FingerTipsLeft = null,
        ThumbLeft,

        ClavicleRight,
        ShoulderRight,
        ElbowRight,
        HandRight,
        FingersRight,
        //	private Transform FingerTipsRight = null,
        ThumbRight,

        HipLeft,
        KneeLeft,
        FootLeft,
        ToesLeft,

        HipRight,
        KneeRight,
        FootRight,
        ToesRight,
    }

    public void SetBone(BoneSlot bone, Transform t)
    {
        switch (bone)
        {
            case BoneSlot.HipCenter:      HipCenter = t; break;
            case BoneSlot.Spine:          Spine = t; break;
            case BoneSlot.SpineMid:       SpineMid = t; break;
            case BoneSlot.ShoulderCenter: ShoulderCenter = t; break;
            case BoneSlot.Neck:           Neck = t; break;
            case BoneSlot.Head:           Head = t; break;

            case BoneSlot.ClavicleLeft:     ClavicleLeft = t; break;
            case BoneSlot.ShoulderLeft:     ShoulderLeft = t; break;
            case BoneSlot.ElbowLeft:        ElbowLeft = t; break;
            case BoneSlot.HandLeft:         HandLeft = t; break;
            case BoneSlot.FingersLeft:      FingersLeft = t; break;
            //	private Transform FingerTipsLeft = null: HipCenter = t; break;
            case BoneSlot.ThumbLeft:        ThumbLeft = t; break;

            case BoneSlot.ClavicleRight:    ClavicleRight = t; break;
            case BoneSlot.ShoulderRight:    ShoulderRight = t; break;
            case BoneSlot.ElbowRight:       ElbowRight = t; break;
            case BoneSlot.HandRight:        HandRight = t; break;
            case BoneSlot.FingersRight:     FingersRight = t; break;
            //	private Transform FingerTipsRight = null: HipCenter = t; break;
            case BoneSlot.ThumbRight:       ThumbRight = t; break;

            case BoneSlot.HipLeft:  HipLeft = t; break;
            case BoneSlot.KneeLeft: KneeLeft = t; break;
            case BoneSlot.FootLeft: FootLeft = t; break;
            case BoneSlot.ToesLeft: ToesLeft = t; break;

            case BoneSlot.HipRight:  HipRight = t; break;
            case BoneSlot.KneeRight: KneeRight = t; break;
            case BoneSlot.FootRight: FootRight = t; break;
            case BoneSlot.ToesRight: ToesRight = t; break;
            default:
                Debug.LogError("No case for Boneslot " + bone.ToString());
                break;
        }

        if (ENABLE_AUX_BONES)
        {
            // Set wrapper transform for non-uniform scaling bones.
            if (t != null && t.parent != null && bone != BoneSlot.HipCenter)
            {
                if (t.parent.name.StartsWith(AUX_PREFIX) == false)
                {
                    GameObject go = new GameObject();
                    Transform wrapper = go.transform;
                    wrapper.name = AUX_PREFIX + t.name;
                    wrapper.position = t.position;
                    wrapper.rotation = Quaternion.identity;
                    wrapper.localScale = Vector3.one;

                    auxBones.Add(bone, wrapper);

                    // replace child of parent with wrapper.
                    Transform origParent = t.parent;
                    int siblingIdx = t.GetSiblingIndex();
                    t.parent = wrapper;
                    wrapper.SetSiblingIndex(siblingIdx);
                    wrapper.parent = origParent;

                }
            }
        }
    }

    protected override Transform GetChildBone(Transform boneTransform, int idx = 0)
    {
        Transform boneChild = base.GetChildBone(boneTransform, idx);
        // Check to see if child bone is a wrapper joint (solution to prevent skewing due to non-uniform scaling of parents).
        // wrapper joints should be skipped when searching for an actual child bone. -HH
        if (ENABLE_AUX_BONES && boneChild.name.StartsWith(AUX_PREFIX))
            return boneChild.GetChild(0); // Wrapper bones should only have one child.
        else
            return boneChild;
    }

    protected override Transform GetParentBone(Transform boneTransform)
    {
        Transform boneParent = base.GetParentBone(boneTransform);
        // Check to see if child bone is a wrapper joint (solution to prevent skewing due to non-uniform scaling of parents).
        // wrapper joints should be skipped when searching for an actual child bone. -HH
        if (ENABLE_AUX_BONES && boneParent.name.StartsWith(AUX_PREFIX))
            return boneParent.parent;
        else
            return boneParent;
    }

    // Update positions of Aux Bones to child position
    void UpdateAuxBones()
    {
        foreach (Transform auxBone in auxBones.Values)
        {
            if (auxBone.childCount == 1)
            {
                auxBone.position = auxBone.GetChild(0).position;
            }
            else if (auxBone.childCount > 1)
            {
                Debug.LogWarning("auxBone [" + auxBone.name + "] has more than one child. This is bad!");
            }
            // else no children, skip

            auxBone.rotation = Quaternion.identity;
        }
    }

    Dictionary<BoneSlot, Transform> auxBones = new Dictionary<BoneSlot, Transform>();

    [Tooltip("The body root node (optional).")]
	public Transform BodyRoot;

    // Offset node this transform is relative to, if any (optional)
    //public GameObject OffsetNode;

    // These are the vertical position weights of spine joints. 
    // (Weight of 0 means vertically aligned to HipCenter, weight of 1 means aligned to ShoulderCenter.
    float spineVerticalWeight;
    float spineMidVerticalWeight;

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
        SetBoneScale(ref HipCenter, new Vector3(hipScaleX, HipCenter.localScale.y, hipScaleX * hipZFactor));
        //Debug.Log("HipCenter.lossyScale " + HipCenter.lossyScale);

        // Unscale so that knee/ankles are normal (Vector3.one)
        //resetJointScale(ref FootLeft);//.localScale = new Vector3(1f / KneeLeft.parent.lossyScale.x, 1f / KneeLeft.parent.lossyScale.y, 1f / KneeLeft.parent.lossyScale.z);
        //resetJointScale(ref FootRight);//.localScale = new Vector3(1f / KneeRight.parent.lossyScale.x, 1f / KneeRight.parent.lossyScale.y, 1f / KneeRight.parent.lossyScale.z);
        //Debug.Log("KneeLeft.lossyScale " + KneeLeft.lossyScale);
        //Spine.localScale = new Vector3(hipWidthFactor, 1, 1);
        float midScaleX = (hipWidthFactor + shoulderWidthFactor) / 2.0f;
        float midScaleZ = (hipWidthFactor * hipZFactor + 1f) / 2.0f;
        resetJointScale(ref SpineMid);
        SetBoneScale(ref SpineMid, new Vector3(SpineMid.localScale.x * midScaleX, SpineMid.localScale.y, SpineMid.localScale.z * midScaleZ));
        //Debug.Log("SpineMid.lossyScale " + SpineMid.lossyScale);
        float shoulderScaleX = shoulderWidthFactor;
        resetJointScale(ref ShoulderCenter);
        SetBoneScale(ref ShoulderCenter, new Vector3(ShoulderCenter.localScale.x * shoulderScaleX, ShoulderCenter.localScale.y, ShoulderCenter.localScale.z));
        //Debug.Log("ShoulderCenter.lossyScale " + ShoulderCenter.lossyScale);
        //Debug.Log("hipWidthFactor " + hipWidthFactor);
        //Debug.Log("shoulderWidthFactor " + shoulderWidthFactor);
        for (int i = 0; i < ShoulderCenter.childCount; i++)
        {
            Transform child = ShoulderCenter.GetChild(i);
            resetJointScale(ref child);
        }
    }

    protected override void resetJointScale(ref Transform joint)
    {
        if (ENABLE_AUX_BONES)
        {
            // Joints with wrapper bone parents don't need to worry about inversing the scale from parent because that's
            // what the wrapper transform will do.
            joint.localScale = Vector3.one;
        }
        else
            base.resetJointScale(ref joint);
    }

    protected override void SetBoneScale(ref Transform boneTransform, Vector3 localScale)
    {
        if (ENABLE_AUX_BONES)
        {
            // Invert scaling of wrapper bone parent before setting localScale of this bone.
            if (boneTransform.parent.name.StartsWith(AUX_PREFIX))
            {
                Vector3 parentScale = boneTransform.parent.parent.lossyScale;
                boneTransform.parent.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
            }
        }
        base.SetBoneScale(ref boneTransform, localScale);
    }

    private void LateUpdate()
    {
        UpdateAuxBones();
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
