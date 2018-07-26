using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MR;

[ExecuteInEditMode]
public class LimbSphereSet : MonoBehaviour {

    public Cloth targetCloth;
    public GameObject targetRootBone;
    public SkinnedMeshRenderer targetSkinnedMeshRenderer;

    public LimbSphere Hip;
    public LimbSphere HipLeft;
    public LimbSphere HipRight;
    public LimbSphere CenterChest;
    public LimbSphere CenterShoulder;

    public LimbSphere LeftChest;
    public LimbSphere LeftShoulder;
    public LimbSphere LeftBack;
    public LimbSphere LeftElbow;
    public LimbSphere LeftWristFront;
    public LimbSphere LeftWristBack;

    public LimbSphere LeftUpLeg;
    public LimbSphere LeftUpLeg2;
    public LimbSphere LeftKnee;
    public LimbSphere LeftCalf;
    public LimbSphere LeftAnkle;

    public LimbSphere RightChest;
    public LimbSphere RightShoulder;
    public LimbSphere RightBack;
    public LimbSphere RightElbow;
    public LimbSphere RightWristFront;
    public LimbSphere RightWristBack;

    public LimbSphere RightUpLeg;
    public LimbSphere RightUpLeg2;
    public LimbSphere RightKnee;
    public LimbSphere RightCalf;
    public LimbSphere RightAnkle;

    protected SphereCollider[] sphereColliders;

    private ClothSphereColliderPair[] _clothSphereColliderPairs;
    public ClothSphereColliderPair[] clothSphereColliderPairs {
        get {
            if (_clothSphereColliderPairs == null)
            {

                // Assign.
                _clothSphereColliderPairs = createClothSphereColliderPairs();
            }
            return _clothSphereColliderPairs;
        }
    }

    protected ClothSphereColliderPair[] createClothSphereColliderPairs() {

                // List the limbspheres in pairs
        LimbSphere[] limbSpheres = new LimbSphere[] { 
            // Center Shoulder
            CenterShoulder, LeftShoulder,
            CenterShoulder, LeftChest,
            CenterShoulder, RightShoulder,
            CenterShoulder, RightChest,

            // Center Chest
            CenterChest,    LeftShoulder,
            CenterChest,    LeftBack,
            CenterChest,    RightShoulder,
            CenterChest,    RightBack,
            CenterChest,    Hip,

            // Upper Left to Right
            LeftChest,      RightChest,
            LeftBack,       RightBack,

            // Upper Left
            LeftShoulder,   LeftBack,
            LeftShoulder,   LeftChest,
            LeftShoulder,   LeftElbow,
            LeftElbow,      LeftWristFront,
            LeftElbow,      LeftWristBack,

            // Upper Right
            RightShoulder,  RightElbow,
            RightShoulder,  RightChest,
            RightShoulder,  RightBack,
            RightElbow,     RightWristFront,
            RightElbow,     RightWristBack,

            // Hip
            Hip,            HipLeft,
            Hip,            HipRight,
            HipLeft,        LeftUpLeg,
            HipRight,       RightUpLeg,

            // Lower Left
            LeftUpLeg,      LeftUpLeg2,
            LeftUpLeg2,     LeftKnee,
            LeftKnee,       LeftCalf,
            LeftCalf,       LeftAnkle,

            // Lower Right
            RightUpLeg,     RightUpLeg2,
            RightUpLeg2,    RightKnee,
            RightKnee,      RightCalf,
            RightCalf,      RightAnkle
            };

        // Create ClothSphereColliderPairs with limbSphere pairs.
        List<ClothSphereColliderPair> spherePairs = new List<ClothSphereColliderPair>();
        for (int i = 0; i < limbSpheres.Length; i += 2)
        {
            spherePairs.Add(
                new ClothSphereColliderPair(
                    limbSpheres[i].sphereCollider,
                    limbSpheres[i + 1].sphereCollider));
        }

        return spherePairs.ToArray();
    }




