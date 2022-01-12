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

    public void Generate(int id, UnityAction<AxoInfo> onFinish=null)
    {
        StartCoroutine(LoadAssets(id, onFinish));
    }

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
    
    IEnumerator LoadAssets(int id, UnityAction<AxoInfo> onFinish)
    {
        AxoStruct traits = AxoDatabase.Get(id);
        
        var baseModelPath = $"BaseModel_{traits.outfit}";
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(baseModelPath);
        yield return handle;
        
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject baseModel = Instantiate(handle.Result, transform);
            baseModel.SetActive(false);
            baseModel.name = traits.id;
            
            var rootFaceNode = "Armature/joint6/joint7/joint8/joint9/joint10/joint24/joint24_end";
            var tailNode = "Armature/joint6/joint7/joint26";
        
            var face = traits.face;
            var outfit = traits.outfit;
            var top = traits.top;
            var color = Color.HSVToRGB(traits.rhue/360.0f, 0.3f, 1f);
            
            Transform rootFaceBone = baseModel.transform.Find(rootFaceNode);
            //baseModel.transform.localPosition = new Vector3(instanceCount++ * -1.0f, 0, 0);
            baseModel.GetComponent<Animator>().runtimeAnimatorController = animatorController;
            var meshRenderer = baseModel.GetComponentInChildren<SkinnedMeshRenderer>();
            
            TailAnimator2 tail = baseModel.transform.Find(tailNode).gameObject.AddComponent<TailAnimator2>();
        
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

            bool useInverseScale = baseModel.transform.Find("Armature") && baseModel.transform.Find("Armature").localScale.x != 1;
            
            
            // CREATE FACE
            if (face.Length > 0 && face != "None")
            {
                handle = Addressables.LoadAssetAsync<GameObject>($"Prefab_Face_{face}");
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
                    Debug.LogError($"#{traits.id} Could not find face Prefab_Face_{face}: " + face + " or " +
                                   traits.rface);
                }
            }

            // CREATE HAT
            if (top.Length > 0 && top != "None")
            {
                handle = Addressables.LoadAssetAsync<GameObject>($"Prefab_Hat_{top}");
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject topModel = Instantiate(handle.Result, rootFaceBone, useInverseScale);
                    if (useInverseScale)
                    {
                        topModel.transform.localPosition = handle.Result.transform.localPosition * handle.Result.transform.localScale.x;
                        topModel.transform.localRotation = handle.Result.transform.localRotation;
                    }
                }
                else
                {
                    Debug.LogError($"#{traits.id} Could not find Prefab_Hat_{face}: " + top + " or " + traits.rtop);
                }
            }
            
            AxoInfo info = baseModel.AddComponent<AxoInfo>();
            NavMeshAgent nav = baseModel.AddComponent<NavMeshAgent>();
            baseModel.AddComponent<Axolittle>();
            
            info.id = id;
            info.name = $"#{id}";
            nav.speed = 1.0f;
            
            // axolittle avatar icon
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"{Configuration.GetWeb3URL()}avatar/{id}.png");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                Texture2D axoTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                info.sprite = Sprite.Create(axoTexture, Rect.MinMaxRect(0, 0, axoTexture.width, axoTexture.height), new Vector2(0.5f, 0.5f) );
            }

            if (onFinish != null)
            {
                onFinish.Invoke(info);
            }
        }
        else
        {
            Debug.LogError($"#{traits.id} Could not find base model {traits.type}: " + baseModelPath + " or " + traits.routfit);
        }
        
        yield return null;
    }
    
}
