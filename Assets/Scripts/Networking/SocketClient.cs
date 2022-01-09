using System.Runtime.InteropServices;
using UnityEngine;

namespace LwNetworking
{
    public class SocketClient : Mingleton<SocketClient>
    {
        [DllImport("__Internal")]
        private static extern void Connect(string ip, bool secure, string room, string nickname, string avatarString);
        [DllImport("__Internal")]
        private static extern void Emit(string eventName, string message);
        [DllImport("__Internal")]
        private static extern bool IsConnected();
        [DllImport("__Internal")]
        private static extern void Disconnect();
        
        public bool Connected => IsConnected();
        
        public SappyIdentity Identity { get; private set; } = SappyIdentity.Null;
        
        public NetworkEventListener Listener = new NetworkEventListener();
        
        new void Awake()
        {
            base.Awake();
            gameObject.name = "SocketClient"; // REQUIRED FOR COMMUNICATION DO NOT CHANGE
            
            Listener.AddListener(NetworkEventEnum.AuthSuccess, obj =>
            {
                Identity = JsonUtility.FromJson<SappyIdentity>((string) obj);
            });
        }
        
        public void TryConnect(string address, string room, string nickname, string avatarString)
        {
            Connect(address, address.StartsWith("https"), room, nickname, avatarString);
        }

        public void DisconnectFromServer()
        {
            Identity = SappyIdentity.Null;
            Disconnect();
        }
        
        public void Emit(NetworkEventEnum eventEnum, string value)
        {
            Emit(eventEnum.ToString(), value);
        }

        public bool IsPlayer(int id)
        {
            return Identity.id == id;
        }

        // ReSharper disable once UnusedMember.Global
        public void InvokeNetworkEvent(string eventString)
        {
            var delim = eventString.IndexOf(':');
            var eventName = eventString.Substring(0, delim);
            var eventData = eventString.Substring(delim + 1);
            
            Listener.Invoke(NetworkEventListener.ToEnum(eventName), eventData);
        }

    }
}
