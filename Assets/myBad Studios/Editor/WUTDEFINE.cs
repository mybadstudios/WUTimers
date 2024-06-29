using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[InitializeOnLoad]
public class WUTDEFINE
{
	static WUTDEFINE()
	{
		BuildTargetGroup btg = EditorUserBuildSettings.selectedBuildTargetGroup;
		string defines_field = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
		List<string> defines = new List<string>(defines_field.Split(';'));
		if (!defines.Contains("WUT"))
		{
			defines.Add("WUT");
			PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, string.Join(";", defines.ToArray()));
		}
	}
}


