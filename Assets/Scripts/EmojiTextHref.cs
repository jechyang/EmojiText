using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiTextHref : Graphic
{
    private List<HrefInfo> _hrefInfos;
    private float _underLineHeight;

    public override Texture mainTexture { get => s_WhiteTexture; }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();
        if (_hrefInfos == null)
            return;
        for (int i = 0; i < _hrefInfos.Count; i++)
        {
            foreach (Rect rect in _hrefInfos[i].ClickRects)
            {
                var tmp = new UIVertex[4];
                tmp[0].position = rect.position;
                tmp[1].position = rect.position + new Vector2(rect.width, 0);
                tmp[2].position = tmp[1].position + new Vector3(0, _underLineHeight);
                tmp[3].position = rect.position + new Vector2(0, _underLineHeight);
                tmp[0].color = Color.red;
                tmp[1].color = Color.red;
                tmp[2].color = Color.red;
                tmp[3].color = Color.red;
                tmp[0].uv0 = new Vector2(0,0);
                tmp[1].uv0 = new Vector2(0,1);
                tmp[2].uv0 = new Vector2(1,1);
                tmp[3].uv0 = new Vector2(1,0);
                
                vh.AddUIVertexQuad(tmp);
            }
        }
    }

    public void SetHrefInfoAndLineHeight(List<HrefInfo> hrefInfos,float underLineHeight)
    {
        _hrefInfos = hrefInfos;
        _underLineHeight = underLineHeight;
    }
}
