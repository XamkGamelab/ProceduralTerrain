using UnityEngine;
using UnityEngine.EventSystems;

public class MouseLook : MonoBehaviour
{
    //This is your Camera's transform
    public Transform CameraTransform;

    //This is your Character's transform
    public Transform CharacterTransform;

    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool ClampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;    
    public float SmoothTime = 5f;
    public bool LockCursor = true;
    public bool PhysicsRotation = false;
    
    private Quaternion initCharacterTargetRot;
    private Quaternion initCameraTargetRot;
    private bool m_cursorIsLocked = true;

    private Rigidbody characterRigidBody;

    private void Start() {
        //Either set this component's Character and Camera in editor or...
        Init(CharacterTransform, CameraTransform);
    }

    //Call Init to initialize mouse look for certain character Transform and camera Transform
    public void Init(Transform _character, Transform _camera) {
        CharacterTransform = _character;
        CameraTransform = _camera;

        if (PhysicsRotation)
            characterRigidBody = CharacterTransform.GetComponent<Rigidbody>();

        //Set init rotations for camera and character
        if (PhysicsRotation)
            initCharacterTargetRot = characterRigidBody.rotation;
        else 
            initCharacterTargetRot = _character.localRotation;

        initCameraTargetRot = _camera.localRotation;


    }
    
    public void UpdateLook(float xRot, float yRot) {
        initCharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
        initCameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

        if(ClampVerticalRotation)
            initCameraTargetRot = MathExtensions.ClampRotationAroundXAxis (initCameraTargetRot, MinimumX, MaximumX);
        
        if (PhysicsRotation)
            characterRigidBody.rotation = Quaternion.Slerp(characterRigidBody.rotation, initCharacterTargetRot, SmoothTime * Time.fixedDeltaTime);
        else
            CharacterTransform.localRotation = Quaternion.Slerp (CharacterTransform.localRotation, initCharacterTargetRot, SmoothTime * Time.deltaTime);

        CameraTransform.localRotation = Quaternion.Slerp (CameraTransform.localRotation, initCameraTargetRot, SmoothTime * Time.deltaTime);
        
        UpdateCursorLock();
    }

    public Quaternion GetCameraLocalRotation()
    {
        return CameraTransform.localRotation;
    }

    public void SetCursorLock(bool value) {
        LockCursor = value;

        //we force unlock the cursor if the user disable the cursor locking helper
        if (!LockCursor)  {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock() {
        //if the user set "lockCursor" we check & properly lock the cursos
        if (LockCursor)
            InternalLockUpdate();
    }

    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            m_cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clicked on the UI, don't lock mouse");
                return;
            }
            else
                m_cursorIsLocked = true;
        }

        if (m_cursorIsLocked) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

