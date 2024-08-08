using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 只支持正交相机调整
/// </summary>
public class AdjustCamera_Orth : MonoBehaviour
{
    public GameObject adjustReferenceObject;

    //public Vector4 vv = new Vector4(-10, 10, -20, 20);
    //bg的size
    private Vector2 m_bgScaleSize = Vector2.one;
    public Vector2 BgScaleSize
    {
        get { return m_bgScaleSize; }
    }

    private Camera m_Camera;
    // Start is called before the first frame update
    void Start()
    {
        m_Camera = GetComponent<Camera>();
        //camera.orthographicSize = 1080.0f / 1920.0f;
        var hsize = Screen.height / 100.0f * 0.5f;
        var wsize = Screen.width / 100.0f * 0.5f;

        var realSize = ((adjustReferenceObject.transform.localScale.x / 2.0f) / wsize) * hsize;
        //console.error("fzy aaa:", hsize, wsize, realSize, adjustReferenceObject.transform.localScale.x / 2.0f, adjustReferenceObject.transform.localScale.x);
        m_Camera.orthographicSize = realSize;

        //默认ui设计分辨率是1080*1920

        var gbScaleX = (adjustReferenceObject.transform.localScale.x * 100 * Screen.width) / 1080.0f;
        gbScaleX = (gbScaleX / 1080.0f);

        var gbScaleY = (realSize * 2 * 100 * Screen.height) / 1920.0f;
        gbScaleY = (gbScaleY / 1920.0f);
        m_bgScaleSize = new Vector2(gbScaleX, gbScaleY);
    }

    // Update is called once per frame
    void Update()
    {


        //console.error("fzy aaax:", gbScaleX, gbScaleY);
        //camera.projectionMatrix = Matrix4x4.Ortho(vv.x, vv.y, vv.z, vv.w, 0.1f, 300);
    }

}