    public void AlignToSkeleton() {
        if (targetRootBone == null) {
            Debug.LogError("No Root Bone assigned!");
            return;
        }

        Dictionary<string, Transform> joints = new Dictionary<string, Transform>();

        joints.Add("Hips", targetRootBone.transform);
        joints.Add("Spine", targetRootBone.transform.FindDeepChild("mixamorig:Spine"));
        joints.Add("Spine1",targetRootBone.transform.FindDeepChild("mixamorig:Spine1"));
        joints.Add("Spine2", targetRootBone.transform.FindDeepChild("mixamorig:Spine2"));
        joints.Add("Neck", targetRootBone.transform.FindDeepChild("mixamorig:Neck"));
        joints.Add("LeftUpLeg", targetRootBone.transform.FindDeepChild("mixamorig:LeftUpLeg"));
        joints.Add("LeftLeg", targetRootBone.transform.FindDeepChild("mixamorig:LeftLeg"));
        joints.Add("LeftFoot", targetRootBone.transform.FindDeepChild("mixamorig:LeftFoot"));
        joints.Add("RightUpLeg", targetRootBone.transform.FindDeepChild("mixamorig:RightUpLeg"));
        joints.Add("RightLeg", targetRootBone.transform.FindDeepChild("mixamorig:RightLeg"));
        joints.Add("RightFoot", targetRootBone.transform.FindDeepChild("mixamorig:RightFoot"));
        joints.Add("LeftShoulder", targetRootBone.transform.FindDeepChild("mixamorig:LeftShoulder"));
        joints.Add("LeftArm", targetRootBone.transform.FindDeepChild("mixamorig:LeftArm"));
        joints.Add("LeftForeArm", targetRootBone.transform.FindDeepChild("mixamorig:LeftForeArm"));
        joints.Add("RightShoulder", targetRootBone.transform.FindDeepChild("mixamorig:RightShoulder"));
        joints.Add("RightArm", targetRootBone.transform.FindDeepChild("mixamorig:RightArm"));
        joints.Add("RightForeArm", targetRootBone.transform.FindDeepChild("mixamorig:RightForeArm"));

        foreach (KeyValuePair<string, Transform> kvp in joints) {
            if (kvp.Value == null)
                Debug.LogError("Could not find transform in targetRootBone's children " + kvp.Key);
        }

        Hip.transform.parent = joints["Hips"];
        HipLeft.transform.parent = joints["Hips"];
        HipRight.transform.parent = joints["Hips"];
        CenterChest.transform.parent = joints["Spine2"];
        CenterShoulder.transform.parent = joints["Spine2"];
        LeftChest.transform.parent = joints["Spine2"];
        RightChest.transform.parent = joints["Spine2"];

        LeftBack.transform.parent = joints["LeftShoulder"];
        LeftShoulder.transform.parent = joints["LeftShoulder"];
        LeftElbow.transform.parent = joints["LeftArm"];
        LeftWristBack.transform.parent = joints["LeftForeArm"];
        LeftWristFront.transform.parent = joints["LeftForeArm"];

        RightBack.transform.parent = joints["RightShoulder"];
        RightShoulder.transform.parent = joints["RightShoulder"];
        RightElbow.transform.parent = joints["RightArm"];
        RightWristBack.transform.parent = joints["RightForeArm"];
        RightWristFront.transform.parent = joints["RightForeArm"];

        LeftUpLeg.transform.parent = joints["LeftUpLeg"];
        LeftUpLeg2.transform.parent = joints["LeftUpLeg"];
        LeftKnee.transform.parent = joints["LeftLeg"];
        LeftCalf.transform.parent = joints["LeftLeg"];
        LeftAnkle.transform.parent = joints["LeftFoot"];

        RightUpLeg.transform.parent = joints["RightUpLeg"];
        RightUpLeg2.transform.parent = joints["RightUpLeg"];
        RightKnee.transform.parent = joints["RightLeg"];
        RightCalf.transform.parent = joints["RightLeg"];
        RightAnkle.transform.parent = joints["RightFoot"];
    }

    public void Gather() {
        Hip.transform.parent =
        HipLeft.transform.parent =
        HipRight.transform.parent =
        CenterChest.transform.parent =
        CenterShoulder.transform.parent =
        LeftChest.transform.parent =
        RightChest.transform.parent =

        LeftBack.transform.parent =
        LeftShoulder.transform.parent =
        LeftElbow.transform.parent =
        LeftWristBack.transform.parent =
        LeftWristFront.transform.parent =

        RightBack.transform.parent =
        RightShoulder.transform.parent =
        RightElbow.transform.parent =
        RightWristBack.transform.parent =
        RightWristFront.transform.parent =

        LeftUpLeg.transform.parent =
        LeftUpLeg2.transform.parent =
        LeftKnee.transform.parent =
        LeftCalf.transform.parent =

        RightUpLeg.transform.parent =
        RightUpLeg2.transform.parent =
        RightKnee.transform.parent = 
        RightCalf.transform.parent = this.transform;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(LimbSphereSet))]
public class LimbSphereSetEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LimbSphereSet limbSphereSet = (LimbSphereSet)target;
        if (GUILayout.Button("Assign To Cloth"))
        {
            Debug.Log("Assign To Cloth");
            if (limbSphereSet.targetCloth != null)
            {
                limbSphereSet.targetCloth.sphereColliders = limbSphereSet.clothSphereColliderPairs;
            }
        }
        if (GUILayout.Button("Align To Skeleton"))
        {
            Debug.Log("Align To Skeleton");
            limbSphereSet.AlignToSkeleton();
        }
        if (GUILayout.Button("Gather"))
        {
            Debug.Log("Gathering...");
            limbSphereSet.Gather();
        }
    }
}
#endif
