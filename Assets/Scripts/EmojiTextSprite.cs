using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class EmojiTextSprite : Graphic
{
    private List<UIVertex> _vertices;
    public SpriteAtlas EmojiAtlas;
    public string AnySpriteName;

    public override Texture mainTexture
    {
        get
        {
            if (!HasSpriteAtlas)
                return base.mainTexture;
            return EmojiAtlas.GetSprite(AnySpriteName).texture;
        }
    }

    public Sprite GetSpriteByName(string spriteName)
    {
        var sprite = EmojiAtlas.GetSprite(spriteName);
        return sprite;
    }

    public bool HasSpriteAtlas => EmojiAtlas!=null;
    
    public void SetVertices(List<UIVertex> vertices)
    {
        _vertices = vertices;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if(_vertices == null)
            return;
        vh.Clear();
        for (int i = 0; i < _vertices.Count; i+=4)
        {
            vh.AddVert(_vertices[i]);
            vh.AddVert(_vertices[i+1]);
            vh.AddVert(_vertices[i+2]);
            vh.AddVert(_vertices[i+3]);
            vh.AddTriangle(i, i+1, i+2);
            vh.AddTriangle(i+2, i+3, i);
        }
        
    }
}
