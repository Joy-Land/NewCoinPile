using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;

/// <summary>
/// 只支持透视相机调整
/// </summary>
public class AdjustCamera : MonoBehaviour
{
    public GameObject adjustReferenceObject;

    private class FrustumCorner
    {
        public Vector3[] Near = new Vector3[4];
        public Vector3[] Far = new Vector3[4];
        public static FrustumCorner Copy(FrustumCorner corners)
        {
            FrustumCorner temp = new FrustumCorner();
            for (int i = 0; i < 4; i++)
            {
                temp.Near[i] = new Vector3(corners.Near[i].x, corners.Near[i].y, corners.Near[i].z);
                temp.Far[i] = new Vector3(corners.Far[i].x, corners.Far[i].y, corners.Far[i].z);
            }
            return temp;
        }
    }


    private Vector2[] m_drawOrder = new Vector2[4] { new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, 1), new Vector2(1, -1) };
    // Start is called before the first frame update
    void Start()
    {
        AdjustCameraDistance();
    }

    private void DrawAABB(FrustumCorner debugCor)
    {
#if !UNITY_EDITOR
    return;
#endif
        Debug.DrawLine(debugCor.Near[0], debugCor.Near[1], Color.magenta);
        Debug.DrawLine(debugCor.Near[1], debugCor.Near[2], Color.magenta);
        Debug.DrawLine(debugCor.Near[2], debugCor.Near[3], Color.magenta);
        Debug.DrawLine(debugCor.Near[3], debugCor.Near[0], Color.magenta);

        Debug.DrawLine(debugCor.Far[0], debugCor.Far[1], Color.blue);
        Debug.DrawLine(debugCor.Far[1], debugCor.Far[2], Color.blue);
        Debug.DrawLine(debugCor.Far[2], debugCor.Far[3], Color.blue);
        Debug.DrawLine(debugCor.Far[3], debugCor.Far[0], Color.blue);

        Debug.DrawLine(debugCor.Far[0], debugCor.Near[0], Color.blue);
        Debug.DrawLine(debugCor.Far[1], debugCor.Near[1], Color.blue);
        Debug.DrawLine(debugCor.Far[2], debugCor.Near[2], Color.blue);
        Debug.DrawLine(debugCor.Far[3], debugCor.Near[3], Color.blue);
    }
    private void DrawAABB(FrustumCorner debugCor, Matrix4x4 mat)
    {
#if !UNITY_EDITOR
    return;
#endif
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Near[0]), mat.MultiplyPoint(debugCor.Near[1]), Color.magenta);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Near[1]), mat.MultiplyPoint(debugCor.Near[2]), Color.magenta);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Near[2]), mat.MultiplyPoint(debugCor.Near[3]), Color.magenta);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Near[3]), mat.MultiplyPoint(debugCor.Near[0]), Color.magenta);

        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[0]), mat.MultiplyPoint(debugCor.Far[1]), Color.blue);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[1]), mat.MultiplyPoint(debugCor.Far[2]), Color.blue);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[2]), mat.MultiplyPoint(debugCor.Far[3]), Color.blue);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[3]), mat.MultiplyPoint(debugCor.Far[0]), Color.blue);

        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[0]), mat.MultiplyPoint(debugCor.Near[0]), Color.blue);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[1]), mat.MultiplyPoint(debugCor.Near[1]), Color.blue);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[2]), mat.MultiplyPoint(debugCor.Near[2]), Color.blue);
        Debug.DrawLine(mat.MultiplyPoint(debugCor.Far[3]), mat.MultiplyPoint(debugCor.Near[3]), Color.blue);
    }

    public float adjustUnit = 5;

    bool preAdjustIsForward = false;

    public void AdjustCameraDistance(int adjustCount = 10)
    {
        var camera = Camera.main;
        float nearPlane = camera.nearClipPlane;
        float farPlane = camera.farClipPlane;
        float height = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        float width = height * camera.aspect;
        for (int i = 0; i < adjustCount; i++)
        {

            Matrix4x4 cameraLocal2World = camera.transform.localToWorldMatrix;

            adjustReferenceObject.transform.rotation = camera.transform.rotation;

            FrustumCorner m_cameraCorners = new FrustumCorner();
            for (int j = 0; j < 4; j++)
            {
                var splitNear = new Vector3(width * m_drawOrder[j].x, height * m_drawOrder[j].y, 1) * nearPlane;
                var splitFar = new Vector3(width * m_drawOrder[j].x, height * m_drawOrder[j].y, 1) * farPlane;

                m_cameraCorners.Near[j] = cameraLocal2World.MultiplyPoint(splitNear);
                m_cameraCorners.Far[j] = cameraLocal2World.MultiplyPoint(splitFar);
                //m_cameraCorners.Near[j] = camera.transform.TransformPoint(splitNear);
                //m_cameraCorners.Far[j] = camera.transform.TransformPoint(splitFar);
            }



            var bounds = adjustReferenceObject.GetComponent<MeshRenderer>().bounds;
            var casterBoundVertsForwardMesh = new FrustumCorner();
            casterBoundVertsForwardMesh.Near[0] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(-1, -1, -1))));
            casterBoundVertsForwardMesh.Near[1] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(-1, 1, -1))));
            casterBoundVertsForwardMesh.Near[2] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(1, 1, -1))));
            casterBoundVertsForwardMesh.Near[3] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(1, -1, -1))));
            casterBoundVertsForwardMesh.Far[0] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(-1, -1, 1))));
            casterBoundVertsForwardMesh.Far[1] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(-1, 1, 1))));
            casterBoundVertsForwardMesh.Far[2] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(1, 1, 1))));
            casterBoundVertsForwardMesh.Far[3] = (bounds.center + Vector3.Scale(bounds.extents, Matrix4x4.Rotate(camera.transform.rotation).MultiplyPoint(new Vector3(1, -1, 1))));
            DrawAABB(casterBoundVertsForwardMesh);

            Plane leftP = new Plane(m_cameraCorners.Near[0], m_cameraCorners.Near[1], m_cameraCorners.Far[1]);
            Plane rightP = new Plane(m_cameraCorners.Far[3], m_cameraCorners.Near[2], m_cameraCorners.Near[3]);

            //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            List<Vector3> leftPList = new List<Vector3>();
            leftPList.Add(casterBoundVertsForwardMesh.Near[0]);
            leftPList.Add(casterBoundVertsForwardMesh.Near[1]);
            leftPList.Add(casterBoundVertsForwardMesh.Far[0]);
            leftPList.Add(casterBoundVertsForwardMesh.Far[1]);

            List<Vector3> rightPList = new List<Vector3>();
            rightPList.Add(casterBoundVertsForwardMesh.Near[2]);
            rightPList.Add(casterBoundVertsForwardMesh.Near[3]);
            rightPList.Add(casterBoundVertsForwardMesh.Far[2]);
            rightPList.Add(casterBoundVertsForwardMesh.Far[3]);


            var leftRes = true;
            var rightRes = true;

            float leftD = leftP.GetDistanceToPoint(leftPList[0]);
            float rightD = rightP.GetDistanceToPoint(rightPList[0]);
            leftRes = leftD > 0;
            rightRes = rightD > 0;


            if (leftD > 0 && rightD > 0)
            {
                if (preAdjustIsForward == true)
                {
                    camera.transform.position += camera.transform.forward * adjustUnit;
                }
                else
                {
                    adjustUnit /= 2.0f;
                    camera.transform.position += camera.transform.forward * adjustUnit;
                }
                preAdjustIsForward = true;

            }
            else if (leftD < 0 && rightD < 0) 
            {
                if (preAdjustIsForward == false)
                {
                    camera.transform.position -= camera.transform.forward * adjustUnit;
                }
                else
                {
                    adjustUnit /= 2.0f;
                    camera.transform.position += camera.transform.forward * adjustUnit;
                }
                preAdjustIsForward = false;
            }
            else 
            {
                camera.transform.position += camera.transform.forward * adjustUnit;
            }

            console.error(adjustUnit, leftRes, rightRes);
        }
    }
}
