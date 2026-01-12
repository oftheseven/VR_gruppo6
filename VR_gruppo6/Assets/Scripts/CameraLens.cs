using UnityEngine;

// script da associare ad ogni lente della camera
public class CameraLens : MonoBehaviour
{
    [Header("Lens properties")]
    [SerializeField] private float focalLength = 24f;
    [SerializeField] private GameObject lensModel; // modello 3D della lente

    public float FocalLength => focalLength;
    public GameObject LensModel => lensModel;

    public void ApplyToCamera(Camera cam)
    {
        if (cam != null)
        {
            cam.focalLength = focalLength;
        }
    }
}
