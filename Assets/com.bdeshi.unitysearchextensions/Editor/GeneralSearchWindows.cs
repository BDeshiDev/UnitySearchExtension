using Editor;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace Bdeshi.UnitySearchExtensions.Editor
{
    public static class GeneralSearchWindows
    {
        [MenuItem("GameObject/Find Reference(Non Recursive)", false)]
        [MenuItem("Assets/Find Reference(Non Recursive)", false)]
        public static void FindRefNonRecursiveSearch(MenuCommand menuCommand)
        {
            var obj = menuCommand.context;
            if (obj != null)
            {
                SearchObjRefWindow(obj);
            }
        }
        private static void SearchObjRefWindow(Object obj)
        {
            EditorGUIUtility.PingObject(obj);
            var view = SearchService.ShowContextual();
            view.SetSearchText($"ref={obj.GetInstanceID()} ");
            view.Focus();
            view.FocusSearch();
            view.SetSearchViewToDisplayMode(DisplayMode.List);
        }

        //we don't want overlapping keybinds so the version with keybinds is a separate function
        [MenuItem("Search/Find Reference(Non Recursive) %#r", false)]
        public static void FindRefNonRecursiveSearch() {
            if (Selection.activeObject != null)
            {
                SearchObjRefWindow(Selection.activeObject);
            }
        }
        
        [MenuItem("Search/Find Scenes %l", false)]
        public static void FindScene() {
            var view = SearchService.ShowContextual();
            
            view.SetSearchText("t:scene ");
            view.Focus();
            view.FocusSearch();
            view.SetSearchViewToDisplayMode(DisplayMode.List);
        }
        
                
        [MenuItem("Search/Find Prefab %p", false)]
        public static void FindPrefab() {
            var view = SearchService.ShowContextual();

            view.SetSearchText("t:prefab ");
            view.Focus();
            view.FocusSearch();
            view.SetSearchViewToDisplayMode(DisplayMode.List);
        }
        [MenuItem("Search/Find  in project %i", false)]
        public static void FindInProject() {
            var view = SearchService.ShowContextual();

            view.SetSearchText("p:");
            view.Focus();
            view.FocusSearch();
            view.SetSearchViewToDisplayMode(DisplayMode.List);
        }
    }
}