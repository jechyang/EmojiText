using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class EmojiInfo
{
    public int Index;
    public string EmojiName;
    public int LineCount;
    public int SkipCount;
    public Vector2 Size;

    public void RefreshSkipCount()
    { 
        this.Index -= this.SkipCount;
    }
}

public class HrefInfo
{
    public int Index;
    public int EventId;
    public List<Rect> ClickRects = new List<Rect>();
    public int LineCount;
    public int SkipCount;
    public int StrCount;
    
    public void RefreshSkipCount()
    {
        this.Index -= this.SkipCount;
    }
}

public class EmojiText : Text, IPointerClickHandler
{
    
    private Dictionary<int, Action> _hrefActionDic = new Dictionary<int, Action>();
    
    private ITagParser _curentTagParser;
    private OldVersionTagParser _oldVersionTagParser = new OldVersionTagParser();
    private NewVersionTagParser _newVersionTagParser = new NewVersionTagParser();

    #region 重写text的一些布局方法，使用消除符号之后的text进行计算顶点和布局宽和高
    private TextGenerator m_TextCache;
    private TextGenerator m_TextCacheForLayout;
    public TextGenerator cachedTextGenerator
    {
        get { return m_TextCache ?? (m_TextCache = (_curentTagParser.ActuallyText.Length != 0 ? new TextGenerator(_curentTagParser.ActuallyText.Length) : new TextGenerator())); }
    }
    
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(_curentTagParser.ActuallyText, settings) / pixelsPerUnit;
        }
    }
    
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(_curentTagParser.ActuallyText, settings) / pixelsPerUnit;
        }
    }

    #endregion
    
    public override string text
    {
        get { return base.text; }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                SetVerticesDirty();
            }
            else if (m_Text != value)
            {
                m_Text = value;
                _curentTagParser = _oldVersionTagParser;
                _curentTagParser.InputText = m_Text;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }
    
    public override void SetLayoutDirty()
    {
        base.SetLayoutDirty();
        EmojiSpriteComp.SetLayoutDirty();
        EmojiTextHrefComp.SetLayoutDirty();
    }
    private EmojiTextSprite _emojiSpriteComp;

    private EmojiTextSprite EmojiSpriteComp =>
        _emojiSpriteComp ?? (_emojiSpriteComp = GetComponentInChildren<EmojiTextSprite>());

    private EmojiTextHref _emojiTextHrefComp;

    public EmojiTextHref EmojiTextHrefComp =>
        _emojiTextHrefComp ?? (_emojiTextHrefComp = GetComponentInChildren<EmojiTextHref>());
    
    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        EmojiSpriteComp.SetVerticesDirty();
        EmojiTextHrefComp.SetVerticesDirty();
    }

    readonly UIVertex[] m_TempVerts = new UIVertex[4];
    
    //本来是不想重写这个方法的，但是因为要用消除自定义符号后的text去进行生成顶点信息，重写text的getter又会增加使用的理解成本。
    private void BaseOnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        var actuallyText = _curentTagParser.ActuallyText;
        cachedTextGenerator.PopulateWithErrors(actuallyText, settings, gameObject);
        
        
        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        int vertCount;
#if UNITY_2019_1_OR_NEWER
        vertCount = verts.Count;
#else
        vertCount = verts.Count - 4;
#endif
        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }

        m_DisableFontTextureRebuiltCallback = false;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        BaseOnPopulateMesh(toFill);
#if UNITY_2019_1_OR_NEWER
        var isNewCalculateWay = CheckIsNewCalculateWay();
        if (isNewCalculateWay)
        {
            _curentTagParser = _newVersionTagParser;
            _curentTagParser.InputText = m_Text;
        }
