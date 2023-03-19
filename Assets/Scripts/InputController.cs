using UnityEngine;
using UniRx;

/// <summary>
/// Singleton class that filter Unity input to reactive properties.
/// </summary>
public class InputController : SingletonMono<InputController>
{
    //Terrain drawing
    public ReactiveProperty<bool> MouseLeftDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> MouseRightDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> MouseMiddleDown = new ReactiveProperty<bool>();
    public ReactiveProperty<float> MouseScroll = new ReactiveProperty<float>();
    public ReactiveProperty<bool> ShiftDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> CtrlDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> AltDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> EscDown = new ReactiveProperty<bool>();

    //FPS controls
    public ReactiveProperty<float> Horizontal = new ReactiveProperty<float>();
    public ReactiveProperty<float> Vertical = new ReactiveProperty<float>();
    public ReactiveProperty<MouseMovement> MouseMove = new ReactiveProperty<MouseMovement>();
    public ReactiveProperty<float> MouseHorizontal = new ReactiveProperty<float>();
    public ReactiveProperty<float> MouseVertical = new ReactiveProperty<float>();
    public ReactiveProperty<bool> Fire1 = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> Jump = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> Run = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> Anykey = new ReactiveProperty<bool>();

    #region Unity
    private void Update()
    {
        //Terrain drawing input
        MouseLeftDown.Value = Input.GetMouseButton(0);
        MouseRightDown.Value = Input.GetMouseButton(1);
        MouseMiddleDown.Value = Input.GetMouseButton(2);
        MouseScroll.Value = Input.GetAxis("Mouse ScrollWheel");
        ShiftDown.Value = Input.GetKey(KeyCode.LeftShift);
        CtrlDown.Value = Input.GetKey(KeyCode.LeftControl);
        AltDown.Value = Input.GetKey(KeyCode.LeftAlt);
        EscDown.Value = Input.GetKey(KeyCode.Escape);

        //Fps input
        Horizontal.Value = Input.GetAxis("Horizontal");
        Vertical.Value = Input.GetAxis("Vertical");

        MouseHorizontal.Value = Input.GetAxis("Mouse X");
        MouseVertical.Value = Input.GetAxis("Mouse Y");

        MouseMove.Value = new MouseMovement(Input.mousePosition, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

        //Buttons

        if (Input.GetButtonDown("Fire1"))
            Fire1.Value = true;

        if (Input.GetButtonUp("Fire1"))
            Fire1.Value = false;

        if (Input.GetButtonDown("Jump"))
            Jump.Value = true;

        if (Input.GetButtonUp("Jump"))
            Jump.Value = false;
    }
    #endregion
}

public class MouseMovement
{
    public Vector2 Position;
    public Vector2 Delta;

    public MouseMovement() { }

    public MouseMovement(Vector2 _pos, Vector2 _delta)
    {
        Position = _pos;
        Delta = _delta;
    }
}