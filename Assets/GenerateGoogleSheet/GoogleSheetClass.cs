using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>You must approach through `GoogleSheetManager.SO<GoogleSheetSO>()`</summary>
public class GoogleSheetSO : ScriptableObject
{
	public List<Class1> Class1List;
	public List<Class2> Class2List;
}

[Serializable]
public class Class1
{
	public int IndexInt;
	public float Var1Float;
	public bool Var2Bool;
}

[Serializable]
public class Class2
{
	public int IndexInt;
	public string Var1String;
	public float Var2Float;
}

