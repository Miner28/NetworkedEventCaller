
using System;
using Miner28.UdonUtils;
using TMPro;
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
    
    public TextMeshProUGUI logText;
    public TextMeshProUGUI byteText;
    private string[] logs = new string[0];
    private bool emergency;
    private bool localEmergency;

    public int byteCount = 0;
    public int byteCountTotal = 0;
    private void Log(string message)
    {
        if (emergency) return;
        if (localEmergency) return;
        logs = logs.Add($"{DateTime.Now.Minute}/{DateTime.Now.Second} - {message}\n");
        if (logs.Length > 25)
        {
            logs = logs.Remove(0);
        }

        logText.text = string.Concat(logs);
    }

    private void Start()
    {
        UpdatePlayerTexts();
        SendCustomEventDelayedSeconds(nameof(UpdateByteDisplay), 1f);

    }

    public void UpdateByteDisplay()
    {
        byteCountTotal += byteCount;
        byteText.text = $"Received bytes in last second: {byteCount} Total: {byteCountTotal}";
        
        byteCount = 0;
        SendCustomEventDelayedSeconds(nameof(UpdateByteDisplay), 1f);
    }

    public TextMeshProUGUI inputShowcaseText;
        
    private VRCPlayerApi[] players = new VRCPlayerApi[0];
    private string[] playerTexts = new string[0];
    private string[] inputTexts = new string[0];
    private int[] playerPing = new int[0];
    private int[] playerFps = new int[0];
    
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        players = players.Add(player);
        playerTexts = playerTexts.Add("");
        inputTexts = inputTexts.Add($"{player.displayName}: Hey! I just joined and haven't sent any funny data yet!");
        playerPing = playerPing.Add(0);
        playerFps = playerFps.Add(0);
        UpdatePlayer(inputTexts.Length - 1);
        Log($"{player.displayName} joined");
    }
    
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        int index = Array.IndexOf(players, player);
        players = players.Remove(index);
        playerTexts = playerTexts.Remove(index);
        inputTexts = inputTexts.Remove(index);
        playerPing = playerPing.Remove(index);
        Log($"{player.displayName} left");
    }

    public void UpdatePlayerTexts()
    {
        var text = string.Concat(playerTexts);

        inputShowcaseText.text = text;

        SendCustomEventDelayedSeconds(nameof(UpdatePlayerTexts), 0.5f);
    }

    public void ReceiveInitialPingRequest()
    {
        var sender = (VRCPlayerApi) parameters[0];
        if (!Utilities.IsValid(sender)) return;
        DateTime time = (DateTime) parameters[1];
        networkManager._SendMethod(SyncTarget.Others, nameof(PingResponse), new object[] {sender, Networking.LocalPlayer, time});
    }

    public void PingResponse()
    {
        var sender = (VRCPlayerApi) parameters[0];
        var receiver = (VRCPlayerApi) parameters[1];
        if (!Utilities.IsValid(sender) || !Utilities.IsValid(receiver)) return;
        if (sender.isLocal)
        {
            DateTime time = (DateTime) parameters[2];
            int index = Array.IndexOf(players, receiver);
            playerPing[index] = (int) (DateTime.Now - time).TotalMilliseconds;
            UpdatePlayer(index);
        }
        Log($"Received ping response from {sender.displayName} to {receiver.displayName}");
    }
    
    
    public void ReceiveFps()
    {
        var player = (VRCPlayerApi)parameters[0];
        if (!Utilities.IsValid(player)) return;
        int index = Array.IndexOf(players, player);
        playerFps[index] = (int)parameters[1];

        UpdatePlayer(index);
        
        Log($"Received FPS from {player.displayName}");
    }

    public void InputTest()
    {
        var player = (VRCPlayerApi)parameters[0];
        if (!Utilities.IsValid(player)) return;
        var input = (string)parameters[1];
        var index = Array.IndexOf(players, player);
        inputTexts[index] = input;
        UpdatePlayer(index);
        
        Log($"Received input from {player.displayName}");
    }

    private void UpdatePlayer(int index)
    {
        playerTexts[index] = $"{players[index].displayName} FPS: {playerFps[index]} RoundTrip: {playerPing[index]}ms CustomText: {inputTexts[index]}\n";
    }
    
    public void ReceiveEmergency()
    {
        var player = (VRCPlayerApi)parameters[0];
        if (!Utilities.IsValid(player)) return;
        if (new string[] {"miner28_3", "CyanLaser", "PhaxeNor", "Faxmashine", "Squid"}.Contains(player.displayName))
        {
            if (emergency)
            {
                emergency = false;
                Log($"{player.displayName} disabled emergency mode");
            }
            else
            {
                Log($"{player.displayName} enabled emergency mode");
                emergency = true;

            }
        }
    }

    public void EmergencyLocal()
    {
        if (localEmergency)
        {
            localEmergency = false;
            Log("Enabled logs locally");
        }
        else
        {
            Log("Disabled Logs Locally");
            localEmergency = true;
        }
    }
}



