using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CheckUpdate : MonoBehaviour
{
    public List<string> keys;
    public float totalDownLoadSize;
    public TextMeshProUGUI desc;
    public AssetReference menuScene;
    private void Start()
    {
        StartCoroutine(CheckUpdateAdddressable());
    }

    IEnumerator CheckUpdateAdddressable()
    {
        keys = new List<string>();

        totalDownLoadSize = 0;
        desc.color = Color.black;
        desc.text = "正在检查更新...";
        Debug.Log("初始化 Addressables");
        yield return Addressables.InitializeAsync();
        Debug.Log("检查 catalogs 更新");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        if (checkHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = checkHandle.Result;
            if (catalogs != null && catalogs.Count > 0)
            {
                Debug.Log("下载 catalogs 更新");
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return updateHandle;
                if (updateHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {

                    var sizeHandle = Addressables.GetDownloadSizeAsync(keys);
                    yield return sizeHandle;
                    desc.text = string.Empty;
                    if (sizeHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        totalDownLoadSize = sizeHandle.Result;
                        Debug.Log("DownLoad Size：" + (totalDownLoadSize / 1024.0f / 1024.0f).ToString("0.00"));
                        if (totalDownLoadSize > 0)
                        {
                            var downloadOperation = Addressables.DownloadDependenciesAsync("RawImage");
                            downloadOperation.Completed += OnDownloadComplete;
                        }
                    }
                 
                    Addressables.Release(sizeHandle);
                }
                else
                {
                    desc.text = "检查更新失败，检查联网状态";
                    desc.color = Color.red;
                  
                }
                Addressables.Release(updateHandle);

            }
            else
            {
                Debug.Log("本地catalogs无需更新,检查资源是否有更新");
                var sizeHandle = Addressables.GetDownloadSizeAsync(keys);
                yield return sizeHandle;
                if (sizeHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    desc.text = string.Empty;
                    totalDownLoadSize = sizeHandle.Result;
                    Debug.Log("DownLoad Size：" + (totalDownLoadSize / 1024.0f / 1024.0f).ToString("0.00"));
                    if (totalDownLoadSize > 0)
                    {
                        var downloadOperation = Addressables.DownloadDependenciesAsync("RawImage");
                        downloadOperation.Completed += OnDownloadComplete;
                    }
                   
                }
                else
                {
                    desc.text = "检查更新失败，检查联网状态";
                    desc.color = Color.red;
                
                }
                Addressables.Release(sizeHandle);
            }
            
        }
    
        Addressables.Release(checkHandle);        
    }

    private void OnDownloadComplete(AsyncOperationHandle handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            desc.text = "下载完成!";
            Debug.Log("下载完成!");
            LoadMenuScene();
        }
        else
        {
            desc.text = "下载失败";
            Debug.Log($"下载失败:{handle.OperationException}");
        }
    }

    private void LoadMenuScene()
    {
        Addressables.LoadSceneAsync(menuScene);
    }
}
