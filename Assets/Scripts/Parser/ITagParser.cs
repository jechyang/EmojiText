using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface ITagParser
{
    List<HrefInfo> HrefInfos { get; }
    List<EmojiInfo> EmojiInfos { get; }
    string ActuallyText { get; }
    string InputText { set; get; }
}
