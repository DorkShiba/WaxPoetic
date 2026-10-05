using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>You must approach through `GoogleSheetManager.SO<GoogleSheetSO>()`</summary>
public class GoogleSheetSO : ScriptableObject
{
	public List<PlayerEXP> PlayerEXPList;
	public List<TestSheet> TestSheetList;
}

[Serializable]
public class PlayerEXP
{
	public int level;
	public int required;
	public int accumulated;
}

[Serializable]
public class TestSheet
{
	public string name;
	public int hp;
	public bool playable;
}

