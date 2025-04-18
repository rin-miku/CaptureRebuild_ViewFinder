using System.Collections;
using System.Linq;
using UnityEngine;

public partial class CaptureRebuild : MonoBehaviour
{
    public Camera viewCamera;
    public Camera skyboxCamera;
    public Transform captureRoot;
    public Transform rebuildRoot;
    public GameObject skyboxPanelPrefab;

    private Plane[] frustumPlanes = new Plane[6];

    public void Capture()
    {
        captureRoot.localScale = Vector3.one;

        CutObjectMeshInFrustum();

        CutSkyboxInFrustum();

        captureRoot.localScale = Vector3.zero;
    }

    public void Rebuild()
    {
        DestroyObjectMeshInFrustum();

        GameObject tempRebuild = Instantiate(captureRoot.gameObject, rebuildRoot, true);
        tempRebuild.transform.localScale = Vector3.one;
        foreach(Transform child in tempRebuild.transform)
        {
            child.GetComponent<Capturable>().isCapturable = true;
        }
    }

    private void CutObjectMeshInFrustum()
    {
        for(int i = 0; i < captureRoot.childCount; i++)
        {
            Destroy(captureRoot.GetChild(i).gameObject);
        }

        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(viewCamera);

        IEnumerable capturableObjects =
            FindObjectsOfType<Capturable>().Where(a =>
            {
                return a.GetComponent<Capturable>().isCapturable &&
                a.TryGetComponent(out Renderer renderer) &&
                renderer != null &&
                GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
            });

        foreach (Capturable capturableObject in capturableObjects)
        {
            GameObject temp = Instantiate(capturableObject.gameObject, captureRoot, true);
            temp.transform.position = capturableObject.transform.position;
            temp.transform.rotation = capturableObject.transform.rotation;
            temp.GetComponent<Capturable>().isCapturable = false;

            Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
            foreach(Plane plane in frustumPlanes)
            {
                CutMesh(mesh, plane, temp.transform);
            }

            Destroy(temp.GetComponent<MeshCollider>());
            temp.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    private void CutSkyboxInFrustum()
    {
        Texture2D skyboxTexture = CommonTools.GetCameraTexture(skyboxCamera);

        GameObject skyboxPanel = Instantiate(skyboxPanelPrefab, captureRoot);
        skyboxPanel.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", skyboxTexture);
        skyboxPanel.AddComponent<Capturable>().isCapturable = false;
    }

    private void DestroyObjectMeshInFrustum()
    {
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(viewCamera);

        IEnumerable capturableObjects =
            FindObjectsOfType<Capturable>().Where(a =>
            {
                return a.GetComponent<Capturable>().isCapturable &&
                a.TryGetComponent(out Renderer renderer) &&
                renderer != null &&
                GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
            });

        GameObject tempCaptureRoot = new GameObject("TempCaptureRoot");
        tempCaptureRoot.transform.parent = rebuildRoot;
        foreach (Capturable capturableObject in capturableObjects)
        {
            foreach (Plane plane in frustumPlanes)
            {
                GameObject temp = Instantiate(capturableObject.gameObject, tempCaptureRoot.transform, true);
                temp.transform.position = capturableObject.transform.position;
                temp.transform.rotation = capturableObject.transform.rotation;

                Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
                CutMesh(mesh, plane.flipped, temp.transform, true);

                Destroy(temp.GetComponent<MeshCollider>());
                temp.AddComponent<MeshCollider>().sharedMesh = mesh; 
            }
            Destroy(capturableObject.gameObject);
        }
    }
}
