using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleOutline : Outline
{
    //斜向圆形折扣比例
    private readonly float diagonalRotia = 0.707f;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        //改变偏移数据生成8倍的顶点数据
        int start = 0;
        int end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, effectDistance.x * diagonalRotia, effectDistance.y * diagonalRotia);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, effectDistance.x, 0);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, effectDistance.x * diagonalRotia, -effectDistance.y * diagonalRotia);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, 0, -effectDistance.y);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, -effectDistance.x * diagonalRotia, -effectDistance.y * diagonalRotia);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, -effectDistance.x, 0);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, -effectDistance.x * diagonalRotia, effectDistance.y * diagonalRotia);

        start = end;
        end = verts.Count;
        ApplyShadow(verts, effectColor, start, verts.Count, 0, effectDistance.y);

        //重添加到原始数据中
        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);

        //清理
        verts.Clear();
        verts = null;
    }
}

