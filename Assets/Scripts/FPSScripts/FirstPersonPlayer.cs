using UnityEngine;
using UniRx;

[RequireComponent(typeof (CharacterController))]
public class FirstPersonPlayer : MonoBehaviour
{
    [Header("Basic Attributes")]
    public string CharacterName;
    public int HitPoints;
    [Header("Basic Movement")]
    public float WalkSpeed = 2.5f;
    public float RotationSpeed = 180f;
    public float RunSpeed = 6f;

    public FirstAndThirdPersonCamera FirstPersonCamera;

    [HideInInspector]
    public bool IsAlive = true;
    [HideInInspector]
    public virtual bool Running
    {
        get { return _running; }
        set { _running = value; }
    }

    protected bool _running;

    public bool CanJump = true;    
    public float JumpSpeed = 8;
    public float StickToGroundForce = 8;
    public float GravityMultiplier = 2;
    
    public MouseLook MouseLook;
    public MouseMovement MouseMove { get; private set; }

    protected CharacterController UnityCharacterController => GetComponent<CharacterController>();
    protected Vector2 AxisInput;

    private bool Jump;
    private bool previouslyGrounded;
    private bool jumping;
    
    private Vector2 MoveInputVector;
    private Vector3 MoveDir = Vector3.zero;    
    private CollisionFlags CollFlags;

    private InputController inputController;
    private CompositeDisposable disposables = new CompositeDisposable();

    #region Public
    public void LockCursor(bool locked)
    {
        MouseLook.SetCursorLock(locked);
    }
    #endregion

    #region Private
    private void HandleJumpInput(bool isDown) {

        if (isDown && CanJump) 
        {
            if (!Jump)
                Jump = true;
        }
    }

    private void GetInput(out float speed)
    {
        // set the desired speed to be walking or running
        speed = Running ? RunSpeed : WalkSpeed;
        MoveInputVector = new Vector2(AxisInput.x, AxisInput.y);

        // normalize input if it exceeds 1 in combined length:
        if (MoveInputVector.sqrMagnitude > 1)
            MoveInputVector.Normalize();
    }
    #endregion
    #region Unity
    private void Start() 
    {
        jumping = false;

        //Get input from input controller
        inputController = InputController.Instance;
        inputController.Horizontal.Subscribe(horizontal => AxisInput.x = horizontal).AddTo(disposables);
        inputController.Vertical.Subscribe(vertical => AxisInput.y = vertical).AddTo(disposables);
        inputController.MouseMove.Subscribe(movement => MouseMove = movement).AddTo(disposables);
        inputController.Run.Subscribe(run => Running = run).AddTo(disposables);        
        inputController.Jump.Subscribe(jump => HandleJumpInput(jump)).AddTo(disposables);
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    private void Update() 
    {
        if (MouseLook != null && MouseMove != null)
            MouseLook.UpdateLook(MouseMove.Delta.y, MouseMove.Delta.x);
        
        if (!previouslyGrounded && UnityCharacterController.isGrounded) {                              
            MoveDir.y = 0f;
            jumping = false;
        }

        if (!UnityCharacterController.isGrounded && !jumping && previouslyGrounded)            
            MoveDir.y = 0f;            

        previouslyGrounded = UnityCharacterController.isGrounded;
    }

    private void FixedUpdate()
    {
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward*MoveInputVector.y + transform.right*MoveInputVector.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, UnityCharacterController.radius, Vector3.down, out hitInfo,
                            UnityCharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        MoveDir.x = desiredMove.x*speed;
        MoveDir.z = desiredMove.z*speed;
        
        if (UnityCharacterController.isGrounded) {
            MoveDir.y = -StickToGroundForce;

            if (Jump) {
                MoveDir.y = JumpSpeed;            
                Jump = false;
                jumping = true;
            }
        }
        else
            MoveDir += Physics.gravity*GravityMultiplier*Time.fixedDeltaTime;

        CollFlags = UnityCharacterController.Move(MoveDir*Time.fixedDeltaTime);
        
        MouseLook.UpdateCursorLock();
    }


      
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        //dont move the rigidbody if the character is on top of it
        if (CollFlags == CollisionFlags.Below) return;
        
        if (body == null || body.isKinematic)  return;
        
        body.AddForceAtPosition(UnityCharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    }
    #endregion
}

