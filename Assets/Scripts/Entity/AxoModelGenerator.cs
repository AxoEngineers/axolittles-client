using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AxoModelGenerator : MonoBehaviour
{
    public static int instanceCount = 0;
    
    public RuntimeAnimatorController AnimatorController;

    public void GenerateFromTraits(AxoStruct traits)
    {
        Debug.Log(traits);
        
        var rootFaceNode = "Armature/joint6/joint7/joint8/joint9/joint10/joint24/joint24_end";
        
        var face = traits.face;
        var outfit = traits.outfit;
        var top = traits.top;
        var color = Color.HSVToRGB(traits.rhue/360.0f, 0.3f, 1f);
        
        var gender = traits.routfit.StartsWith("woman") ? "female" : "male";

        // CREATE BASE MODEL
        var baseModelPath = $"Models/Axolittles/BaseModel/{outfit}";
        var modelAsset = Resources.Load<GameObject>(baseModelPath);

        if (modelAsset == null)
        {
            Debug.LogError($"#{traits.id} Could not find base model {gender}: " + baseModelPath + " or " + traits.routfit);
            return;
        }
        
        GameObject baseModel = Instantiate(modelAsset, transform);
        baseModel.name = traits.id;
        
        Transform rootFaceBone = baseModel.transform.Find(rootFaceNode);
        baseModel.transform.localPosition = new Vector3(instanceCount++ * -1.0f, 0, 0);
        baseModel.GetComponent<Animator>().runtimeAnimatorController = AnimatorController;
        var meshRenderer = baseModel.GetComponentInChildren<SkinnedMeshRenderer>();
        
        // ADJUST TO ROBOT/COSMIC?
        if (traits.type == "Robot")
        {
            meshRenderer.sharedMaterial = Instantiate(Resources.Load<Material>("Models/Axolittles/Faces/Robot/Body/Material"));
            meshRenderer.sharedMaterial.color = Color.white;
        }
        else if (traits.type == "Cosmic")
        {
            meshRenderer.sharedMaterial = Instantiate(Resources.Load<Material>("Models/Axolittles/Faces/Cosmic/Body/Material"));
            meshRenderer.sharedMaterial.color = Color.white;
        }
        else
        {
            meshRenderer.sharedMaterial = meshRenderer.material;
            meshRenderer.sharedMaterial.color = color;
        }

        bool useInverseScale = baseModel.transform.Find("Armature").localScale.x != 1;
        
        // CREATE FACE
        if (face.Length > 0 && face != "None")
        {
            var faceModelPath = $"Prefabs/Traits/Face/{face}";
            GameObject faceAsset = Resources.Load<GameObject>(faceModelPath);
            if (faceAsset == null)
            {
                Debug.LogError($"#{traits.id} Could not find face {gender}: " + face + " or " + traits.rface);
            }
            else
            {
                GameObject faceModel = Instantiate(faceAsset, rootFaceBone, useInverseScale);
                if (useInverseScale)
                {
                    faceModel.transform.localPosition = faceAsset.transform.localPosition * faceAsset.transform.localScale.x;
                    faceModel.transform.localRotation = faceAsset.transform.localRotation;
                }
            }
        }

        // CREATE HAT
        if (top.Length > 0 && top != "None")
        {
            var topModelPath = $"Prefabs/Traits/Top/{top}";
            GameObject topAsset = Resources.Load<GameObject>(topModelPath);
            if (topAsset == null)
            {
                Debug.LogError($"#{traits.id} Could not find top {gender}: " + top + " or " + traits.rtop);
            }
            else
            {
                GameObject topModel = Instantiate(topAsset,rootFaceBone, useInverseScale);
                if (useInverseScale)
                {
                    topModel.transform.localPosition = topAsset.transform.localPosition * topAsset.transform.localScale.x;
                    topModel.transform.localRotation = topAsset.transform.localRotation;
                }
            }
        }
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 200; i <= 210; i++)
        {
            GenerateFromTraits(AxoDatabase.Data[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
