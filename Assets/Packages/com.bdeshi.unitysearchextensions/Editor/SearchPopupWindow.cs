using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Search;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.ShortcutManagement;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class GameObjectSearchProvider
    {
        private static string _providerId = "SearchInScene";
        private static GameObject DontDestroyHelper;
        
        [SearchItemProvider]
        public static SearchProvider CreateProvider()
        {
            //#TODO cache on enable
            //#TODO close properly
            var provider = new SearchProvider(_providerId, "Search In Scene")
            {
                filterId = "sgo:",
                fetchItems = (context, items, provider) =>
                {
                    var searchQuery = context.searchQuery.ToLower();
                    var results = new List<SearchItem>();

                    // Search all active scenes
                    for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                    {
                        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                        if (scene.isLoaded)
                        {
                            foreach (GameObject go in scene.GetRootGameObjects())
                            {
                                SearchInChildren(go, searchQuery, results);
                            }
                        }
                    }

                    if (Application.isPlaying)
                    {
                        var dontDestroyOnLoadObjects = GetDontDestroyOnLoadObjects();
                        foreach (var go in dontDestroyOnLoadObjects)
                        {
                            SearchInChildren(go, searchQuery, results);
                        }
                    }

                    return results;
                },
                fetchLabel = (item, context) => item.label,
                fetchDescription = (item, context) => item.description,
                fetchThumbnail = (item, context) =>
                    AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<GameObject>(item.id)),
                trackSelection = (item, context) => PingGameObject(item),
                
            };
            provider.actions.Add(new SearchAction(_providerId, "Select", new GUIContent("Select"))
            {
                handler = FocusOnGameObject,
                closeWindowAfterExecution = true
            });
            provider.actions.Add(new SearchAction(_providerId, "Ping", new GUIContent("Ping"))
            {
                handler = PingGameObject,
                closeWindowAfterExecution = false
            });
            provider.actions.Add(new SearchAction(_providerId, "Frame", new GUIContent("Frame"))
            {
                handler = PingAndFrameGameObject,
                closeWindowAfterExecution = false
            });
            
            return provider;
        }


        private static List<GameObject> GetDontDestroyOnLoadObjects()
        {
            var dontDestroyOnLoadObjects = new List<GameObject>();
            if (DontDestroyHelper == null)
            {
                DontDestroyHelper = new GameObject("FakeDontDestroyOnLoadParent");
                MonoBehaviour.DontDestroyOnLoad(DontDestroyHelper);
            }
            dontDestroyOnLoadObjects.AddRange(DontDestroyHelper.scene.GetRootGameObjects());
            dontDestroyOnLoadObjects.Remove(DontDestroyHelper);

            return dontDestroyOnLoadObjects;
        }

        private static void SearchInChildren(GameObject parent, string searchQuery, List<SearchItem> results)
        {
            var itemName = parent.name.ToLower();
            if (SearchProviderUtility.FuzzyMatch(itemName, searchQuery, out var score))
            {
                var item = new SearchItem(parent.GetInstanceID().ToString())
                {
                    label = parent.name,
                    description = parent.scene.name,
                    provider = SearchService.GetProvider(_providerId),
                    value = parent,
                    score = score,
                };
                results.Add(item);
            }

            foreach (Transform child in parent.transform)
            {
                SearchInChildren(child.gameObject, searchQuery, results);
            }
        }

        [MenuItem("Window/QuickSearch/GameObjects In Scene")]
        public static void ShowWindow()
        {
            SearchService.ShowContextual(_providerId);
        }

        private static void FocusOnGameObject(SearchItem item)
        {
            if (int.TryParse(item.id, out int instanceId))
            {
                var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                if (go != null)
                {
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                    if (Event.current.shift)
                    {
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
            }
        }
        
        private static void PingGameObject(SearchItem item)
        {
            if (int.TryParse(item.id, out int instanceId))
            {
                var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                if (go != null)
                {
                    EditorGUIUtility.PingObject(go);
                    Selection.activeGameObject = go;
                }
            }
        }
        
        private static void PingAndFrameGameObject(SearchItem item)
        {
            if (int.TryParse(item.id, out int instanceId))
            {
                var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                if (go != null)
                {
                    EditorGUIUtility.PingObject(go);
                    Selection.activeGameObject = go;
                    SceneView.lastActiveSceneView.FrameSelected();
                }
            }
        }
    }
}