﻿using Naukri.InspectorMaid.Editor.Core;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class MonoBehaviourCustomEditor : InspectorMaidEditor 
{
	
}