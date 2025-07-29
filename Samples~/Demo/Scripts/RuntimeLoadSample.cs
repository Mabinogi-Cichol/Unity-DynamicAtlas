using DynamicAtlas;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeLoadSample : MonoBehaviour
{
    private string[] mSpriteNames = new string[]
    {
        "01",
        "02",
        "03",
        "04",//bad sample for texture format
        "05",
    };
    private DynamicImage mDynamicImage;
    private int mIndex;

    // Start is called before the first frame update
    void Start()
    {
        mDynamicImage = GetComponent<DynamicImage>();
        StartCoroutine(ReplaceSprite());
    }

    private IEnumerator ReplaceSprite()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (mIndex >= mSpriteNames.Length)
                mIndex = 0;
            mDynamicImage.SetDynamicSprite(mSpriteNames[mIndex++]);
        }
    }
}
