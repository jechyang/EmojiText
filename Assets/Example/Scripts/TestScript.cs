using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private EmojiText _emojiText;

    private void Awake()
    {
        _emojiText = GetComponent<EmojiText>();
        _emojiText.AddClickListener(1, () =>
        {
            Debug.LogError("hello");
        });
        _emojiText.AddClickListener(2, () =>
        {
            Debug.LogError("world");
        });
    }
}
