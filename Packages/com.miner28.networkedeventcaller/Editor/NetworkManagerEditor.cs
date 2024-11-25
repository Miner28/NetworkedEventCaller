using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Components;

namespace Miner28.UdonUtils.Network
{
    [CustomEditor(typeof(NetworkManager))]
    public class NetworkManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            NetworkManager networkManager = (NetworkManager)target;

            networkManager.debug = EditorGUILayout.Toggle("Debug mode", networkManager.debug);

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup NetworkManager"))
            {
                SetupNetworkManager();
            }

            if (GUILayout.Button("Setup NetworkInterface IDs"))
            {
                HandleNetworkSetup();
            }
        }
        
        public static void SetupNetworkManager()
        {
            NetworkManager networkManager = GetAllObjectsInScene().Find(x => x.GetComponent<NetworkManager>() != null)
                ?.GetComponent<NetworkManager>();
            
            if (networkManager == null)
            {
                networkManager = new GameObject().AddComponent<NetworkManager>();
            }

            GameObject obj = networkManager.gameObject;
            obj.name = "NetworkManager";

            var children = obj.transform.childCount;
            for (int i = 0; i < children; i++)
            {
                var child = obj.transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
            
            var newChild = new GameObject("NetworkedEventCaller");
            newChild.transform.parent = obj.transform;
            newChild.AddComponent<NetworkedEventCaller>();
            newChild.GetComponent<NetworkedEventCaller>().networkManager = networkManager;
            newChild.AddComponent<VRCPlayerObject>();
        }

    public static void HandleNetworkSetup()
        {
            var objectsInScene = GetAllObjectsInScene();
            var networkManager = objectsInScene.Find(x => x.GetComponent<NetworkManager>() != null)
                ?.GetComponent<NetworkManager>();

            //List of Objects implementing NetworkInterface
            var interfacesGameObjects = objectsInScene.FindAll(x => x.GetComponent<NetworkInterface>() != null);
            var interfaces = interfacesGameObjects.ConvertAll(x => x.GetComponent<NetworkInterface>());

            //Ensure all NetworkInterfaces have unique IDs and reassign them if not, ids are incremental
            Undo.RecordObjects(interfaces.ToArray(), "NetworkInterface");
            foreach (var interfaceObj in interfaces)
            {
                var obj = interfaceObj;
                while (interfaces.FindAll(x => x.networkID == obj.networkID).Count > 1)
                {
                    interfaceObj.networkID++;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(interfaceObj);
                }
            }

            var networkIds = interfaces.ConvertAll(x => x.networkID);
            networkManager.sceneInterfaces = interfaces.ToArray();
            networkManager.sceneInterfacesIds = networkIds.ToArray();

            Undo.RecordObject(networkManager, "NetworkedEventCaller");
            PrefabUtility.RecordPrefabInstancePropertyModifications(networkManager);


            //List of Objects implementing NetworkedEventCaller
            var callersGameObjects = objectsInScene.FindAll(x => x.GetComponent<NetworkedEventCaller>() != null);
            var callers = callersGameObjects.ConvertAll(x => x.GetComponent<NetworkedEventCaller>());

            Undo.RecordObjects(callers.ToArray(), "NetworkedEventCaller");

            foreach (var caller in callers)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(caller);

                caller.sceneInterfaces = networkManager.sceneInterfaces;
                caller.sceneInterfacesIds = networkManager.sceneInterfacesIds;
                caller.networkManager = networkManager;
            }
        }

        public static List<GameObject> GetAllObjectsInScene()
        {
            List<GameObject> objectsInScene = new List<GameObject>();
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var gameObject in go.GetComponentsInChildren<Transform>(true))
                {
                    objectsInScene.Add(gameObject.gameObject);
                }
            }

            return objectsInScene;
        }
    }
    
    [InitializeOnLoad]
    public static class NetworkManagerUpdater
    {
        static NetworkManagerUpdater()
        {
            var objectsInScene = NetworkManagerEditor.GetAllObjectsInScene();
            var networkManager = objectsInScene.Find(x => x.GetComponent<NetworkManager>() != null)
                ?.GetComponent<NetworkManager>();
            
            if (networkManager == null) return;
            
            NetworkManagerEditor.SetupNetworkManager();
        }

    }
}
