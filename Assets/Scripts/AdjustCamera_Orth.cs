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
    //public Transform tt;
    //bg的size
    private Vector2 m_bgScaleSize = Vector2.one;
    public Vector2 BgScaleSize
    {
        get { return m_bgScaleSize; }
    }

    private Camera m_Camera;


    private float targetWidth = 10.8f;
    // Start is called before the first frame update
    void Start()
    {
        m_Camera = GetComponent<Camera>();
        //camera.orthographicSize = 1080.0f / 1920.0f;

        //m_bgScaleSize = new Vector2()
        targetWidth = 920 / 100.0f ;



        var hsize = Screen.height / 100.0f * 0.5f;
        var wsize = Screen.width / 100.0f * 0.5f;

        var realSize = ((targetWidth / 2.0f) / wsize) * hsize;
        //console.error("fzy aaa:", hsize, wsize, realSize, testW / 2.0f, testW);
        m_Camera.orthographicSize = realSize;

        //默认ui设计分辨率是1080*1920
        /*
         1080    = Screen.x
         jj*100  = 
         */


        var gbScaleX = (targetWidth * 100 * Screen.width) / 1080.0f;
        //console.error("fzy aaa a:", gbScaleX);
        gbScaleX = (gbScaleX / Screen.width);

        var gbScaleY = (realSize * 2 * 100 * Screen.height) / 1920.0f;
        //console.error("fzy aaa b:", gbScaleY);
        gbScaleY = (gbScaleY / Screen.height);
        m_bgScaleSize = new Vector2 (gbScaleX, gbScaleY);
        //tt.localScale = new Vector3(gbScaleX, gbScaleY, 1);
        //console.error("fzy aaax:", gbScaleX, gbScaleY);
        //camera.projectionMatrix = Matrix4x4.Ortho(vv.x, vv.y, vv.z, vv.w, 0.1f, 300);
    }

    // Update is called once per frame
    void Update()
    {


    }

}
