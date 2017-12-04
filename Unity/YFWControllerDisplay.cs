using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YFWControllerDisplay : MonoBehaviour {
    public Transform Joystick1;
    public Transform Controller;
    public Transform ButtonA;
    public float StickRange;
    public Vector3 JoystickCenter1;
    public Vector3 ButtonCenterA;
    public float ButtonRange;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Joystick1.position = new Vector3(Controller.position.x+JoystickCenter1.x+(YFWModule.YFWMod.GetAxisRaw("Horizontal")*StickRange),Controller.position.y + JoystickCenter1.y + (YFWModule.YFWMod.GetAxisRaw("Vertical")*StickRange), Controller.position.z +JoystickCenter1.z);
        Joystick1.localEulerAngles = new Vector3((YFWModule.YFWMod.GetAxisRaw("Vertical") * StickRange) - 90f , 0, (YFWModule.YFWMod.GetAxisRaw("Horizontal") * StickRange * -1f) );
        ButtonA.localPosition = new Vector3(ButtonCenterA.x,  ButtonCenterA.y ,ButtonCenterA.z + (YFWModule.YFWMod.GetButton("Jump") ? ButtonRange : 0));
	}
}
