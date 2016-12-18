﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SearchByTypeWindow : EditorWindow
{
	string filterString;
    string selected;

    string searchingForType;

    List<string> lastSearch = new List<string>();

    Vector2 searchResultsScrollPosition;
	
	[MenuItem("Window/Search By Type &g")]
	static void CreateWindow()
	{
		SearchByTypeWindow window = (SearchByTypeWindow)EditorWindow.GetWindow(typeof(SearchByTypeWindow));
	}

	void OnGUI()
	{
        //GUI.skin = skin;

        var originalColor = GUI.backgroundColor;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		filterString = EditorGUILayout.TextField("", filterString);
        EditorGUILayout.EndHorizontal();

        if(!string.IsNullOrEmpty(searchingForType))
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Searching for assets with "+searchingForType+" components", GUI.skin.label);
            EditorGUILayout.EndHorizontal();
        }

        if(GUI.changed)
        {
            Search(filterString);
        }

        // TODO: Move this to a config section
        // TODO: Find out how to make this editor / inspector driven without creating a shitty GUISkin
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.background = EditorGUIUtility.whiteTexture;
        style.margin.left = 0;
        style.margin.right = 0;
        style.margin.top = -6;
        style.margin.bottom = -4;
        style.padding.top = 0;
        style.padding.left = 6;
        style.padding.bottom = -4;

        searchResultsScrollPosition = GUILayout.BeginScrollView(searchResultsScrollPosition);
		foreach (string guid in lastSearch)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if(guid == selected)
            {
                GUI.backgroundColor = new Color(0.25f, 0.5f, 0.9f);
                style.normal.textColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = new Color(0.78f, 0.78f, 0.78f);
                style.normal.textColor = Color.black;
            }

            // TODO: Review if a Box would be a better control to use here
            var rect = EditorGUILayout.BeginHorizontal(style);

            var icon = AssetDatabase.GetCachedIcon(path);

            GUILayout.Label(icon, style, GUILayout.Width(24), GUILayout.Height(24));

            var nameToDisplay = path.Substring(path.LastIndexOf("/") + 1);
            if(nameToDisplay.Contains("."))
            {
                nameToDisplay = nameToDisplay.Remove(nameToDisplay.LastIndexOf("."));
            }
            EditorGUILayout.LabelField(nameToDisplay, style);

            if(Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
            {
                Debug.Log (path);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                selected = guid;
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
		}
        GUILayout.EndScrollView();

        GUI.backgroundColor = originalColor;
	}

	private void Search(string searchString)
	{
        // Only check for a component type if t: appears in the search string
        // AND it isn't one of our keywords (other built in asset type searches)
        //
        searchingForType = string.Empty;

        if(searchString.Contains("t:") && !FILTER_KEYWORDS.Any(searchString.Contains))
        {
            var typeName = searchString.Substring(searchString.IndexOf("t:") + 2);
            Debug.Log("Type: "+typeName);
            searchString = searchString.Replace("t:"+typeName, "t:Prefab");
            Debug.Log("Search: "+searchString);

            // Taken from http://stackoverflow.com/questions/11107536/convert-string-to-type-in-c-sharp
            // Searches all loaded assemblies for the type specified
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsSubclassOf(typeof(Component)));
            // Try to find an exact match
            System.Type type = types.FirstOrDefault(x => x.Name == typeName);
            // If that fails, try to find a partial match
            if(type == null)
            {
                type = types.FirstOrDefault(x => x.Name.StartsWith(typeName));
            }

            if(type == null)
            {
                lastSearch.Clear();
                return;
            }
            searchingForType = type.Name;

            Debug.Log(type.Name);

            // Only search for prefabs, since they are the only assets that can contain components
            // We'll eventually want to filter for scriptable objects as well
            lastSearch = AssetDatabase.FindAssets (searchString)
                .Where(x => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(x)).GetComponent(type) != null)
                .ToList();
        }
        else
        {
            lastSearch = AssetDatabase.FindAssets (searchString)
                .ToList();
        }

	}

    readonly string[] FILTER_KEYWORDS = {"t:Animation", "t:AudioClip", "t:Font", "t:GUISkin", "t:Mesh", "t:Model", "t:PhysicMaterial", "t:Script", "t:Shader", "t:Texture", "t:Scene", "t:Material"};

}
