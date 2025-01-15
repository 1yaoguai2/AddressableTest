using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// Addressable资源更新器
/// </summary>
public class AddressableUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;  // 状态显示文本
    [SerializeField] private Slider progressBar;  // 进度条
    [SerializeField] private AssetReference mainSceneAddress;  // 主场景地址

    private void Start()
    {
        StartCoroutine(StartUpdateProcess());
    }

    /// <summary>
    /// 开始更新流程
    /// </summary>
    private System.Collections.IEnumerator StartUpdateProcess()
    {
        statusText.text = "正在初始化Addressable系统...";
        Debug.Log("正在初始化Addressable系统...");
        
        // var initOperation = Addressables.InitializeAsync();
        //
        // while (!initOperation.IsDone)
        // {
        //     yield return null;
        // }
        yield return Addressables.InitializeAsync();
        
        statusText.text = "正在检查更新...";
        Debug.Log("正在检查更新...");
        var catalogOperation = Addressables.CheckForCatalogUpdates(false);
        yield return catalogOperation;
        Debug.Log($"Catalog检查结果: 状态={catalogOperation.Status}, 结果数量={catalogOperation.Result?.Count ?? 0}");
        if (catalogOperation.Status == AsyncOperationStatus.Succeeded && catalogOperation.Result.Count > 0)
        {
                statusText.text = "发现更新，正在检查大小...";
                Debug.Log("发现更新，正在检查大小...");
                var sizeOperation = Addressables.GetDownloadSizeAsync("RawImage");
                yield return sizeOperation;
                
                if (sizeOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    long downloadSize = sizeOperation.Result;
                    if (downloadSize > 0)
                    {
                        statusText.text = $"下载大小: {FormatSize(downloadSize)}";
                        Debug.Log($"下载大小: {FormatSize(downloadSize)}");
                        var downloadOperation = Addressables.DownloadDependenciesAsync("RawImage");
                        
                        while (!downloadOperation.IsDone)
                        {
                            UpdateProgress(downloadOperation);
                            yield return null;
                        }
                        
                        if (downloadOperation.Status == AsyncOperationStatus.Succeeded)
                        {
                            statusText.text = "下载完成!";
                            Debug.Log("下载完成!");
                            progressBar.value = 1f;
                        }
                        else
                        {
                            statusText.text = "下载失败";
                            Debug.Log($"下载失败:{downloadOperation.OperationException}");
                        }
                    }
                    else
                    {
                        statusText.text = "无需下载";
                        Debug.Log("无需下载");
                    }
                }
                else
                {
                    statusText.text = "检查下载大小失败";
                    Debug.Log("检查下载大小失败");
                }
        }
        else
        {
                statusText.text = "没有可用更新";
                Debug.Log("没有可用更新");
        }


        // 使用Addressables加载主场景
        statusText.text = "正在加载主场景...";
        Debug.Log("正在加载主场景...");
        var sceneLoadOperation = Addressables.LoadSceneAsync(mainSceneAddress);
        yield return sceneLoadOperation;

        if (sceneLoadOperation.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("主场景加载成功");
            Debug.Log("主场景加载成功");
        }
        else
        {
            Debug.LogError($"主场景加载失败: {sceneLoadOperation.OperationException}");
            statusText.text = "场景加载失败";
            Debug.Log("场景加载失败");
        }
    }

    /// <summary>
    /// 更新下载进度
    /// </summary>
    private void UpdateProgress(AsyncOperationHandle handle)
    {
        progressBar.value = handle.PercentComplete;
        statusText.text = $"下载中... {handle.PercentComplete * 100:F1}%";
        Debug.Log($"下载中... {handle.PercentComplete * 100:F1}%");
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes = bytes / 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 销毁时释放资源
    /// </summary>
    private void OnDestroy()
    {
        // 所有操作句柄现在都在局部变量中，无需手动释放
    }
}
