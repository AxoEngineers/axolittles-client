using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AxoModelGenerator : MonoBehaviour
{
    public RuntimeAnimatorController AnimatorController;

    public void GenerateFromTraits(AxoStruct traits)
    {
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
            Debug.LogError("Could not find base model: " + baseModelPath);
            return;
        }
        
        GameObject baseModel = Instantiate(modelAsset, transform);
        baseModel.GetComponent<Animator>().runtimeAnimatorController = AnimatorController;
        var meshRenderer = baseModel.GetComponentInChildren<SkinnedMeshRenderer>();
        
        // ADJUST TO ROBOT/COSMIC?
        if (traits.type == "robot")
        {
            meshRenderer.sharedMaterial = Instantiate(Resources.Load<Material>("Models/Axolittles/Faces/Robot/Body/Material"));
        }
        if (traits.type == "cosmic")
        {
            meshRenderer.sharedMaterial = Instantiate(Resources.Load<Material>("Models/Axolittles/Faces/Cosmic/Body/Material"));
        }
        else
        {
            meshRenderer.sharedMaterial = meshRenderer.material;
        }
        
        meshRenderer.sharedMaterial.color = color;

        // CREATE FACE
        if (face.Length > 0)
        {
            var faceModelPath = $"Prefabs/Traits/Face/{face}";
            GameObject faceAsset = Resources.Load<GameObject>(faceModelPath);
            if (faceAsset == null)
            {
                Debug.LogError("Could not find face: " + face);
            }
            else
            {
                GameObject faceModel = Instantiate(faceAsset,baseModel.transform.Find(rootFaceNode), false);
            }
        }

        // CREATE HAT
        if (top.Length > 0)
        {
            var topModelPath = $"Prefabs/Traits/Top/{top}";
            GameObject topAsset = Resources.Load<GameObject>(topModelPath);
            if (topAsset == null)
            {
                Debug.LogError("Could not find top: " + face);
            }
            else
            {
                GameObject topModel = Instantiate(topAsset,baseModel.transform.Find(rootFaceNode), false);
            }
        }
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateFromTraits(AxoDatabase.Data[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
