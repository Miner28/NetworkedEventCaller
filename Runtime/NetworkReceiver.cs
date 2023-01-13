
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class NetworkReceiver : UdonSharpBehaviour
{
    [NonSerialized] public object[] parameters;
    [HideInInspector] public NetworkManager networkManager;

    /*
     * Main Class for Receiving and handling networked events with parameters.
     * Below is a simple example on how to receive event and read its variables
     *
     * Delete/Comment out the example below but keep the two variables above
     * If you wish to send response to a method you receive you can use reference to NetworkManager to do so.
     *
     * This Behaviour has a SyncMode set to None so you do not need to worry about your Methods needing and underscore.
     * All methods NEED to be made public with return type void
     */

    
    /*
     * To call this method from your script you need to have reference to NetworkManager
     * And you could do:
     * networkManager._SendMethod(SyncTarget.All, "TeleportPlayer", new object[]{
     * Networking.LocalPlayer, Networking.LocalPlayer, new Vector3(10f, 15f, 14f), new Quaternion(0.5f, 0.5f, 0.5f, 0.5f)
     * });
     */
    #region Methods
    
    public void TeleportPlayer()
    {
        VRCPlayerApi playerToTeleport = (VRCPlayerApi) parameters[0];
        VRCPlayerApi playerRequestingTeleport = (VRCPlayerApi) parameters[1];
        Vector3 location = (Vector3) parameters[2];
        Quaternion rotation = (Quaternion) parameters[3];
        
        if (playerRequestingTeleport.GetPlayerTag("rank") == "admin") // Just example on validating that player who sent event has some custom playerTag
        {
            if (playerToTeleport.isLocal)
            {
                playerToTeleport.TeleportTo(location, rotation);
            }
        }
        else
        {
            Debug.Log("Error no permissions");
        }
    }
    
    #endregion

}
