using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class LoadAddressable : MonoBehaviour
{

    public IEnumerator Start()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>("RawImage");
        handle.Completed += (obj) =>
        {
            var prefab = obj.Result;
            var image =Instantiate(prefab,transform.GetChild(0));
        };
        while (!handle.IsDone)
        {
            yield return null;
        }
        //Addressables.Release(handle);
    }
}
