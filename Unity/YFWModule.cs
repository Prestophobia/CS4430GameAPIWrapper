using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;

public class YFWModule : MonoBehaviour {
    public static YFWModule YFWMod;
    private float _DeltaTime;
    public float DeltaTime
    {
        get
        {
            return Replaying ? _DeltaTime : Time.smoothDeltaTime;
        }
        set
        {
            _DeltaTime = value;
        }
    }
    public bool Replaying;
    public bool Recording;
    private int _FrameNumber;
    private long _SessionID;
    public List<string> AxesToListenTo;
    public List<KeyCode> KeysToListenTo;
    public List<string> ButtonsToListenTo;
    private List<string> _HeldButtons;
    private LibcheckersFrameState _CurrentFrame;
    public InputField inputField;
    public Image RecordingIcon;
    public Image PlayingIcon;
    public Image StopIcon;
    private string _DBFilename;
    public string DBFileName
    {
        get
        {
            if (_DBFilename == null || _DBFilename.Length < 1 || _DBFilename.Equals("default.db"))
            {
                return "temp.db";
            }
            return _DBFilename;
        }
        set
        {
            if (value != null && value.Length > 0 && !value.Equals("default.db"))
                _DBFilename = value;
        }
    }

    public LibcheckersFrameState CurrentFrame
    {
        get
        {
            return _CurrentFrame;
        }
    }


