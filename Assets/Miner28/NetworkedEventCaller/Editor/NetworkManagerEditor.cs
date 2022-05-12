using System;
using System.IO;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace Miner28.NetworkedEventCaller.Editor
{
    
    [CustomEditor(typeof(NetworkManager))]
    public class NetworkManagerEditor : UnityEditor.Editor
    {
        private int netCallAmount = 0;
        
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            NetworkManager networkManager = (NetworkManager) target;

            bool newDebug = EditorGUILayout.Toggle("Debug mode", networkManager.debug);

            if (newDebug != networkManager.debug)
            {
                networkManager.debug = newDebug;
                var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                Debug.Log($"Android Symbols: {symbols}");
                if (newDebug)
                {
                    var arraySymbols = symbols.Split(';').ToList();
                    if (!arraySymbols.Contains("NETDEBUG"))
                    {
                        arraySymbols.Add("NETDEBUG");
                    }
                    symbols = String.Join(";", arraySymbols);
                }
                else
                {
                    var arraySymbols = symbols.Split(';').ToList();
                    if (arraySymbols.Contains("NETDEBUG"))
                    {
                        arraySymbols.Remove("NETDEBUG");
                    }
                    symbols = String.Join(";", arraySymbols);
                }
                Debug.Log($"New Android Symbols: {symbols}");
                
                
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbols);
                
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                Debug.Log($"PC Symbols: {symbols}");
                if (newDebug)
                {
                    var arraySymbols = symbols.Split(';').ToList();
                    if (!arraySymbols.Contains("NETDEBUG"))
                    {
                        arraySymbols.Add("NETDEBUG");
                    }
                    symbols = String.Join(";", arraySymbols);
                }
                else
                {
                    var arraySymbols = symbols.Split(';').ToList();
                    if (arraySymbols.Contains("NETDEBUG"))
                    {
                        arraySymbols.Remove("NETDEBUG");
                    }
                    symbols = String.Join(";", arraySymbols);
                }
                Debug.Log($"New PC Symbols: {symbols}");
                
                
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);

            }
            
            
            GUILayout.Label("Should always be MaxInstanceSize * 2 + 2");
            netCallAmount = EditorGUILayout.IntSlider("NetCallers: ", netCallAmount, 0, 160);
            
            if (GUILayout.Button("Setup NetworkManager"))
            {
                GameObject obj = networkManager.gameObject;
                obj.name = "NetworkManager";
                
                var pool = obj.GetComponent<VRCObjectPool>();

                if (pool == null)
                {
                    pool = obj.AddComponent<VRCObjectPool>();
                }
                
                var networkReceiver = obj.transform.Find("NetworkReceiver")?.GetComponent<NetworkReceiver>();

                if (networkReceiver == null)
                {
                    var netReceiver = new GameObject();
                    netReceiver.name = "NetworkReceiver";
                    netReceiver.transform.SetParent(obj.transform);
                    networkReceiver = netReceiver.AddComponent<NetworkReceiver>();
                }

                networkManager.pool = pool;

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
                    var newObj = new GameObject();
                    newObj.name = $"NetCaller {i + 1}";
                    
                    newObj.transform.SetParent(callObj.transform);
                    poolObjects[i] = newObj;
                    var newCaller = newObj.AddComponent<global::NetworkedEventCaller>();
                    newCaller.networkReceiver = networkReceiver;
                }

                networkReceiver.networkManager = networkManager;
                networkManager.pool.Pool = poolObjects;
            }
        }
    }
}