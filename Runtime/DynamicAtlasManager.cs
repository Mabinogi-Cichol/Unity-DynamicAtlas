using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DynamicAtlas
{
    public class DynamicAtlasManager : MonoBehaviour
    {
        public enum eLoadResult
        {
            Success,
            Failure,
        }

        public struct Setting
        {
            public int ATLAS_SIZE;
            public int SINGLE_TEXTURE_MAX_SIZE;
            public int PADDING;
            public bool LOG;
            public TextureFormat AtlasFormat;
            public Func<string, Task<Sprite>> LoadSpriteFunc;
            public Action<string, eLoadResult> AtlasAppendDone;
        }

        public static DynamicAtlasManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var go = new GameObject("DynamicAtlasManager");
                    mInstance = go.AddComponent<DynamicAtlasManager>();
                    DontDestroyOnLoad(go);
                }
                return mInstance;
            }
        }

        private static bool mInitialized = false;
        private static DynamicAtlasManager mInstance;
        public static bool Initialized { get { return mInitialized; } }
        public static bool EDITOR_LOG { get; private set; }
        public static int ATLAS_SIZE { get; private set; } = 2048;
        public static int SINGLE_TEXTURE_MAX_SIZE { get; private set; } = 512;
        public static int PADDING { get; private set; } = 2;
        public static TextureFormat AtlasFormat { get; private set; } = TextureFormat.RGBA32;
        public static Func<string, Task<Sprite>> LoadSpriteFunc { get; private set; }
        public static Action<string, eLoadResult> AppendAtlasDone { get; private set; }
        private List<DynamicAtlas> mDynamicAtlases = new List<DynamicAtlas>();
        private Dictionary<string, DynamicAtlas> mSpriteToAtlas = new Dictionary<string, DynamicAtlas>();

        public static void Init(Setting setting)
        {
            if (mInitialized) return;
            EDITOR_LOG = setting.LOG;
            ATLAS_SIZE = setting.ATLAS_SIZE;
            SINGLE_TEXTURE_MAX_SIZE = setting.SINGLE_TEXTURE_MAX_SIZE;
            PADDING = setting.PADDING;
            AtlasFormat = setting.AtlasFormat;
            LoadSpriteFunc = setting.LoadSpriteFunc;
            AppendAtlasDone = setting.AtlasAppendDone;
            mInitialized = true;
        }

        private void LateUpdate()
        {
            bool createdNewAtlasThisFrame = false;
            for (int i = 0; i < mDynamicAtlases.Count; i++)
            {
                mDynamicAtlases[i].LateUpdate();

                // Overflow routing: if this atlas is full and has overflow textures,
                // ensure a new atlas exists for future allocations
                var atlas = mDynamicAtlases[i];
                if (atlas.IsFull)
                {
                    var overflowNames = atlas.OverflowTextureNames;
                    if (overflowNames != null && overflowNames.Count > 0)
                    {
                        if (!createdNewAtlasThisFrame)
                        {
                            // Create ONE new atlas to prime the pool for next allocations
                            GetDynamicAtlas();
                            createdNewAtlasThisFrame = true;
                        }
                    }
                }
            }
        }

        public DynamicAtlas GetDynamicAtlas()
        {
            for (int i = 0; i < mDynamicAtlases.Count; i++)
            {
                if (!mDynamicAtlases[i].IsFull)
                {
                    return mDynamicAtlases[i];
                }
            }
            var newAtlas = new DynamicAtlas();
            mDynamicAtlases.Add(newAtlas);
            return newAtlas;
        }

        public DynamicAtlas GetDynamicAtlas(int index)
        {
            if (index < mDynamicAtlases.Count)
                return mDynamicAtlases[index];

            // Create atlases to fill up to the requested index
            while (mDynamicAtlases.Count <= index)
            {
                var newAtlas = new DynamicAtlas();
                mDynamicAtlases.Add(newAtlas);
            }
            return mDynamicAtlases[index];
        }

        public async Task<Sprite> GetSprite(string spriteName, int atlasIndex, CancellationToken token)
        {
            spriteName = Path.GetFileNameWithoutExtension(spriteName);

            // Cache hit: sprite already assigned to an atlas
            if (mSpriteToAtlas.TryGetValue(spriteName, out var cachedAtlas))
            {
                return await cachedAtlas.GetSpriteAsync(spriteName, token);
            }

            // Select target atlas
            DynamicAtlas targetAtlas;
            if (atlasIndex == -1)
                targetAtlas = GetDynamicAtlas();
            else
                targetAtlas = GetDynamicAtlas(atlasIndex);

            // Try to load and pack, with retry for overflow
            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (token.IsCancellationRequested) return null;

                var result = await targetAtlas.GetSpriteAsync(spriteName, token);
                if (result != null)
                {
                    mSpriteToAtlas[spriteName] = targetAtlas;
                    return result;
                }

                // Check if this was an overflow failure
                var overflowNames = targetAtlas.OverflowTextureNames;
                if (overflowNames != null && overflowNames.Contains(spriteName))
                {
                    // Overflow: try again on a new atlas
                    targetAtlas = GetDynamicAtlas();
                    continue;
                }

                // Load failure or other non-overflow issue
                return null;
            }

            return null;
        }

        public void ReleaseSprite(string spriteName)
        {
            if (mSpriteToAtlas.TryGetValue(spriteName, out var atlas))
            {
                atlas.RemoveSprite(spriteName);
                mSpriteToAtlas.Remove(spriteName);
            }
        }
    }
}