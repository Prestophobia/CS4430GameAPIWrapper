using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//Simple wrapper class for libcheckers for C#
public class Libcheckers{
    [DllImport("libcheckers", CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr database_interop_getFrameState(int frameNumber);

    [DllImport("libcheckers", CallingConvention = CallingConvention.StdCall)]
    private static extern void database_interop_destroyFrameState(string frameState);

    [DllImport("libcheckers", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private static extern void database_interop_insertFrameState(string frameState);

    [DllImport("libcheckers", CallingConvention = CallingConvention.StdCall)]
    private static extern void database_interop_loadGame(string saveFileName);

    [DllImport("libcheckers", CallingConvention = CallingConvention.StdCall)]
    private static extern void database_interop_saveGame(string saveFileName);

    private static string PtrToString(IntPtr p)
    {
        // TODO: deal with character set issues.  Will PtrToStringAnsi always
        // "Do The Right Thing"?
        if (p == IntPtr.Zero)
            return null;
        return Marshal.PtrToStringAnsi(p);
        // Marshal.Release()
    }

    public static void InsertFrameState(string frameState)
    {
        database_interop_insertFrameState(frameState);
    }

    public static void InsertFrameState(LibcheckersFrameState frameState)
    {
        InsertFrameState(frameState.ToString());
    }

    public static string GetFrameState(int frameNumber)
    {
        return PtrToString(database_interop_getFrameState(frameNumber));
    }

    public static LibcheckersFrameState GetFrameStateObject(int frameNumber)
    {
        return new LibcheckersFrameState(GetFrameState(frameNumber));
    }

    public static void LoadGame(string saveFileName)
    {
        database_interop_loadGame(saveFileName);
    }

    public static void SaveGame(string saveFileName)
    {
        database_interop_saveGame(saveFileName);
    }

    public static void DestroyFrameState(string frameState)
    {
        database_interop_destroyFrameState(frameState);
    }
}

public class LibcheckersFrameState
{
    private int _FrameNumber;
    private List<LibcheckersInput> _Inputs;
    private List<LibcheckersState> _States;

    public int FrameNumber
    {
        get
        {
            return _FrameNumber;
        }
        set
        {
            _FrameNumber = value;
        }
    }

    public bool Empty;

    public List<LibcheckersInput> Inputs
    {
        get
        {
            return _Inputs;
        }
        set
        {
            _Inputs = value;
        }
    }

    public List<LibcheckersState> States
    {
        get
        {
            return _States;
        }
        set
        {
            _States = value;
        }
    }

    public LibcheckersFrameState(string frameState)
    {
        string[] sA = frameState.Split('\t');
        if(sA.Length >= 3)
        {
            _FrameNumber = int.Parse(sA[0]);
            _Inputs = LibcheckersInput.ParseInputs(sA[1]);
            _States = LibcheckersState.ParseStates(sA[2]);
            Empty = false;
        }
        else
        {
            _FrameNumber = 0;
            _Inputs = new List<LibcheckersInput>();
            _States = new List<LibcheckersState>();
            Empty = true;
        }
    }

    public LibcheckersFrameState(int frameNumber, List<LibcheckersInput> inputs, List<LibcheckersState> states)
    {
        _FrameNumber = frameNumber;
        _Inputs = (inputs != null) ? inputs : new List<LibcheckersInput>();
        _States = (states != null) ? states : new List<LibcheckersState>();
    }

    public override string ToString()
    {
        return ""+_FrameNumber+"\t"+LibcheckersInput.InputListToString(_Inputs)+"\t"+LibcheckersState.StateListToString(_States)+"\t\0";
    }
    
    public void InsertInput(LibcheckersInput input)
    {
        _Inputs.Add(input);
    }

    public string GetInputValue(string inputName)
    {
        if (inputName == null) return "0";
        foreach (LibcheckersInput existingInput in _Inputs)
        {
            if (existingInput.InputName.Equals(inputName))
            {
                return existingInput.Value;
            }
        }
        return "0";
    }

    public void InsertStateVariable(LibcheckersState state)
    {
        _States.Add(state);
    }

    public string GetStateVariableValue(string stateVarName)
    {
        if (stateVarName == null) return "0";
        foreach (LibcheckersState state in _States)
        {
            if (state.StateVariable.Equals(stateVarName))
            {
                return state.Value;
            }
        }
        return "0";
    }

}

public class LibcheckersInput
{
    string _InputName;
    string _Value;

    public string InputName
    {
        get
        {
            return _InputName;
        }
        set
        {
            _InputName = value;
        }
    }

    public string Value
    {
        get
        {
            return _Value;
        }
        set
        {
            _Value = value;
        }
    }

    public LibcheckersInput(string str)
    {
        string[] sA = str.Split('=');
        if (sA.Length >= 2)
        {
            _InputName = sA[0];
            _Value = sA[1];
        }
    }

    public LibcheckersInput(string name, string val)
    {
        _InputName = name;
        _Value = val;
    }

    public override string ToString()
    {
        return ((_InputName != null && _InputName.Length > 0)?_InputName:"BLANK") + "=" + ((_Value != null && _Value.Length > 0)?_Value:"BLANK");
    }

    public static List<LibcheckersInput> ParseInputs(string inputList)
    {
        List<LibcheckersInput> output = new List<LibcheckersInput>();
        string[] inputs = inputList.Split(',');
        foreach (string input in inputs){
            output.Add(new LibcheckersInput(input));
        }
        return output;
    }

    public static string InputListToString(List<LibcheckersInput> inputs)
    {
        string output = "def=0";
        if(inputs != null)
        {
            if(inputs.Count > 0)
            {
                for(int i = 0; i < inputs.Count; i++)
                {
                    output += ","+inputs[i].ToString();
                }
            }
        }
        return output;
    }
}

public class LibcheckersState
{
    string _StateVariable;
    string _Value;

    public string StateVariable
    {
        get
        {
            return _StateVariable;
        }
        set
        {
            _StateVariable = value;
        }
    }

    public string Value
    {
        get
        {
            return _Value;
        }
        set
        {
            _Value = value;
        }
    }

    public LibcheckersState(string stateVariable, string val)
    {
        _StateVariable = stateVariable;
        _Value = val;
    }

    public LibcheckersState(string str)
    {
        string[] sA = str.Split('=');
        if (sA.Length >= 2)
        {
            _StateVariable = sA[0];
            _Value = sA[1];
        }
    }

    public override string ToString()
    {
        return ((_StateVariable != null && _StateVariable.Length > 0) ? _StateVariable : "BLANK") + "=" + ((_Value != null && _Value.Length > 0) ? _Value : "BLANK");
    }

    public static List<LibcheckersState> ParseStates(string stateList)
    {
        List<LibcheckersState> output = new List<LibcheckersState>();
        string[] states = stateList.Split(',');
        foreach (string state in states)
        {
            output.Add(new LibcheckersState(state));
        }
        return output;
    }

    public static string StateListToString(List<LibcheckersState> states)
    {
        string output = "def=0";
        if (states != null)
        {
            if (states.Count > 0)
            {
                output = states[0].ToString();
            }
            if (states.Count > 1)
            {
                for (int i = 1; i < states.Count; i++)
                {
                    output += "," + states[i].ToString();
                }
            }
        }
        return output;
    }
}
