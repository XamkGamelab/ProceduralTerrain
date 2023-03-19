using UnityEngine;
using UniRx;

/// <summary>
/// Camera controller for top-down free moving camera with pan, zoom and rotate.
/// </summary>
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

    /// <summary>
    /// Rotate cam transform around a world point
    /// </summary>
    /// <param name="worldPoint">Rotate around this point.</param>
    /// <param name="speed">Rotation speed.</param>
    public void RotateAroundWorldPoint(Vector3 worldPoint, float speed)
    {
        Cam.transform.RotateAround(worldPoint, Vector3.up, speed * Time.deltaTime);
    }

    /// <summary>
    /// Zoom camera.
    /// </summary>
    /// <param name="_zoomDelta">Zoom delta value</param>
    private void Zoom(float _zoomDelta)
    {
        if (_zoomDelta == 0)
            return;

        cameraNextPosition = Cam.transform.position + Cam.transform.forward * _zoomDelta * ZoomSpeed;
        if (cameraNextPosition.y > ZoomMinY && cameraNextPosition.y < ZoomMaxY)
            Cam.transform.position = Vector3.Lerp(Cam.transform.position, cameraNextPosition, Time.deltaTime * ZoomInterpolationSpeed);        
    }

    /// <summary>
    /// Pan camera xz in local space.
    /// </summary>
    /// <param name="moveDelta">Movement delta.</param>
    private void Pan(Vector3 moveDelta)
    {
        cameraNextPosition = mouseDownCameraPosition + Cam.transform.TransformDirection(new Vector3(moveDelta.x, 0, moveDelta.y)) * PanDragMultiplier;
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
