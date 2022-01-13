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
    
    public void GetImage(NftAddress avatar, UnityAction<Sprite> onFinish = null)
    {
        StartCoroutine(DownloadSprite(avatar.id, onFinish));
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

    public void Generate(NftAddress avatar, UnityAction<AxoInfo> onFinish = null)
    {
        StartCoroutine(LoadAssets(avatar, onFinish));
    }
    
    IEnumerator LoadAssets(NftAddress avatar, UnityAction<AxoInfo> onFinish)
    {
        Transform existing = transform.Find($"{avatar.id}");

        if (existing)
        {
            AxoInfo info = existing.GetComponent<AxoInfo>();
            if (info)
            {
                onFinish?.Invoke(info);
                yield break;
            }
        }
        
        AxoStruct traits = AxoDatabase.Get(avatar.id);

        var assetsRequired = GetAssetsRequired(avatar.id);

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetsRequired["base"]);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject baseModel = Instantiate(handle.Result, transform);
            baseModel.SetActive(false);
            baseModel.name = traits.id;

            AxoInfo axoObject = baseModel.AddComponent<AxoInfo>();
            axoObject.id = avatar.id;
            axoObject.name = $"#{avatar.id}";

            var rootFaceNode = "Armature/joint6/joint7/joint8/joint9/joint10/joint24/joint24_end";
            var tailNode = "Armature/joint6/joint7/joint26";

            var face = traits.face;
            var top = traits.top;
            var color = Color.HSVToRGB(traits.rhue / 360.0f, 0.3f, 1f);

            Transform rootFaceBone = baseModel.transform.Find(rootFaceNode);
            //baseModel.transform.localPosition = new Vector3(instanceCount++ * -1.0f, 0, 0);
            axoObject.gameObject.GetComponent<Animator>().runtimeAnimatorController = animatorController;
            var meshRenderer = axoObject.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

            TailAnimator2 tail = axoObject.gameObject.transform.Find(tailNode).gameObject.AddComponent<TailAnimator2>();

            // ADJUST TO ROBOT/COSMIC?
            if (traits.type == "Robot")
            {
                AsyncOperationHandle<Material> materialHandle = Addressables.LoadAssetAsync<Material>(assetsRequired["material"]);
                yield return materialHandle;
                meshRenderer.sharedMaterial = Instantiate(materialHandle.Result);
                meshRenderer.sharedMaterial.color = Color.white;
            }
            else if (traits.type == "Cosmic")
            {
                AsyncOperationHandle<Material> materialHandle = Addressables.LoadAssetAsync<Material>(assetsRequired["material"]);
                yield return materialHandle;
                meshRenderer.sharedMaterial = Instantiate(materialHandle.Result);
                meshRenderer.sharedMaterial.color = Color.white;
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

            NavMeshAgent nav = axoObject.gameObject.AddComponent<NavMeshAgent>();
            nav.speed = 1.0f;

            axoObject.gameObject.AddComponent<Axolittle>();

            // axolittle avatar icon
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"{Configuration.GetWeb3URL()}avatar/{avatar.id}.png");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D axoTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
                axoObject.sprite = Sprite.Create(axoTexture, Rect.MinMaxRect(0, 0, axoTexture.width, axoTexture.height),
                    new Vector2(0.5f, 0.5f));
            }

            onFinish?.Invoke(axoObject);

            yield return null;
        }
    }
}
