using UnityEngine;
using System.Collections;

public class LinearBlendSkin : MonoBehaviour
{
    private MeshFilter filter;
    private Vector3[] originalVertices;
    private Transform[] bones;

    public SkinnedMeshRenderer prefabRenderer;

    // Use this for initialization
    void Start()
    {
        // The SkinnedMeshRenderer that is created with
        // the prefab magically knows what order the bones
        // are in, but it is impossible to get this information
        // through any public methods, so we'll have to
        // use an instance to look them up.
        bones = new Transform[prefabRenderer.bones.Length];
        Transform[] transforms = transform.root.GetComponentsInChildren<Transform>();
        int boneCount = bones.Length;
        int transCount = transforms.Length;
        for (int bone = 0; bone < boneCount; bone++)
        {
            string boneName = prefabRenderer.bones[bone].name;
            for (int trans = 0; trans < transCount; trans++)
            {
                if (transforms[trans].name == boneName)
                {
                    bones[bone] = transforms[trans];
                    break;
                }
            }
        }

        filter = GetComponent<MeshFilter>();

        // The sharedMesh reference gets broken
        // as soon as you modify the mesh, which
        // makes absolutely no sense, but that's
        // how Unity has decided it should work.
        originalVertices = filter.sharedMesh.vertices;

        // SkinnedMeshRenderer seems to use its
        // own transform only for the bounds, but
        // MeshRenderer uses it while rendering?
        // Should this be built into the binds?
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.localPosition = new Vector3();
        transform.localRotation = new Quaternion();
    }

    // Animations are updated between Update and LateUpdate
    void LateUpdate()
    {
        Mesh mesh = filter.mesh;

        Matrix4x4[] binds = mesh.bindposes;

        Vector3[] vertices = mesh.vertices;
        BoneWeight[] weights = mesh.boneWeights;
        int vertexCount = mesh.vertexCount;
        for (int vert = 0; vert < vertexCount; vert++)
        {
            int index0 = weights[vert].boneIndex0;
            float weight0 = weights[vert].weight0;
            vertices[vert] = bones[index0].localToWorldMatrix.MultiplyPoint3x4(binds[index0].MultiplyPoint3x4(originalVertices[vert])) * weight0;

            // This is ignoring the quality setting and
            // just using all 4 bone weights for now
            int index1 = weights[vert].boneIndex1;
            float weight1 = weights[vert].weight1;
            if (weight1 > 0f)
                vertices[vert] += bones[index1].localToWorldMatrix.MultiplyPoint3x4(binds[index1].MultiplyPoint3x4(originalVertices[vert])) * weight1;

            int index2 = weights[vert].boneIndex2;
            float weight2 = weights[vert].weight2;
            if (weight2 > 0f)
                vertices[vert] += bones[index2].localToWorldMatrix.MultiplyPoint3x4(binds[index2].MultiplyPoint3x4(originalVertices[vert])) * weight2;

            int index3 = weights[vert].boneIndex3;
            float weight3 = weights[vert].weight3;
            if (weight3 > 0f)
                vertices[vert] += bones[index3].localToWorldMatrix.MultiplyPoint3x4(binds[index3].MultiplyPoint3x4(originalVertices[vert])) * weight3;
        }

        mesh.vertices = vertices;
    }
}