    void Awake()
    {
        if (YFWMod == null)
        {
            DontDestroyOnLoad(gameObject);
            YFWMod = this;
        }
        else if (YFWMod != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
        Libcheckers.LoadGame("default.db");
        Recording = false;
        Replaying = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (Recording)
        {
            _FrameNumber++;
            Debug.Log("Recording Frame " + _FrameNumber);
            LibcheckersFrameState frame = new LibcheckersFrameState(_FrameNumber, new List<LibcheckersInput>(), new List<LibcheckersState>());
            frame.InsertStateVariable(new LibcheckersState("deltaTime", ("" + Time.deltaTime)));
            if (KeysToListenTo != null)
            {
                foreach (KeyCode key in KeysToListenTo)
                {
                    string name = System.Enum.GetName(typeof(KeyCode), key);
                    LibcheckersInput input = null;
                    if (Input.GetKeyDown(key)) input = new LibcheckersInput(name, "d");
                    else if (Input.GetKeyUp(key)) input = new LibcheckersInput(name, "u");
                    else if (Input.GetKey(key)) input = new LibcheckersInput(name, "h"); 
                    if(input != null) frame.InsertInput(input); 
                }
            }
            if (ButtonsToListenTo != null)
            {
                foreach (string button in ButtonsToListenTo)
                {
                    LibcheckersInput input = null;
                    if (Input.GetButtonDown(button)) input = new LibcheckersInput(button, "d");                    
                    if (Input.GetButtonUp(button)) input = new LibcheckersInput(button, "u");                    
                    else if (Input.GetButton(button)) input = new LibcheckersInput(button, "h");                   
                    if (input != null) frame.InsertInput(input);                    
                }
            }
            if (AxesToListenTo != null)
            {
                foreach (string axis in AxesToListenTo)
                {
                    float AxisValue = Input.GetAxisRaw(axis);
                    if (AxisValue != 0)
                        frame.InsertInput(new LibcheckersInput(axis,""+AxisValue));
                }
            }
            Libcheckers.InsertFrameState(frame);
            Debug.Log(frame.ToString());
        }
        else if (Replaying)
        {
            _FrameNumber++;
            Debug.Log("Replaying Frame " + _FrameNumber);
            _CurrentFrame = Libcheckers.GetFrameStateObject(_FrameNumber);
            Debug.Log(_CurrentFrame.ToString());
            if (_CurrentFrame.Empty || _CurrentFrame.GetStateVariableValue("endRec").Equals("1")) 
            {
                StopPlaying();
                Debug.Log("REPLAY OVER");
            }
            else
            {
                _DeltaTime = float.Parse(_CurrentFrame.GetStateVariableValue("deltaTime"));
            }
        }

    }

    public void StartRecording()
    {
        StopPlaying();
        inputField.interactable = false;
        Libcheckers.LoadGame("temp.db");
        Recording = true;
        RecordingIcon.enabled = true;
        StopIcon.enabled = false;
        CharacterMovement.Character.CanMove = true;
        _FrameNumber = 1;
        Debug.Log("Recording Frame " + _FrameNumber);
        LibcheckersFrameState frame = new LibcheckersFrameState(_FrameNumber, new List<LibcheckersInput>(), new List<LibcheckersState>());
        //frame.InsertInput(new LibcheckersInput("dummy", "1"));
        frame.InsertStateVariable(new LibcheckersState("deltaTime", ("" + Time.deltaTime)));
        frame.InsertStateVariable(new LibcheckersState("charPosX",""+ CharacterMovement.Character.CharPos.x));
        frame.InsertStateVariable(new LibcheckersState("charPosY", "" + CharacterMovement.Character.CharPos.y));
        frame.InsertStateVariable(new LibcheckersState("charPosZ", "" + CharacterMovement.Character.CharPos.z));
        Libcheckers.InsertFrameState(frame);
        Debug.Log(frame.ToString());
    }

    public void StopRecording()
    {
        /*_FrameNumber++;
        Debug.Log("Recording Frame " + _FrameNumber);
        LibcheckersFrameState frame = new LibcheckersFrameState(_FrameNumber, new List<LibcheckersInput>(), new List<LibcheckersState>());
        frame.InsertStateVariable(new LibcheckersState("endRec", "1"));
        Libcheckers.InsertFrameState(frame);
        Debug.Log(frame.ToString());*/
        //Libcheckers.SaveGame("temp.db");
        Recording = false;
        CharacterMovement.Character.CanMove = false;
        RecordingIcon.enabled = false;
        StopIcon.enabled = true;
        inputField.interactable = true;
        _FrameNumber = 0;
    }

    public void StartPlaying()
    {
        StopRecording();
        Replaying = true;
        PlayingIcon.enabled = true;
        StopIcon.enabled = false;
        inputField.interactable = false;
        CharacterMovement.Character.CanMove = true;
        _FrameNumber = 1;
        Debug.Log("Replaying Frame " + _FrameNumber);
        _CurrentFrame = Libcheckers.GetFrameStateObject(1);
        Vector3 newCharPos = new Vector3(float.Parse(_CurrentFrame.GetStateVariableValue("charPosX")), float.Parse(_CurrentFrame.GetStateVariableValue("charPosY")), float.Parse(_CurrentFrame.GetStateVariableValue("charPosZ")));
        CharacterMovement.Character.SetPosition(newCharPos);
        Debug.Log(_CurrentFrame.ToString());
    }

    public void StopPlaying()
    {
        Replaying = false;
        PlayingIcon.enabled = false;
        StopIcon.enabled = true;
        CharacterMovement.Character.CanMove = false;
        inputField.interactable = true;
        _FrameNumber = 0;
        StopIcon.enabled = true;
    }

    public void Stop()
    {
        if (Replaying)
        {
            StopPlaying();
        }
        else if (Recording)
        {
            StopRecording();
        }
        
    }

    public void SetDatabaseFileName(string fileName)
    {
        DBFileName = fileName;
    }

    public void SaveDB()
    {
        Libcheckers.SaveGame(DBFileName);
        Debug.Log("Saved to " + DBFileName);
    }

    public void LoadDB()
    {
        Libcheckers.LoadGame(DBFileName);
        Debug.Log("Loaded from " + DBFileName);
    }
    
    public float GetAxisRaw(string axisName)
    {
        if (Replaying && _CurrentFrame != null && !_CurrentFrame.Empty)
        {
            return float.Parse(_CurrentFrame.GetInputValue(axisName));
        }
        return Input.GetAxisRaw(axisName);
    }

    public bool GetButtonDown(string buttonName)
    {
        if (Replaying && _CurrentFrame != null && !_CurrentFrame.Empty)
        {
            return _CurrentFrame.GetInputValue(buttonName).Equals("d");
        }
        return Input.GetButtonDown(buttonName);
    }

    public bool GetButton(string buttonName)
    {
        if (Replaying && _CurrentFrame != null)
        {
            return _CurrentFrame.GetInputValue(buttonName).Equals("h");
        }
        return Input.GetButton(buttonName);
    }

    public bool GetButtonUp(string buttonName)
    {
        if (Replaying && _CurrentFrame != null)
        {
            return _CurrentFrame.GetInputValue(buttonName).Equals("u");
        }
        return Input.GetButtonUp(buttonName);
    }

    public bool GetKeyDown(string keyName)
    {
        if (Replaying && _CurrentFrame != null)
        {
            return _CurrentFrame.GetInputValue(keyName).Equals("d");
        }
        return Input.GetKeyDown(keyName);
    }

    public bool GetKey(string keyName)
    {
        if (Replaying && _CurrentFrame != null)
        {
            return _CurrentFrame.GetInputValue(keyName).Equals("h");
        }
        return Input.GetKey(keyName);
    }

    public bool GetKeyUp(string keyName)
    {
        if (Replaying && _CurrentFrame != null)
        {
            return _CurrentFrame.GetInputValue(keyName).Equals("u");
        }
        return Input.GetKeyUp(keyName);
    }
}


