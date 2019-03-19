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
    private static string AUX_PREFIX = "x_";
    private static bool ENABLE_AUX_BONES = false;
    private static bool ENABLE_DETACH_LIMBS = true;
    internal static bool FLATTEN_BONES = true;

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

        if (boneSlotMap.ContainsKey(bone) == false)
            boneSlotMap[bone] = t;
        else
            Debug.LogError("BoneSlot " + bone + " already has a transform assigned. Assigned=" + boneSlotMap[bone].name + " New=" + t.name);

        if (ENABLE_AUX_BONES)
        {
            // Set wrapper transform to prevent skewing of children transforms due to 
            // non-uniform scaling of parent transforms.
            if (t != null && t.parent != null && bone != BoneSlot.HipCenter)
            {
                if (isAuxBone(t.parent) == false)
                {
                    GameObject go = new GameObject();
                    Transform wrapper = go.transform;
                    wrapper.name = AUX_PREFIX + t.name;
                    wrapper.position = t.position;
                    wrapper.rotation = Quaternion.identity;
                    wrapper.localScale = Vector3.one;

                    auxBones.Add(bone, wrapper);
                    auxBonesByTransform.Add(wrapper, bone);

                    // replace child of parent with wrapper.
                    Transform origParent = t.parent;
                    int siblingIdx = t.GetSiblingIndex();
                    t.parent = wrapper;
                    wrapper.SetSiblingIndex(siblingIdx);
                    wrapper.parent = origParent;

                }
            }
        }

        if (FLATTEN_BONES)
        {
            if (t != null && t.parent != this)
            {
                parentMap.Add(t, new ParentAndChildIdx(t.parent, t.GetSiblingIndex())); // <Child, <Parent, SiblingIdx>>
            }
        }
    }

    internal bool isAuxBone(Transform transform)
    {
        return auxBonesByTransform.ContainsKey(transform);
    }

    internal override Transform GetChildBone(Transform parent, int idx = 0)
    {
        if (FLATTEN_BONES)
        {
            foreach (KeyValuePair<Transform, ParentAndChildIdx> kvp in parentMap)
                if (kvp.Value.parent == parent && kvp.Value.childIdx == idx)
                    return kvp.Key;

            return null;
        }

        Transform boneChild = base.GetChildBone(parent, idx);
        // Check to see if child bone is a wrapper joint (solution to prevent skewing due to non-uniform scaling of parents).
        // wrapper joints should be skipped when searching for an actual child bone. -HH
        if (ENABLE_AUX_BONES && isAuxBone(boneChild))
            return boneChild.GetChild(0); // Wrapper bones should only have one child.
        else
            return boneChild;
    }

    internal override Transform GetParentBone(Transform child)
    {
        if (FLATTEN_BONES)
            return parentMap.ContainsKey(child) ? parentMap[child].parent : null;

        Transform boneParent = base.GetParentBone(child);
        // Check to see if child bone is a wrapper joint (solution to prevent skewing due to non-uniform scaling of parents).
        // wrapper joints should be skipped when searching for an actual child bone. -HH
        if (ENABLE_AUX_BONES && isAuxBone(boneParent))
            return boneParent.parent;
        else
            return boneParent;
    }

    internal void FlattenBones()
    {
        if (FLATTEN_BONES)
            foreach (Transform child in parentMap.Keys)
            {
                if (child == null)
                    continue;

                // Don't flatten bones that don't have corresponding Kinect joint, otherwise they will be stuck (no movement).
                // These joints rely on their parents' position (parents have corresponding Kinect joint).
                if (boneSlotMap[BoneSlot.ClavicleLeft] == child ||
                    boneSlotMap[BoneSlot.ClavicleRight] == child ||
                    boneSlotMap[BoneSlot.SpineMid] == child)
                {
                    continue;
                }

                child.parent = this.transform;
            }
    }

    // Update positions of Aux Bones to child position
    void UpdateAuxBonePositions()
    {
        if (ENABLE_AUX_BONES == false)
            return;

        foreach (Transform auxBone in auxBones.Values)
        {
            if (auxBone.childCount == 1)
            {
                // Detach child.
                Transform child = auxBone.GetChild(0);
                Transform origParen = child.parent;
                child.parent = null;
                // Move auxBone to child position, reset rotation.
                auxBone.position = child.position;
                auxBone.rotation = Quaternion.identity;
                Transform parent = auxBone.parent;
                if (parent != null && parent.lossyScale != Vector3.one)
                {
                    // Reset global scale to 1.
                    auxBone.localScale = new Vector3(1f / parent.lossyScale.x, 1f / parent.lossyScale.y, 1f / parent.lossyScale.z);
                }
                //auxBone.localScale = Vector3.one;
                //Reattach child.
                child.parent = origParen;
            }
            else if (auxBone.childCount > 1)
            {
                Debug.LogWarning("auxBone [" + auxBone.name + "] has more than one child. This is bad!");
            }
            // else no children, skip

            //auxBone.rotation = Quaternion.identity;
        }
    }

    Dictionary<BoneSlot, Transform> auxBones = new Dictionary<BoneSlot, Transform>();
    Dictionary<Transform, BoneSlot> auxBonesByTransform = new Dictionary<Transform, BoneSlot>();

    struct ParentAndChildIdx
    {
        public ParentAndChildIdx(Transform parent, int childIdx)
        {
            this.parent = parent;
            this.childIdx = childIdx;
        }
        public Transform parent;
        public int childIdx;
    }

    Dictionary<Transform, ParentAndChildIdx> parentMap = new Dictionary<Transform, ParentAndChildIdx>(); // key is child, value is parent
    Dictionary<BoneSlot, Transform> boneSlotMap = new Dictionary<BoneSlot, Transform>();
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

    //internal override void resetJointScale(Transform joint)
    //{
    //    //if (ENABLE_AUX_BONES)
    //    //{
    //    //    // Joints with wrapper bone parents don't need to worry about inversing the scale from parent because that's
    //    //    // what the wrapper transform will do.
    //    //    joint.localScale = Vector3.one;
    //    //}
    //    //else
    //        base.resetJointScale(joint);
    //}

    internal override void SetBoneScale(Transform boneTransform, Vector3 worldScale)
    {
        if (float.IsNaN(worldScale.x) || float.IsNaN(worldScale.y) || float.IsNaN(worldScale.z))
        {
            Debug.LogError("SetBoneScale scale is NaN");
            return;
        }
        
        if (ENABLE_AUX_BONES)
        {
            // Invert scaling of wrapper bone parent before setting localScale of this bone.
            if (isAuxBone(boneTransform.parent))
            {
                //boneTransform.parent.eulerAngles = Vector3.zero;
                resetJointScale(boneTransform.parent);
            }
        }
        base.SetBoneScale(boneTransform, worldScale);
    }

    //private void LateUpdate()
    //{
    //    UpdateAuxBones();
    //}
    protected override void PostTranslateBones()
    {
        UpdateAuxBonePositions();
    }

}

