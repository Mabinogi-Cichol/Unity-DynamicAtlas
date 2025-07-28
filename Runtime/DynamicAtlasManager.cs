using System;
using System.Collections;
using System.Collections.Generic;
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
        private static DynamicAtlasManager mInstance;
        public static int ATLAS_SIZE { get; private set; } = 2048;
        public static int SINGLE_TEXTURE_MAX_SIZE { get; private set; } = 512;
        public static int PADDING { get; private set; } = 2;
        public static TextureFormat AtlasFormat { get; private set; } = TextureFormat.RGBA32;
        public static Func<string, Task<Sprite>> LoadSpriteFunc { get; private set; }
        public static Action<string, eLoadResult> AppendAtlasDone { get; private set; }
        private List<DynamicAtlas> mDynamicAtlases = new List<DynamicAtlas>();

        public static void Init(Setting setting)
        {
            ATLAS_SIZE = setting.ATLAS_SIZE;
            SINGLE_TEXTURE_MAX_SIZE = setting.SINGLE_TEXTURE_MAX_SIZE;
            PADDING = setting.PADDING;
            AtlasFormat = setting.AtlasFormat;
            LoadSpriteFunc = setting.LoadSpriteFunc;
            AppendAtlasDone = setting.AtlasAppendDone;
        }

        private void LateUpdate()
        {
            for (int i = 0; i < mDynamicAtlases.Count; i++)
            {
                mDynamicAtlases[i].LateUpdate();
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
    }
}