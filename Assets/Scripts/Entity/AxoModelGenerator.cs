using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxoModelGenerator : MonoBehaviour
{
    public RuntimeAnimatorController AnimatorController;
    
    public Dictionary<string, string> Data = new Dictionary<string, string>()
    {
        {"number", "0"},
        {"iswoman", "y"},
        {"hue", "168"},
        {"top", "Explorer"},
        {"face", "Eyelashes and Tongue"},
        {"outfit", "Flowers Shirt"},
        {"bodytype", "normal"}
    };

    public void GenerateFromTraits(Dictionary<string, string> traits)
    {
        var rootFaceNode = "Armature/joint6/joint7/joint8/joint9/joint10/joint24/joint24_end";
        
        var face = traits["face"];
        var outfit = traits["outfit"];
        var top = traits["top"];
        var color = Color.HSVToRGB(int.Parse(traits["hue"])/360.0f, 0.3f, 1f);
        
        var gender = traits["iswoman"] == "y" ? "female" : "male";

        // CREATE BASE MODEL
        var baseModelPath = $"Models/Axolittles/BaseModel/{gender}/{outfit}";
        Debug.Log(baseModelPath);
        GameObject baseModel = Instantiate(Resources.Load<GameObject>(baseModelPath), transform);
        baseModel.GetComponent<Animator>().runtimeAnimatorController = AnimatorController;
        baseModel.GetComponentInChildren<SkinnedMeshRenderer>().material.color = color;
        
        // CREATE FACE
        if (face.Length > 0)
        {
            var faceModelPath = $"Prefabs/Traits/Face/{face}";
            GameObject faceModel = Instantiate(Resources.Load<GameObject>(faceModelPath), baseModel.transform.Find(rootFaceNode), false);
        }

        // CREATE HAT
        if (top.Length > 0)
        {
            var topModelPath = $"Prefabs/Traits/Top/{top}";
            GameObject topModel = Instantiate(Resources.Load<GameObject>(topModelPath),baseModel.transform.Find(rootFaceNode), false);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateFromTraits(Data);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
