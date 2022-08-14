# NetworkedEventCaller
### Extends VRChat's SendCustomNetworkEvent and allows to send data together with the event.
### Also improves VRChat's implementation of NetworkTarget

# Requires [UdonSharp 1.0!](https://discord.gg/bNNTbrDvUN)
This prefab is meant only for slightly more ADVANCED users! This still requires coding for it to be useful.


## Setup
#### 1. Create Empty GameObject anywhere in scene. Recommended in the top of hiearchy.
#### 2. Attach NetworkManager (/NetworkedEventCaller/Scripts/NetworkManager.cs) to it.
#### 3. Select number of callers. Should be equal to Player Limit * 2 + 2 (Maximum possible amount in instance, *2 because you can always overfill instance by double with friends + 1, Instance creator has reserved spot, + 1 World creator has reserved spot and VRChat Admins can go over this limit to theoretical infinite)
#### 4. Use NetworkReceiver (/NetworkedEventCaller/Scripts/NetworkReceiver.cs) to write your receiver end methods. All methods sent over network will be forwarded there. <br>  For getting variables use object[] variable "parameters" and cast your required variables out of it by indexing the correct place in the array.<br>You ALWAYS have to cast out the exact parameter you sent. If its byte, you need to cast byte, not Int<br>Example:
    public void ExampleMethod()
    {
        var intArray = (int[]) parameters[0];
        // .... 
    }
#### NOTE: VRCPlayerApi objects should be checked using Utilities.IsValid() before working with them!!
#### 5. To send methods you need to have reference to NetworkManager you created in 2nd step. If you attempt to send a event and the user is not yet fully joined. The event will be discarded and you should account for that!<br>Example:
    {
        networkManagerReference._SendMethod(SyncTarget.All, "ExampleMethod", new object[] {new int[] {10, 20, 30, 40}});
    }
#### Supported types:
    Regular + Array: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, string, bool, VRCPlayerApi
    Regular: Color, Color32, Vector3, Quaternion
##### If you are missing a type that is not supported, request support via Issues.

# Special thanks
### Merlin - UdonSharp 1.0
### Nestorboy - Idea for a lot more optimized type sending
