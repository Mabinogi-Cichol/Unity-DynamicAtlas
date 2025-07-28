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
            AtlasAppendDone = OnAtlasAppendDone,
// You can set the texture format here according to the platform used in your project
#if UNITY_STANDALONE
            AtlasFormat = TextureFormat.BC7,
#elif UNITY_ANDROID
            AtlasFormat = TextureFormat.ASTC_4x4,
#elif UNITY_IOS
            AtlasFormat = TextureFormat.ASTC_4x4,
#elif UNITY_PS5
            AtlasFormat = TextureFormat.DXT5,
#else
            AtlasFormat = TextureFormat.RGBA32,
#endif
        });
    }

    private async Task<Sprite> LoadSpriteAsync(string sprite)
    {
        // You can replace this with your own resource loading logic
        var req = Resources.LoadAsync(sprite);
        while (!req.isDone)
        {
            await Task.Yield();
        }
        return (Sprite)req.asset;
    }

    private void OnAtlasAppendDone(string sprite, DynamicAtlasManager.eLoadResult result)
    {
        // Handle the logic of releasing references to individual textures according to your own resource loading framework
    }

}