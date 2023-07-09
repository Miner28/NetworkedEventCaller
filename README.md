# Networked Event Caller
## Description
### Allows you to call events/methods over the network with ease and support for sending parameters over the network.
# [Install through VPM](https://miner28.github.io/NetworkedEventCaller/)

# How to use
## Scene Setup
### 1. Add the NetworkManager component to a GameObject. It is recommended that this is put into root of the scene. And must be only one in the scene.
### 2. On NetworkManager set the amount of NetworkedEventCallers you wish to generate and press Setup NetworkManager. This will generate the NetworkedEventCallers and set them up.
## Your code setup
### 1. Inherit from NetworkInterface on the class you wish to call and receive events/methods on.
### 2. Use `SendNetworkedMethod` to send a method/event over the network. The parameters you send much much 1:1 in type and order to the parameters of the method/event you are calling. Otherwise this will crash the UdonBehaviour.

# Example
## Sending a method/event
```csharp
    public override void Interact()
    {
        SendMethodNetworked(nameof(CoolMethod), SyncTarget.All, Time.time, new DataToken(transform.position), new DataToken(transform.rotation), new DataToken(Networking.LocalPlayer));
    }
```
## Sending method to another UdonBehaviour
```csharp
    public override void Interact()
    {
        otherBehaviour.SendMethodNetworked(nameof(OtherBehaviourClass.CoolMethod), SyncTarget.All, Time.time, new DataToken(transform.position), new DataToken(transform.rotation), new DataToken(Networking.LocalPlayer));
    }
```

## To receive we can declare regular C#/U# method and add `[NetworkedMethod]` to mark it as networked.
```csharp
    [NetworkedMethod]
    public void CoolMethod(float time, Vector3 position, Quaternion rotation, VRCPlayerApi player)
    {
        Debug.Log($"{time} - {position} - {rotation} - {player.displayName}");
    }
```


# Special thanks
## Merlin - UdonSharp
## Phasedragon - General help
