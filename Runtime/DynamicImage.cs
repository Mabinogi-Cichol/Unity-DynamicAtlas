using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicAtlas
{
    [AddComponentMenu("UI/Dynamic Image", 11)]
    public class DynamicImage : Image
    {
        private static float ICON_ASYNC_FADE_TIME = 0.1f;
        private static float NEED_FADE_INTERVAL_TIME = 0.05f;
        private DynamicAtlas mDynamicAtlas;

        private bool mInited;
        private CancellationTokenSource mCancellation;

        protected override void Start()
        {
            base.Start();
            Init();
        }

        protected override void OnDestroy()
        {
            if (Application.isPlaying && mInited)
                ReleaseSprite();
            base.OnDestroy();
        }

        private void Init()
        {
            if (mInited) return;
            if (Application.isPlaying)
            {
                mDynamicAtlas = DynamicAtlasManager.Instance.GetDynamicAtlas();
                SetDefaultSprite();
                mInited = true;
            }
        }

        private void SetDefaultSprite()
        {
            if (sprite != null)
            {
                mDynamicAtlas.AppendSprite(sprite);
                if (mCancellation != null)
                {
                    mCancellation.Cancel();
                    mCancellation = null;
                }
                mCancellation = new CancellationTokenSource();
                var token = mCancellation.Token;
                SetSpriteAsync(sprite.name, token)/*.Forget()*/;
            }
        }

        public void SetDynamicSprite(string spriteName)
        {
            Init();
            ReleaseSprite();
            if (mCancellation != null)
            {
                mCancellation.Cancel();
                mCancellation = null;
            }
            mCancellation = new CancellationTokenSource();
            var token = mCancellation.Token;
            SetSpriteAsync(spriteName, token)/*.Forget()*/;
        }

        private async Task SetSpriteAsync(string spriteName, CancellationToken token)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return;
            }
            CrossFadeAlpha(0, 0, true);
            var start_time = Time.unscaledTime;
            var sprite = await mDynamicAtlas.GetSpriteAsync(spriteName,  token);
            if (token.IsCancellationRequested) return;
            var end_time = Time.unscaledTime;
            CrossFadeAlpha(1, end_time - start_time > NEED_FADE_INTERVAL_TIME ? ICON_ASYNC_FADE_TIME : 0, true);
            if (sprite != null)
            {
                this.sprite = sprite;
            }
        }

        public void ReleaseSprite()
        {
            if (mCancellation != null)
            {
                mCancellation.Cancel();
                mCancellation = null;
            }
            if (sprite == null)
                return;
            mDynamicAtlas.RemoveSprite(sprite.name);
            sprite = null;
        }
    }
}