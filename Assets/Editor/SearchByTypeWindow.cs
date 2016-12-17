using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SearchByTypeWindow : EditorWindow
{
	string filterString;
	bool searched;

    List<string> lastSearch;

    Vector2 searchResultsScrollPosition;


    string selected;
	
	
	[MenuItem("Window/Search By Type &g")]
	static void CreateWindow()
	{
		SearchByTypeWindow window = (SearchByTypeWindow)EditorWindow.GetWindow(typeof(SearchByTypeWindow));
	}


	void OnGUI()
	{
		GUILayout.Label("Base Settings", EditorStyles.boldLabel);
		filterString = EditorGUILayout.TextField("Search", filterString);

        if(GUI.changed)
        {
            Search(filterString);
        }

        searchResultsScrollPosition = GUILayout.BeginScrollView(searchResultsScrollPosition);
		foreach (string guid in lastSearch) 
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            //MenuItemStyle = new GUIStyle(GUI.skin.button);
            GUIStyle style = new GUIStyle(GUI.skin.button);
            //style.normal.background = EditorGUIUtility.whiteTexture;
            //GUI.backgroundColor = Color.white;
            //
            //

            if(guid == selected)
            {
                GUI.backgroundColor = Color.blue;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            var rect = EditorGUILayout.BeginHorizontal(style);

            EditorGUILayout.LabelField(path);


            if(Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
            {
                Debug.Log (path);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                selected = guid;
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
		}
        GUILayout.EndScrollView();
	}

	private void Search(string typeName)
	{

        // Taken from http://stackoverflow.com/questions/11107536/convert-string-to-type-in-c-sharp
        // Searches all loaded assemblies for the type specified
        var type = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(x => x.Name == typeName);

        if(type == null)
        {
            lastSearch.Clear();
            return;
        }
        
        // Only search for prefabs, since they are the only assets that can contain components
        // We'll eventually want to filter for scriptable objects as well
		lastSearch = AssetDatabase.FindAssets ("t:Prefab")
			.Where(x => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(x)).GetComponent(type) != null)
            .ToList();
	}

}