#endif
        var vertices = GetCurrentVertices(toFill);
        try
        {
            DrawSprite(toFill, vertices);
            DrawHref(toFill, vertices);
            ClearQuadShowError(toFill);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    private bool CheckIsNewCalculateWay()
    {
        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        var actuallyText = _curentTagParser.ActuallyText;
        cachedTextGenerator.PopulateWithErrors(actuallyText, settings, gameObject);
        
        var allTextLineCount = Regex.Matches(_curentTagParser.ActuallyText, @"[\r\n]").Count + 1;
        return (allTextLineCount == cachedTextGenerator.lineCount);
    }
    
    private void DrawHref(VertexHelper vh, List<UIVertex> vertices)
    {
        var bounds = new Bounds();
        foreach (HrefInfo info in _curentTagParser.HrefInfos)
        {
            info.ClickRects.Clear();
            var lineCount = info.LineCount;
            var firstVertexIndex = (info.Index - lineCount) * 4;
            bounds = new Bounds(vertices[firstVertexIndex].position, Vector3.zero);

            for (int i = 1; i < info.StrCount; i++)
            {
                //左上角的点
                var nextLeftUpVertex = vertices[firstVertexIndex + i * 4];
                //右下角的点
                var nextRightBottomVertex = vertices[firstVertexIndex + i * 4 + 2];
                if (nextLeftUpVertex.position.x < bounds.center.x)
                {
                    info.ClickRects.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(nextLeftUpVertex.position, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(nextRightBottomVertex.position);
                }
            }
            bounds.Encapsulate(vertices[firstVertexIndex+(info.StrCount-1)*4+2].position);
            info.ClickRects.Add(new Rect(bounds.min, bounds.size));
        }

        EmojiTextHrefComp.SetHrefInfoAndLineHeight(_curentTagParser.HrefInfos, fontSize * 0.1f);
    }
    
    //这里quad会有乱码，因此需要清除uv
    private void ClearQuadShowError(VertexHelper vh)
    {
        foreach (EmojiInfo info in _curentTagParser.EmojiInfos)
        {
            for (int i = (info.Index - info.LineCount) * 4; i < (info.Index - info.LineCount) * 4 + 4; i++)
            {
                UIVertex tmp = new UIVertex();
                vh.PopulateUIVertex(ref tmp, i);
                tmp.uv0 = Vector2.zero;
                vh.SetUIVertex(tmp, i);
            }
        }
    }
    
    private List<UIVertex> GetCurrentVertices(VertexHelper vh)
    {
        List<UIVertex> result = new List<UIVertex>();
        UIVertex tmp = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref tmp, i);
            result.Add(tmp);
        }

        return result;
    }


    private void DrawSprite(VertexHelper vh, List<UIVertex> vertices)
    {
        if (EmojiSpriteComp == null || !EmojiSpriteComp.HasSpriteAtlas)
        {
            return;
        }
        
        List<UIVertex> uiVertices = new List<UIVertex>();
        Vector3[] spriteVertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        foreach (EmojiInfo info in _curentTagParser.EmojiInfos)
        {
            var emojiName = info.EmojiName;
            var sprite = EmojiSpriteComp.GetSpriteByName(emojiName);
            if (sprite == null)
            {
                throw new Exception($"图集里无法找到指定name:{emojiName}的sprite");
            }
            var uv = DataUtility.GetOuterUV(sprite);
            var lineCount = info.LineCount;
            var firstVertexIndex = (info.Index + 1 - lineCount) * 4 - 1;
            var textPos = vertices[firstVertexIndex].position;
            spriteVertices[0] = textPos;
            spriteVertices[1] = textPos + new Vector3(0, info.Size.y);
            spriteVertices[2] = textPos + new Vector3(info.Size.x, info.Size.y);
            spriteVertices[3] = textPos + new Vector3(info.Size.x, 0);
            uvs[0] = new Vector2(uv.x, uv.y);
            uvs[1] = new Vector2(uv.x, uv.w);
            uvs[2] = new Vector2(uv.z, uv.w);
            uvs[3] = new Vector2(uv.z, uv.y);

            UIVertex tmp;
            for (int i = 0; i < 4; i++)
            {
                tmp = new UIVertex();
                tmp.position = spriteVertices[i];
                tmp.uv0 = uvs[i];
                tmp.color = color;
                uiVertices.Add(tmp);
            }
        }

        EmojiSpriteComp.SetVertices(uiVertices);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);
        foreach (HrefInfo info in _curentTagParser.HrefInfos)
        {
            foreach (Rect clickRect in info.ClickRects)
            {
                if (clickRect.Contains(lp))
                {
                    if (_hrefActionDic.TryGetValue(info.EventId, out var action))
                    {
                        action.Invoke();
                    }
                }
            }
        }
    }
    
    public void AddClickListener(int eventId, Action action)
    {
        if (_hrefActionDic.ContainsKey(eventId))
        {
            _hrefActionDic[eventId] = action;
        }
        else
        {
            _hrefActionDic.Add(eventId, action);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (!IsActive())
        {
            base.OnValidate();
        }

        _curentTagParser = _oldVersionTagParser;
        _curentTagParser.InputText = m_Text;   
        base.OnValidate();
    }
#endif

}