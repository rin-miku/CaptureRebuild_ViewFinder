using System.Collections;
using System.Linq;
using UnityEngine;

public partial class CaptureRebuild : MonoBehaviour
{
    public Photo photo;
    public Camera viewCamera;
    public Camera skyboxCamera;
    public Transform captureRoot;
    public Transform rebuildRoot;
    public GameObject skyboxPanelPrefab;

    private Plane[] frustumPlanes = new Plane[6];

    public void Capture()
    {
        photo.transform.localPosition = Vector3.zero;
        photo.transform.localRotation = Quaternion.identity;
        captureRoot.localScale = Vector3.one;

        CaptureObjectsInFrustum();

        CaptureSkyboxInFrustum();

        photo.transform.localPosition = photo.pointerValues[0].position;
        photo.transform.localRotation = Quaternion.Euler(photo.pointerValues[0].rotation);
        captureRoot.localScale = Vector3.zero;
    }

    public void Rebuild()
    {
        GameObject tempRebuild = Instantiate(captureRoot.gameObject, rebuildRoot, true);
        tempRebuild.transform.localScale = Vector3.one;
    }

    private void CaptureObjectsInFrustum()
    {
        foreach (Transform child in captureRoot) Destroy(child.gameObject);

        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(viewCamera);

        IEnumerable capturableObjects =
            FindObjectsOfType<Capturable>().Where(a =>
            {
                return a.TryGetComponent(out Renderer renderer) &&
                renderer != null &&
                GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
            });

        foreach (Capturable capturableObject in capturableObjects)
        {
            GameObject temp = Instantiate(capturableObject.gameObject, captureRoot, true);
            temp.transform.position = capturableObject.transform.position;
            temp.transform.rotation = capturableObject.transform.rotation;

            Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
            foreach(Plane plane in frustumPlanes)
            {
                CutMesh(mesh, plane, capturableObject.transform);
            }

            Destroy(temp.GetComponent<MeshCollider>());
            temp.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    private void CaptureSkyboxInFrustum()
    {
        Texture2D skyboxTexture = CommonTools.GetCameraTexture(skyboxCamera);

        Instantiate(skyboxPanelPrefab, captureRoot).GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", skyboxTexture);
    }
}
