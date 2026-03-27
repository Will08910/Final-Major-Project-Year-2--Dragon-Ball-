using UnityEngine;

public class CameraFovController : MonoBehaviour
{
    public Camera cam;
    public float fov = 60f;

    void Start()
    {
       
    }


    void Update()
    {
        cam.fieldOfView = fov;
    }
}
