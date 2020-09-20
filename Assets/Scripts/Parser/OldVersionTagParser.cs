using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class OldVersionTagParser : ITagParser
{
    private string _inputText = string.Empty;
    private List<EmojiInfo> _emojiInfos = new List<EmojiInfo>();
    public List<EmojiInfo> EmojiInfos => _emojiInfos;
    private List<HrefInfo> _hrefInfos = new List<HrefInfo>();
    public List<HrefInfo> HrefInfos => _hrefInfos;
    private StringBuilder _actuallyTextBuilder = new StringBuilder();

    public string InputText
    {
        get => _inputText;
        set
        {
            _inputText = value;
            try
            {
                ParseText();
            }
            catch (Exception e)
            {
                Debug.LogError("标签解析失败，请查看标签格式");
                _emojiInfos.Clear();
                _hrefInfos.Clear();
            }
        }
    }

    public string ActuallyText => _actuallyTextBuilder.ToString();

    private void ParseText()
    {
        _emojiInfos.Clear();
        _hrefInfos.Clear();
        _actuallyTextBuilder.Clear();
        _actuallyTextBuilder.Append(_inputText);
        var matches = Consts.TagRegex.Matches(_inputText);
        int currentAddCount = 0;
        foreach (Match match in matches)
        {
            char typeChar = char.Parse(match.Groups[1].Value);
            switch (typeChar)
            {
                case 'E':
                    currentAddCount += ParseEmojiText(match, currentAddCount);
                    break;
                case 'H':
                    currentAddCount -= ParseHrefText(match, currentAddCount);
                    break;
                default:
                    Debug.LogError($"无法解析的type:{typeChar}");
                    break;
            }
        }   
    }
    
    private int ParseEmojiText(Match match, int currentJumpCount)
    {
        var paramsStr = match.Groups[2].Value;
        var paramsArr = paramsStr.Split(' ');
        int size = int.Parse(paramsArr[0]);
        string emojiName = paramsArr[1];
        if (string.IsNullOrEmpty(emojiName))
        {
            throw new Exception();
        }
        var tmpInfo = new EmojiInfo();
        tmpInfo.Index = match.Index + currentJumpCount;
        tmpInfo.Size = new Vector2(size, size);
        tmpInfo.EmojiName = emojiName;
        _emojiInfos.Add(tmpInfo);
        var targetStr = $"<quad size={size} />";
        _actuallyTextBuilder.Replace(match.Value, targetStr);

        return targetStr.Length - match.Value.Length;
    }
    
    private int ParseHrefText(Match match,int currentJumpCount)
    {
        var paramsStr = match.Groups[2].Value;
        var paramsArr = paramsStr.Split(' ');
        var showResult = paramsArr[0];
        int eventId = 999;
        if (paramsArr.Length > 1)
            eventId = int.Parse(paramsArr[1]);
        var tmp = new HrefInfo();
        tmp.Index = match.Index + currentJumpCount;
        tmp.EventId = eventId;
        tmp.StrCount = showResult.Length;
        _hrefInfos.Add(tmp);
        _actuallyTextBuilder.Replace(match.Value, showResult);
        return (match.Value.Length - showResult.Length);
    }
}
