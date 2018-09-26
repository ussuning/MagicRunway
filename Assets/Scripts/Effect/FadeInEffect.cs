using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInEffect : MonoBehaviour {
    public float duration = 1.0f;
    List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
    List<Material[]> origSharedMaterials = new List<Material[]>();
    bool isEffectDone = false;

	// Use this for initialization
	void Awake () {
        skinnedRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());

        for (int i = 0; i < skinnedRenderers.Count; i++)
        {
            SkinnedMeshRenderer sRenderer = skinnedRenderers[i];
            origSharedMaterials.Add(sRenderer.sharedMaterials);
            for (int j = 0; j < sRenderer.materials.Length; j++)
            {
                sRenderer.materials[j] = new Material(sRenderer.materials[j]);
                sRenderer.materials[j].name += " TEMP";
                Color c = sRenderer.materials[j].color;
                c.a = 0;
                sRenderer.materials[j].color = c;

                StandardShaderUtils.ChangeRenderMode(sRenderer.materials[j], StandardShaderUtils.BlendMode.Fade);
            }
        }
    }

    private void OnDestroy()
    {

        for (int i= 0; i < skinnedRenderers.Count; i++)
        {
            skinnedRenderers[i].sharedMaterials = origSharedMaterials[i];
        }
    }

    private void Update()
    {
        if (isEffectDone)
        {
            Destroy(this);
        }
        else
        {
            foreach (SkinnedMeshRenderer sRenderer in skinnedRenderers)
            {
                foreach (Material mat in sRenderer.materials)
                {

                    Color c = mat.color;
                    c.a = Mathf.Clamp01(c.a + Time.deltaTime / duration);
                    mat.color = c;

                    if (c.a >= 1.0f)
                        isEffectDone = true;
                }
            }
        }

    }
}
