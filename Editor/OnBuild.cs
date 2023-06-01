using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Miner28.UdonUtils.Network.Editor
{
    public class OnBuild : IVRCSDKBuildRequestedCallback
    {
        private static void RunChecks()
        {
                        //Ensure all references are valid, Ensure all NetworkInterfaces have unique IDs and cancel build if not
            
            var objectsInScene = GetAllObjectsInScene();
            var networkManager = objectsInScene.Find(x => x.GetComponent<NetworkManager>() != null)?.GetComponent<NetworkManager>();
            
            //List of Objects implementing NetworkInterface
            var interfacesGameObjects = objectsInScene.FindAll(x => x.GetComponent<NetworkInterface>() != null);
            var interfaces = interfacesGameObjects.ConvertAll(x => x.GetComponent<NetworkInterface>());
            
            //Ensure all NetworkInterfaces have unique IDs, cancel build if not
            bool hasDuplicates = false;
            foreach (var obj in interfaces.Where(obj => interfaces.FindAll(x => x.networkID == obj.networkID).Count > 1))
            {
                Debug.LogError(
                    $"NetworkInterface {obj.name} has duplicate networkID {obj.networkID}! Please assign unique networkIDs to all NetworkInterfaces in the scene.");
                hasDuplicates = true;
            }

            if (hasDuplicates)
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
                throw new Exception("Please assign unique networkIDs to all NetworkInterfaces in the scene. For automatic assignment, use the NetworkManager inspector.");
            }
            
            
            var networkIds = interfaces.ConvertAll(x => x.networkID);
            networkManager.sceneInterfaces = interfaces.ToArray();
            networkManager.sceneInterfacesIds = networkIds.ToArray();
            
            //List of Objects implementing NetworkedEventCaller
            var callersGameObjects = objectsInScene.FindAll(x => x.GetComponent<NetworkedEventCaller>() != null);
            var callers = callersGameObjects.ConvertAll(x => x.GetComponent<NetworkedEventCaller>());
            
            foreach (var caller in callers)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(caller);

                caller.sceneInterfaces = networkManager.sceneInterfaces;
                caller.sceneInterfacesIds = networkManager.sceneInterfacesIds;
                caller.networkManager = networkManager;
            }
        }


        
        
        [PostProcessScene(-100)]
        public static void OnPostProcessScene()
        {
            if (isBuilding)
            {
                isBuilding = false;
            } 
            else
            {
                RunChecks();
            }
        }


        private static bool isBuilding = false;
        public int callbackOrder { get; }
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            isBuilding = true;
            RunChecks();
            return true;
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