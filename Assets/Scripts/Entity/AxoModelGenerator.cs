using System;
using System.Collections;
using FIMSpace.FTail;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AxoModelGenerator : Mingleton<AxoModelGenerator>
{
    //public int instanceCount = 0;

    public RuntimeAnimatorController animatorController;

    public static string[] GetAssetsRequired(int id)
    {
        AxoStruct traits = AxoDatabase.Get(id);

        return new string[]
        {
            $"BaseModel_{traits.outfit}",
            $"Prefab_Face_{traits.face}",
            $"Prefab_Hat_{traits.top}"
        };

    }
    
    public void Create(int id, UnityAction<AxoInfo> onFinish = null)
    {
        StartCoroutine(CreateModel(id, onFinish));
    }
    
    public void Generate(AxoInfo axoObject, UnityAction<AxoInfo> onFinish = null)
    {
        StartCoroutine(LoadAssets(axoObject, onFinish));
    }

    IEnumerator CreateModel(int id, UnityAction<AxoInfo> onFinish)
    {
        AxoStruct traits = AxoDatabase.Get(id);

        var assetsRequired = GetAssetsRequired(id);

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired[0]);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject baseModel = Instantiate(handle.Result, transform);
            baseModel.SetActive(false);
            baseModel.name = traits.id;

            AxoInfo axoObject = baseModel.AddComponent<AxoInfo>();
            axoObject.id = id;
            axoObject.name = $"#{id}";
            
            NavMeshAgent nav = axoObject.gameObject.AddComponent<NavMeshAgent>();
            nav.speed = 1.0f;
            
            axoObject.gameObject.AddComponent<Axolittle>();

            // axolittle avatar icon
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"{Configuration.GetWeb3URL()}avatar/{id}.png");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D axoTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
                axoObject.sprite = Sprite.Create(axoTexture, Rect.MinMaxRect(0, 0, axoTexture.width, axoTexture.height),
                    new Vector2(0.5f, 0.5f));
            }

            onFinish?.Invoke(axoObject);
        }
        else
        {
            Debug.LogError(
                $"#{traits.id} Could not find base model {traits.type}: {assetsRequired[0]} or {traits.routfit}");
        }

        yield return null;
    }

    IEnumerator LoadAssets(AxoInfo axoObject, UnityAction<AxoInfo> onFinish)
    {
        AxoStruct traits = AxoDatabase.Get(axoObject.id);

        var assetsRequired = GetAssetsRequired(axoObject.id);

        AsyncOperationHandle<GameObject> handle;

        var rootFaceNode = "Armature/joint6/joint7/joint8/joint9/joint10/joint24/joint24_end";
        var tailNode = "Armature/joint6/joint7/joint26";

        var face = traits.face;
        var top = traits.top;
        var color = Color.HSVToRGB(traits.rhue / 360.0f, 0.3f, 1f);

        Transform rootFaceBone = axoObject.gameObject.transform.Find(rootFaceNode);
        //baseModel.transform.localPosition = new Vector3(instanceCount++ * -1.0f, 0, 0);
        axoObject.gameObject.GetComponent<Animator>().runtimeAnimatorController = animatorController;
        var meshRenderer = axoObject.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

        TailAnimator2 tail = axoObject.gameObject.transform.Find(tailNode).gameObject.AddComponent<TailAnimator2>();

        // ADJUST TO ROBOT/COSMIC?
        if (traits.type == "Robot")
        {
            meshRenderer.sharedMaterial =
                Instantiate(Resources.Load<Material>("Models/Axolittles/Faces/Robot/Body/Material"));
            meshRenderer.sharedMaterial.color = Color.white;
        }
        else if (traits.type == "Cosmic")
        {
            meshRenderer.sharedMaterial =
                Instantiate(Resources.Load<Material>("Models/Axolittles/Faces/Cosmic/Body/Material"));
            meshRenderer.sharedMaterial.color = Color.white;
        }
        else
        {
            meshRenderer.sharedMaterial = meshRenderer.material;
            meshRenderer.sharedMaterial.color = color;
        }

        bool useInverseScale = axoObject.gameObject.transform.Find("Armature") &&
                               axoObject.gameObject.transform.Find("Armature").localScale.x != 1;


        // CREATE FACE
        if (face.Length > 0 && face != "None")
        {
            handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired[1]);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject faceModel = Instantiate(handle.Result, rootFaceBone, useInverseScale);
                if (useInverseScale)
                {
                    faceModel.transform.localPosition = handle.Result.transform.localPosition *
                                                        handle.Result.transform.localScale.x;
                    faceModel.transform.localRotation = handle.Result.transform.localRotation;
                }
            }
            else
            {
                Debug.LogError($"#{traits.id} Could not find face {assetsRequired[1]}: {face} or {traits.rface}");
            }
        }

        // CREATE HAT
        if (top.Length > 0 && top != "None")
        {
            handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired[2]);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject topModel = Instantiate(handle.Result, rootFaceBone, useInverseScale);
                if (useInverseScale)
                {
                    topModel.transform.localPosition =
                        handle.Result.transform.localPosition * handle.Result.transform.localScale.x;
                    topModel.transform.localRotation = handle.Result.transform.localRotation;
                }
            }
            else
            {
                Debug.LogError($"#{traits.id} Could not find {assetsRequired[2]}: {top} or {traits.rtop}");
            }
        }

        onFinish?.Invoke(axoObject);

        yield return null;
    }
}
