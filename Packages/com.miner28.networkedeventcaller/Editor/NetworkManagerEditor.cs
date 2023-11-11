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
        private int netCallAmount = 0;

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            NetworkManager networkManager = (NetworkManager) target;

            networkManager.debug = EditorGUILayout.Toggle("Debug mode", networkManager.debug);
            
            EditorGUILayout.Space();
            


            GUILayout.Label("Should always be MaxInstanceSize * 2 + 2");
            netCallAmount = EditorGUILayout.IntSlider("NetCallers: ", netCallAmount, 0, 160);

            if (GUILayout.Button("Setup NetworkManager"))
            {
                GameObject obj = networkManager.gameObject;
                obj.name = "NetworkManager";

                var pool = obj.GetComponent<VRCObjectPool>();

                if (pool != null)
                {
                    Debug.LogWarning($"It appears you have upgraded from an older version of NetworkEventCaller, Performing cleanup on {obj.name}");
                    DestroyImmediate(pool);
                }

                
                var callObj = obj.transform.Find("NetCallers")?.gameObject;
                if (callObj)
                {
                    while (callObj.transform.childCount > 0)
                    {
                        DestroyImmediate(callObj.transform.GetChild(0).gameObject);
                    }
                }
                else
                {
                    callObj = new GameObject();
                    callObj.name = "NetCallers";
                    callObj.transform.SetParent(obj.transform);
                }


                GameObject[] poolObjects = new GameObject[netCallAmount];
                for (int i = 0; i < netCallAmount; i++)
                {
                    var newObj = new GameObject($"NetCaller {i + 1}", typeof(NetworkedEventCaller));

                    newObj.transform.SetParent(callObj.transform);
                    poolObjects[i] = newObj;
                }

                networkManager.pool = poolObjects;
            }

            if (GUILayout.Button("Setup NetworkInterface IDs"))
            {
                HandleNetworkSetup();
            }
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

        static List<GameObject> GetAllObjectsInScene()
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
}