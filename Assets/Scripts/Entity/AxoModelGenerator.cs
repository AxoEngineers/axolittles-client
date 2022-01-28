using System;
using System.Collections;
using System.Collections.Generic;
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

    public static Dictionary<string, Color> colorCodes = new Dictionary<string, Color>()
    {
        {"Orange", new Color(255/255.0f,172/255.0f,131/255.0f)},
        {"Purple", new Color(128/255.0f, 0/255.0f, 128/255.0f)},
        {"Fuchsia", new Color(255/255.0f,0/255.0f,255/255.0f)},
        {"Yellow", new Color(255/255.0f,255/255.0f,0/255.0f)},
        {"Blue", new Color(135/255.0f,206/255.0f,250/255.0f)},
        {"Cyan", new Color(0/255.0f,255/255.0f,255/255.0f)},
        {"St Patrick", new Color(0/255.0f,128/255.0f,0/255.0f)},
        {"Magenta", new Color(255/255.0f,0/255.0f,255/255.0f)}
    };

    private void Start()
    {
        for (int i = 0; i < 9999; i++)
        {
            GetAssetsRequired(i);
        }
    }

    public static Dictionary<string, string> GetAssetsRequired(int id)
    {
        AxoStruct traits = AxoDatabase.Get(id);

        Dictionary<string, string> keyMap = new Dictionary<string, string>();

        keyMap.Add("base", $"BaseModel_{traits.outfit}");
        keyMap.Add("face", $"Prefab_Face_{traits.face}");
        keyMap.Add("hat", $"Prefab_Hat_{traits.top}");
        keyMap["material"] = null;
        
        if (traits.type == "Robot")
        {
            keyMap["material"] = "Robot Material";
        }
        else if (traits.type == "Cosmic")
        {
            keyMap["material"] = "Cosmic Material";
        }

        return keyMap;
    }
    
    public Coroutine GetImage(NftAddress avatar, UnityAction<Sprite> onFinish = null)
    {
        return StartCoroutine(DownloadSprite(avatar.id, onFinish));
    }
    
    IEnumerator DownloadSprite(int id, UnityAction<Sprite> onFinish = null)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture($"{Configuration.GetWeb3URL()}avatar/{id}.png");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D axoTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
            onFinish?.Invoke(Sprite.Create(axoTexture, Rect.MinMaxRect(0, 0, axoTexture.width, axoTexture.height),
                new Vector2(0.5f, 0.5f)));
        }
    }

    public Coroutine Generate(int id, UnityAction<AxoInfo> onFinish = null)
    {
        Coroutine routine = StartCoroutine(LoadAssets(id, onFinish));
        LoadingIndicator.Instance.modelRoutine = routine;
        return routine;
    }
    
    IEnumerator LoadAssets(int id, UnityAction<AxoInfo> onFinish)
    {
        Transform existing = transform.Find($"#{id}");

        if (existing)
        {
            AxoInfo info = existing.GetComponent<AxoInfo>();
            if (info)
            {
                onFinish?.Invoke(info);
                LoadingIndicator.Instance.modelRoutine = null;
                yield break;
            }
        }
        
        AxoStruct traits = AxoDatabase.Get(id);

        // Debug.Log(traits.ToString());
        
        var assetsRequired = GetAssetsRequired(id);

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired["base"]);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject baseModel = Instantiate(handle.Result, transform);
            baseModel.layer = LayerMask.NameToLayer("Axo");
            baseModel.SetActive(false);
            baseModel.name = traits.id;

            AxoInfo axoObject = baseModel.AddComponent<AxoInfo>();
            Animator ani = axoObject.gameObject.GetComponent<Animator>();
            axoObject.id = id;
            axoObject.name = $"#{id}";

            var baseNode = baseModel.transform.Find("Armature") ? (baseModel.transform.Find("Armature").name + "/") : "";
            
            var rootFaceNode = baseNode + "joint6/joint7/joint8/joint9/joint10/joint24/joint24_end";
            
            var tailNode = baseNode + "joint6/joint7/joint26";

            var face = traits.face;
            var top = traits.top;
            var color = Color.HSVToRGB(traits.rhue / 360.0f, 0.3f, 1f);
            var bgColor = GetColorCode(traits.background);

            Transform rootFaceBone = baseModel.transform.Find(rootFaceNode);
            Transform noneRootFaceBone = baseModel.transform.Find(baseNode + "joint6/joint7/joint8/joint9/joint10/joint24/joint25");

            if (noneRootFaceBone)
            {
                rootFaceBone = noneRootFaceBone;
            }
            // create a joint24_end bone on top of the head if it doesnt exist
            else if (!rootFaceBone)
            {
                rootFaceBone = baseModel.transform.Find(baseNode + "joint6/joint7/joint8/joint9/joint10/joint24");
                GameObject newRootFaceBone = new GameObject("joint24_end");
                newRootFaceBone.transform.SetParent(rootFaceBone, false);
                newRootFaceBone.transform.localPosition = new Vector3(0, 0.584521f, 0);
                rootFaceBone = newRootFaceBone.transform;
            }
            
            //baseModel.transform.localPosition = new Vector3(instanceCount++ * -1.0f, 0, 0);
            ani.runtimeAnimatorController = animatorController;
            var meshRenderer = axoObject.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

            Transform tailBone = axoObject.gameObject.transform.Find(tailNode);
            TailAnimator2 tail = tailBone.gameObject.AddComponent<TailAnimator2>();

            // ADJUST TO ROBOT/COSMIC?
            if (traits.type == "Robot" || traits.type == "Cosmic")
            {
                AsyncOperationHandle<Material> materialHandle = Addressables.LoadAssetAsync<Material>(assetsRequired["material"]);
                yield return materialHandle;
                if (materialHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    meshRenderer.sharedMaterial = Instantiate(materialHandle.Result);
                    if (traits.type == "Cosmic")
                    {
                        meshRenderer.sharedMaterial.color = bgColor;
                    }
                    else
                    {
                        meshRenderer.sharedMaterial.color = Color.white;
                    }
                }
            }
            else
            {
                meshRenderer.sharedMaterial = meshRenderer.material;
                meshRenderer.sharedMaterial.color = color;
            }

            bool useInverseScale = axoObject.gameObject.transform.Find("Armature") && axoObject.gameObject.transform.Find("Armature").localScale.x != 1;

            // CREATE FACE
            if (face.Length > 0 && face != "None")
            {
                handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired["face"]);
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

                    var faceMeshRenderer = faceModel.GetComponent<MeshRenderer>();
                    
                    var bgSync = faceModel.GetComponent<BackgroundColorSync>();
                    if (bgSync)
                    {
                        for (int i=0; i < faceMeshRenderer.sharedMaterials.Length; i++)
                        {
                            if (bgSync.targetMaterial.name == faceMeshRenderer.sharedMaterials[i].name)
                            {
                                // Debug.Log(bgSync.targetMaterial.name + " - " + faceMeshRenderer.sharedMaterials[i].name);
                                var newFaceMat = faceMeshRenderer.materials[i];
                                float h, s, v;
                                Color.RGBToHSV(bgColor, out h, out s, out v);
                                s /= 1.5f;
                                v *= 1.25f;

                                Color partBgColor = Color.HSVToRGB(h,s,v);
                                newFaceMat.color = partBgColor;
                                if (newFaceMat.IsKeywordEnabled("_EMISSION"))
                                {
                                    newFaceMat.SetColor("_EmissionColor", partBgColor);
                                }
                                break;
                            }
                        }
                    }
                    
                    if (traits.type == "Robot")
                    {
                        faceMeshRenderer.sharedMaterial = Instantiate(faceMeshRenderer.sharedMaterial);
                        faceMeshRenderer.sharedMaterial.color = Color.HSVToRGB((146 + traits.rhue) / 360.0f, 50f / 100.0f, 75f / 100.0f);
                    }
                    else if (traits.type == "Cosmic")
                    {
                        AsyncOperationHandle<Material> cosmicFaceHandle = Addressables.LoadAssetAsync<Material>("Cosmic Face");
                        yield return cosmicFaceHandle;
                        if (cosmicFaceHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            for (int i=0; i < faceMeshRenderer.sharedMaterials.Length; i++)
                            {
                                if (faceMeshRenderer.sharedMaterials[i].name.StartsWith("Faces"))
                                {
                                    faceMeshRenderer.sharedMaterials[i] = cosmicFaceHandle.Result;
                                    // Debug.Log("SET " + faceMeshRenderer.sharedMaterials[i].name);
                                }
                            }
                        }
                    }

                }
                else
                {
                    Debug.LogError($"#{traits.id} Could not find face {assetsRequired["face"]}: {face} or {traits.rface}");
                }
            }

            // CREATE HAT
            if (top.Length > 0 && top != "None")
            {
                handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired["hat"]);
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
                    Debug.LogError($"#{traits.id} Could not find {assetsRequired["hat"]}: {top} or {traits.rtop}");
                }
            }

            if (noneRootFaceBone)
            {
                noneRootFaceBone.localRotation = Quaternion.Euler(180, 0f, 90f);
            }

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
            LoadingIndicator.Instance.modelRoutine = null;
            yield return null;
        }
    }

    public static Color GetColorCode(string background)
    {
        if (background.StartsWith("#"))
        {
            ColorUtility.TryParseHtmlString(background, out var c);
            return c;
        }

        return colorCodes[background];
    }
}
