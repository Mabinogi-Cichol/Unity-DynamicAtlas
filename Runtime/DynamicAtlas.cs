using DaVikingCode.RectanglePacking;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DynamicAtlas
{
    public struct TextureAsset
    {
        public string name { get; private set; }
        public int index { get; private set; }
        public int x { get; private set; }
        public int y { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }
        public TextureAsset(string name, int index, int x, int y, int width, int height)
        {
            this.name = name;
            this.index = index;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public class DynamicAtlas
    {
        private Texture2D mAtlas;
        private RectanglePacker mPacker;
        private List<string> mProcessTextureNames;
        private List<Texture2D> mProcessTextures;
        private List<int> mProcessTextureIds;

        public bool IsFull => mIsFull;

        private Dictionary<string, DynamicTextureData> mUsingTexture = new Dictionary<string, DynamicTextureData>();
        private Dictionary<string, Sprite> mSingleTexture = new Dictionary<string, Sprite>();
        private List<string> mNeedSingleTextures = new List<string>();
        private bool mIsFull;

        private bool mEditorLog = false;
        private int mCurrentPackedId;
        private bool mNeedProcessPack = false;
        public UnityEvent<string, Sprite> OnSpriteRePacked;

        public DynamicAtlas()
        {
#if UNITY_EDITOR
            mEditorLog = false;
#endif
            mAtlas = null;

            var texture_size = DynamicAtlasManager.ATLAS_SIZE;
            TextureFormat format = DynamicAtlasManager.AtlasFormat;

            mPacker = new RectanglePacker(texture_size, texture_size, DynamicAtlasManager.PADDING, 4);
            mAtlas = new Texture2D(texture_size, texture_size, format, false);
            mAtlas.Apply();

            mProcessTextureNames = new List<string>();
            mProcessTextures = new List<Texture2D>();
            mProcessTextureIds = new List<int>();
        }

        public void LateUpdate()
        {
            if (mNeedProcessPack)
            {
                ProcessPack();
                mNeedProcessPack = false;
            }
        }

        public bool AppendSprite(Sprite sprite)
        {

            var texture = sprite.texture;
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
            {

                Debug.LogWarning($"Cuurent Graphic Device:{SystemInfo.graphicsDeviceName}, API: {SystemInfo.graphicsDeviceType} NotSupport CopyTexture ! can not add to dynamic atlas");
                return false;
            }
            if (texture.format != mAtlas.format)
            {
                Debug.LogWarning($"texture: {texture.name} format is diff ,format:{texture.format}!");
                return false;
            }
            if (sprite.rect.width > DynamicAtlasManager.SINGLE_TEXTURE_MAX_SIZE || sprite.rect.height > DynamicAtlasManager.SINGLE_TEXTURE_MAX_SIZE)
            {
                Debug.LogWarning($"texture: {texture.name} size is outside {DynamicAtlasManager.SINGLE_TEXTURE_MAX_SIZE}, can not add to dynamic atlas");
                return false;
            }

            AddTextureToPack(sprite.name, sprite.texture);
            mNeedProcessPack = true;
            return true;
        }

        private void AddTextureToPack(string name, Texture2D texture)
        {
            if (mProcessTextureNames.Contains(name))
                return;
            mProcessTextureNames.Add(name);
            mProcessTextures.Add(texture);
            mProcessTextureIds.Add(mCurrentPackedId++);
        }

        private void ProcessPack()
        {
            for (int i = 0; i < mProcessTextures.Count; i++)
            {
                var texture = mProcessTextures[i];
                var id = mProcessTextureIds[i];
                mPacker.insertRectangle(texture.width, texture.height, id);
            }

            List<TextureAsset> textureAssets = new List<TextureAsset>();
            IntegerRectangle rect = new IntegerRectangle();
            int packedCount = mPacker.packRectangles();
            for (int i = 0; i < mProcessTextureIds.Count; i++)
            {
                var process_texture_name = mProcessTextureNames[i];
                var process_texture_id = mProcessTextureIds[i];
                var process_texture = mProcessTextures[i];
                bool added = false;
                for (int j = 0; j < mPacker.rectangleCount; j++)
                {
                    int id = mPacker.getRectangleId(j);
                    if (id != process_texture_id)
                        continue;
                    rect = mPacker.getRectangle(j, rect);
                    Graphics.CopyTexture(process_texture, 0, 0, 0, 0, rect.width, rect.height,
                        mAtlas, 0, 0, rect.x, rect.y);
                    TextureAsset textureAsset = new TextureAsset(process_texture_name, process_texture_id, rect.x, rect.y, rect.width, rect.height);
                    textureAssets.Add(textureAsset);
                    added = true;
                    break;
                }
                if (!added)
                {
                    mNeedSingleTextures.Add(process_texture_name);
                }
            }
            for (int i = 0; i < textureAssets.Count; i++)
            {
                var textureAsset = textureAssets[i];
                var sprite = Sprite.Create(mAtlas, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect);
                if (!mUsingTexture.TryGetValue(textureAsset.name, out var dynamicTextureData))
                {
                    dynamicTextureData = new DynamicTextureData(textureAsset.index, new IntegerRectangle(0, 0, textureAsset.width, textureAsset.height));
                    if (!mUsingTexture.TryAdd(textureAsset.name, dynamicTextureData))
                    {
                        Debug.LogError($"UsingTexture Add Failed ! {textureAsset.name} is Added !");
                    }
                }
                dynamicTextureData.SetSprite(sprite, textureAsset.name, textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height);
            }
            if (mEditorLog)
                Debug.Log($"DyanamicAtlas: ProcessPack Done, mPacker.rectangleCount: {packedCount}");

            mProcessTextureNames.Clear();
            mProcessTextures.Clear();
            mProcessTextureIds.Clear();
        }

        private Sprite GetSprite(string sprite_name)
        {
            if (mUsingTexture.TryGetValue(sprite_name, out var textureData))
            {
                textureData.AddReference();
                if (mEditorLog)
                    Debug.Log($"DyanamicAtlas: {sprite_name} AddReference, now referecneCount: {textureData.ReferenceCount}");
                return textureData.Sprite;
            }
            if (mSingleTexture.TryGetValue(sprite_name, out var handle))
            {
                if (mEditorLog)
                    Debug.Log($"DyanamicAtlas: {sprite_name} Add from SingleTexture");
                return handle;
            }
            Debug.LogError($"Not Found Sprite: {sprite_name} from DynamicAtlas UsingTextures!");
            return null;
        }

        public async Task<Sprite> GetSpriteAsync(string sprite_name, CancellationToken token)
        {
            sprite_name = Path.GetFileNameWithoutExtension(sprite_name);
            if (mEditorLog)
                Debug.Log($"DyanamicAtlas: Get Sprite: {sprite_name}");

            if (mProcessTextureNames.Contains(sprite_name))
            {
                while (mNeedProcessPack)
                {
                    await Task.Yield();
                }
            }
            if (mUsingTexture.TryGetValue(sprite_name, out var textureData))
            {
                return GetSprite(sprite_name);
            }
            else
            {
                var sprite = await LoadAssetAsync(sprite_name);
                if (token.IsCancellationRequested) return null;
                if (sprite != null)
                {
                    var success = AppendSprite(sprite);
                    if (!success)
                    {
                        mNeedSingleTextures.Add(sprite_name);
                    }
                    while (mNeedProcessPack)
                    {
                        await Task.Yield();
                        if (token.IsCancellationRequested) return null;
                    }
                    if (mNeedSingleTextures.Contains(sprite_name))
                    {
                        if (mSingleTexture.ContainsKey(sprite_name))
                        {
                            mSingleTexture.Remove(sprite_name);
                        }
                        mSingleTexture.TryAdd(sprite_name, sprite);
                        mNeedSingleTextures.Remove(sprite_name);
                    }
                    else
                    {
                        DynamicAtlasManager.AppendAtlasDone(sprite_name, DynamicAtlasManager.eLoadResult.Success);
                    }
                    if (token.IsCancellationRequested) return null;
                    return GetSprite(sprite_name);
                }
            }
            DynamicAtlasManager.AppendAtlasDone(sprite_name, DynamicAtlasManager.eLoadResult.Failure);
            return null;
        }

        public void RemoveSprite(string sprite_name)
        {
            if (mSingleTexture.ContainsKey(sprite_name))
            {
                mSingleTexture.Remove(sprite_name);
                if (mEditorLog)
                    Debug.Log($"DyanamicAtlas: {sprite_name} Remove from SingleTexture");
            }
            if (mUsingTexture.ContainsKey(sprite_name))
            {
                var textureData = mUsingTexture[sprite_name];
                textureData.RemoveReference();
                if (mEditorLog)
                    Debug.Log($"DyanamicAtlas: {sprite_name} RemoveReference, now referecneCount: {textureData.ReferenceCount}");
                if (textureData.ReferenceCount == 0)
                {
                    bool success = mPacker.releaseRectangle(textureData.Id);
                    if (mEditorLog)
                        Debug.Log($"DyanamicAtlas: Release {sprite_name} mPacker.rectangleCount: {mPacker.rectangleCount}");
                    if (!success)
                    {
                        Debug.LogError($"Release {sprite_name} from atlas Failed");
                        return;
                    }
                    mUsingTexture.Remove(sprite_name);
                }
            }
        }

        private async Task<Sprite> LoadAssetAsync(string assetName)
        {
            return await DynamicAtlasManager.LoadSpriteFunc(assetName);
        }
    }
}
