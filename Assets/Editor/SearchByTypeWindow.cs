using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SearchByTypeWindow : EditorWindow
{
	string filterString;
	bool searched;

    List<string> lastSearch = new List<string>();

    Vector2 searchResultsScrollPosition;


    string selected;

    private GUISkin skin;
	
	
	[MenuItem("Window/Search By Type &g")]
	static void CreateWindow()
	{
		SearchByTypeWindow window = (SearchByTypeWindow)EditorWindow.GetWindow(typeof(SearchByTypeWindow));
	}

    void Awake()
    {
        Debug.Log("Hello");
        skin = EditorGUIUtility.Load("Skin.guiskin") as GUISkin;
        Debug.Log(skin);
    }


	void OnGUI()
	{
        //GUI.skin = skin;

        var originalColor = GUI.backgroundColor;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		filterString = EditorGUILayout.TextField("", filterString);
        EditorGUILayout.EndHorizontal();

        if(GUI.changed)
        {
            Search(filterString);
        }

        GUIStyle style = GUI.skin.label;
        style.normal.background = EditorGUIUtility.whiteTexture;
        style.margin.left = 0;
        style.margin.right = 0;
        style.margin.top = 0;
        style.margin.bottom = 0;
        style.padding.left = 6;

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
                GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
                style.normal.textColor = Color.black;
            }

            var rect = EditorGUILayout.BeginHorizontal(style);

            EditorGUILayout.LabelField(path, style);

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

        GUI.backgroundColor = originalColor;
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
