using UnityEngine;
using UniRx;

public class InputController : SingletonMono<InputController>
{
    public ReactiveProperty<bool> MouseLeftDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> MouseRightDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> MouseMiddleDown = new ReactiveProperty<bool>();
    public ReactiveProperty<float> MouseScroll = new ReactiveProperty<float>();

    public ReactiveProperty<bool> ShiftDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> CtrlDown = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> AltDown = new ReactiveProperty<bool>();

    void Update()
    {
        MouseLeftDown.Value = Input.GetMouseButton(0);
        MouseRightDown.Value = Input.GetMouseButton(1);
        MouseMiddleDown.Value = Input.GetMouseButton(2);
        MouseScroll.Value = Input.GetAxis("Mouse ScrollWheel");

        ShiftDown.Value = Input.GetKey(KeyCode.LeftShift);
        CtrlDown.Value = Input.GetKey(KeyCode.LeftControl);
        AltDown.Value = Input.GetKey(KeyCode.LeftAlt);
    }
}
