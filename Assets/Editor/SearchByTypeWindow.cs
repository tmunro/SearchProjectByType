using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SearchByTypeWindow : EditorWindow
{
	string filterString;
    string selected;
    int selectedIndex = -1;

    string searchingForType;

    List<string> lastSearch = new List<string>();

    Vector2 searchResultsScrollPosition;

    readonly Color HIGHLIGHT_COLOR = new Color(0.25f, 0.5f, 0.9f);
    readonly Color DEFOCUSED_HIGHLIGHT_COLOR = new Color(0.55f, 0.55f, 0.55f);

    readonly string[] FILTER_KEYWORDS = {"t:Animation", "t:AudioClip", "t:Font", "t:GUISkin", "t:Mesh", "t:Model", "t:PhysicMaterial", "t:Script", "t:Shader", "t:Texture", "t:Scene", "t:Material"};
	
	[MenuItem("Window/Search By Type &g")]
	static void CreateWindow()
	{
		SearchByTypeWindow window = (SearchByTypeWindow)EditorWindow.GetWindow(typeof(SearchByTypeWindow));
	}

	void OnGUI()
	{
        //GUI.skin = skin;
        

        var test = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        
        var focused = EditorWindow.focusedWindow == this;

        var originalColor = GUI.backgroundColor;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		filterString = EditorGUILayout.TextField("", filterString, test.FindStyle("ToolbarSeachTextField"));
        if(!string.IsNullOrEmpty(filterString))
        {
            if(GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                // Remove focus if cleared
                filterString = "";
                GUI.FocusControl(null);
            }
        }
        else
        {
            GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"));
        }
        
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

        //style = GUI.skin.FindStyle("PreBackground");

        searchResultsScrollPosition = GUILayout.BeginScrollView(searchResultsScrollPosition);
		for(var i = 0; i < lastSearch.Count; i++)
        {
            var guid = lastSearch[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if(guid == selected || i == selectedIndex) // || Selection.assetGUIDs[0] == guid)
            {
                GUI.backgroundColor = focused ? HIGHLIGHT_COLOR : DEFOCUSED_HIGHLIGHT_COLOR;
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
            //if(GUILayout.Button(new GUIContent(nameToDisplay, icon)))
            {
                Debug.Log (path);
                GUI.FocusControl("");
                SetSelected(i);
            }

            EditorGUILayout.EndHorizontal();
		}

        if(Event.current.type == EventType.KeyDown)
        {
            if(Event.current.keyCode == KeyCode.DownArrow)
            {
                SetSelected(selectedIndex + 1);
            }
            else if(Event.current.keyCode == KeyCode.UpArrow)
            {
                SetSelected(selectedIndex - 1);
            }
        }

        GUILayout.EndScrollView();

        GUI.backgroundColor = originalColor;
	}

    void SetSelected(int index)
    {
                Event.current.Use();
        if(index < 0 || index >= lastSearch.Count)
            return;

                GUI.changed = true;
        selected = lastSearch[index];
        selectedIndex = index;
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(selected));
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


}
