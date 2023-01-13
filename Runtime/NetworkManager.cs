using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class NetworkManager : UdonSharpBehaviour
{
    private NetworkedEventCaller myCaller;
    
    #region PoolManager
    
    [HideInInspector]
    public VRCObjectPool pool;
    [HideInInspector]
    public bool debug;
    
    [UdonSynced] private int[] poolOwners = new int[100];
    [UdonSynced] private int[] toClean = new int[0];

    private bool runOnce;
    private void OnEnable()
    {
        if (runOnce) return;

        poolOwners = new int[pool.Pool.Length];
        runOnce = true;
    }


    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer.isMaster)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            var obj = pool.TryToSpawn();
            
            _Cleanup();

            poolOwners[Array.IndexOf(pool.Pool, obj)] = player.playerId;
            
            RequestSerialization();
            
            if (player.isLocal)
            {
                myCaller = obj.GetComponent<NetworkedEventCaller>();
                Networking.SetOwner(Networking.LocalPlayer, myCaller.gameObject);
                Log("I have got myself Caller - Master");
            }
        }
    }
    
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        int userId = player.playerId;

        var index = Array.IndexOf(poolOwners, userId);
        if (index == -1) return;

        toClean = toClean.Add(index);
        
        if (Networking.LocalPlayer.isMaster)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            poolOwners[index] = 0;
            Networking.SetOwner(Networking.LocalPlayer, pool.Pool[index]);

            RequestSerialization();
            
            SendCustomEventDelayedFrames(nameof(_Cleanup), 5);
        }
    }
    
    public override void OnDeserialization()
    {
        if (!Utilities.IsValid(myCaller))
        {
            int myId = Networking.LocalPlayer.playerId;

            for (var i = 0; i < poolOwners.Length; i++)
            {
                if (myId == poolOwners[i])
                {
                    myCaller = pool.Pool[i].GetComponent<NetworkedEventCaller>();
                    Networking.SetOwner(Networking.LocalPlayer, myCaller.gameObject);
                    Log($"I have got myself Caller - NonMaster {myCaller == null}");
                }
            }
        }
    }


    public void _Cleanup()
    {
        if (toClean.Length == 0) return;

        for (var index = 0; index < toClean.Length; index++)
        {
            var i = toClean[index];

            var o = pool.Pool[i];
            var networkedEventCaller = o.GetComponent<NetworkedEventCaller>();
            if (networkedEventCaller.methodName == "")
            {
                Log($"Returning {i}");
                pool.Return(o);
                toClean = toClean.Remove(index);
                index--;
            }
            else
            {
                Log($"Cleaning up {i}");
                networkedEventCaller.methodName = "";
                
                Networking.SetOwner(Networking.LocalPlayer, o);
                networkedEventCaller.RequestSerialization();
            }
        }
        SendCustomEventDelayedFrames(nameof(_Cleanup), 5);
    }

    private static void Log(string log)
    {
        Debug.Log($"<color=#FFFF00>PoolManager</color> {log}");
    }
    
    #endregion

    
    /// <summary>
    /// Sends a method over network with variables
    /// </summary>
    /// <param name="target"></param>
    /// <param name="method"></param>
    /// <param name="paramsObj"></param>
    public void _SendMethod(SyncTarget target, string method, object[] paramsObj) //Middle man method to interact with NetworkEventCaller. Ensures there is no NULL Exception
    {
        if (Utilities.IsValid(myCaller))
        {
            myCaller._SendMethod(target, method, paramsObj);
        }
    }
}
