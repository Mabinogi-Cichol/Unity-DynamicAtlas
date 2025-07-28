using DynamicAtlas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Init : MonoBehaviour
{
    private void Awake()
    {
        DynamicAtlasManager.Init(new DynamicAtlasManager.Setting()
        {
            ATLAS_SIZE = 4096,
            SINGLE_TEXTURE_MAX_SIZE = 512,
            LoadSpriteFunc = LoadSpriteAsync,
            AtlasAppendDone = OnAtlasAppendDone
        });
    }

    private async Task<Sprite> LoadSpriteAsync(string sprite)
    {
        //此处可以替换成自己的资源框架加载逻辑
        var req = Resources.LoadAsync(sprite);
        while (!req.isDone)
        {
            await Task.Yield();
        }
        return (Sprite)req.asset;
    }

    private void OnAtlasAppendDone(string sprite, DynamicAtlasManager.eLoadResult result)
    {
        //根据自己的资源加载框架，处理散图的引用释放等逻辑
    }

}