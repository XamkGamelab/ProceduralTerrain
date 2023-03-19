using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CameraController : MonoBehaviour
{
    public Camera Cam { get; private set; }
    public float ZoomMinY = 10f;
    public float ZoomMaxY = 200f;
    public float ZoomSpeed = 60f;
    public float ZoomInterpolationSpeed = 80f;
    public float PanDragMultiplier = .1f;
    public float PanInterpolationSpeed = 10f;

    private Vector3 cameraNextPosition;
    
    private Vector3 mouseDownPosition = Vector3.zero;
    private Vector3 mouseDownCameraPosition = Vector3.zero;
    private Vector3 mouseDownDelta = Vector3.zero;

    private bool mouseMiddleDrag = false;
    private CompositeDisposable disposables = new CompositeDisposable();

    public void Init(Camera camera)
    {
        Cam = camera;
        cameraNextPosition = Cam.transform.position;

        //Subscribe to input
        InputController.Instance.MouseMiddleDown.Subscribe(b => HandleMouseMiddleDown(b)).AddTo(disposables);        
        InputController.Instance.MouseScroll.Subscribe(f => Zoom(f)).AddTo(disposables);
    }

    public void HandleMouseMiddleDown(bool b)
    {
        mouseMiddleDrag = b;

        if (b)
        {
            mouseDownPosition = Input.mousePosition;
            mouseDownCameraPosition = Cam.transform.position;
        }
        else
        {
            mouseDownDelta = Vector3.zero;
            cameraNextPosition = Cam.transform.TransformDirection(mouseDownCameraPosition);
        }
    }

    public void RotateAroundWorldPoint(Vector3 worldPoint, float speed)
    {
        Cam.transform.RotateAround(worldPoint, Vector3.up, speed * Time.deltaTime);
    }

    private void Zoom(float _zoomDelta)
    {
        if (_zoomDelta == 0)
            return;

        cameraNextPosition = Cam.transform.position + Cam.transform.forward * _zoomDelta * ZoomSpeed;
        if (cameraNextPosition.y > ZoomMinY && cameraNextPosition.y < ZoomMaxY)
            Cam.transform.position = Vector3.Lerp(Cam.transform.position, cameraNextPosition, Time.deltaTime * ZoomInterpolationSpeed);        
    }
    private void Pan(Vector3 _mouseDelta)
    {
        cameraNextPosition = mouseDownCameraPosition + Cam.transform.TransformDirection(new Vector3(_mouseDelta.x, 0, _mouseDelta.y)) * PanDragMultiplier;
        cameraNextPosition.y = Cam.transform.position.y;
        Cam.transform.position = Vector3.Lerp(Cam.transform.position, cameraNextPosition, Time.deltaTime * PanInterpolationSpeed);
    }

    private void Update()
    {
        if (mouseMiddleDrag)
        {
            mouseDownDelta = Input.mousePosition - mouseDownPosition;
            Pan(-mouseDownDelta);
        }
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
