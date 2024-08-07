using System.Collections.Generic;
using Editor;
using Gemserk;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

public static class SelectionHistorySearchProvider
{
    private static string _providerId = "SearchInSearchHistoryWindow";

    [MenuItem("Search/GameObjects In Selection History  %#a")]
    public static void ShowWindow()
    {
        SearchService.ShowContextual(_providerId);
    }
    
    [SearchItemProvider]
    public static SearchProvider CreateSearchHistoryProvider()
    {
        //#TODO cache on enable
        //#TODO close properly
        var provider = new SearchProvider(_providerId, "Search In SearchHistoryWindow")
        {
            fetchItems = (context, items, provider) =>
            {
                var searchQuery = context.searchQuery.ToLower();
                var results = new List<SearchItem>();
                var h = SelectionHistoryAsset.instance.selectionHistory;
                foreach (var entry in h.History)
                {
                    if (entry != null && 
                        entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced &&
                        entry.Reference != null && 
                        !entry.isUnloadedHierarchyObject)
                    {
                        // Debug.Log(entry.Reference.name);
                        var entryName = entry.Reference.name.ToLower();
                        if (SearchProviderUtility.FuzzyMatch(entryName, searchQuery, out var score))
                        {
                            var searchItem = new SearchItem(entry.Reference.GetInstanceID().ToString())
                            {
                                label = entryName,
                                description = entry.Reference.GetType().ToString(),
                                provider = SearchService.GetProvider(_providerId),
                                value = entry.Reference,
                                score = score
                            };
                            results.Add(searchItem);
                        }

                    }
                }
                return results;
            },
            fetchLabel = (item, context) => item.label,
            fetchDescription = (item, context) => item.description,
            fetchThumbnail = (item, context) =>
                AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<GameObject>(item.id)),
            trackSelection = PingObject
        };
        provider.actions.Add(new SearchAction(_providerId, "Select", new GUIContent("Select"))
        {
            handler = FocusObject,
            closeWindowAfterExecution = true
        });

        return provider;
    }

    private static void PingObject(SearchItem arg1, SearchContext arg2)
    {
        PingObject(arg1);
    }

    public static bool IsPrefabAsset(UnityEngine.Object o)
    {
        return PrefabUtility.IsPartOfPrefabAsset(o) && o is GameObject;
    }
    private static void FocusObject(SearchItem item)
    {
        if (int.TryParse(item.id, out int instanceId))
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var go = obj as GameObject;
            if (go != null)
            {
                Selection.activeGameObject = go;
                SceneView.lastActiveSceneView.FrameSelected();
            } 
            else if (IsPrefabAsset(obj))
            {
                AssetDatabase.OpenAsset(obj);
            }
            UnityEditor.Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }

    private static void PingObject(SearchItem item)
    {
        if (int.TryParse(item.id, out int instanceId))
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId) ;
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
                if (obj is GameObject go)
                {
                    Selection.activeGameObject = go;
                }
            }
        }
    }


}