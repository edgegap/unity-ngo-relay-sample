using Edgegap;
using kcp2k;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        [SerializeField] public string token = "RELAY_PROFILE_TOKEN";
        private readonly RelayScript _relay = new();
        private bool waitingForResponse;
        private string sessionId;
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            GUILayout.Label("Session ID: ");
            sessionId = GUILayout.TextField(sessionId);

            if (GUILayout.Button("Client")) {
                ButtonClient();
            }

            if (GUILayout.Button("Host")) {
                ButtonHost();   
            }
            // if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }

        private async void ButtonHost() {
            if (!waitingForResponse)
            {
                waitingForResponse = true;
                await _relay.SendRequest(token, 2);
                waitingForResponse = false;
            }
        }

        private async void ButtonClient() {
            await _relay.JoinRoom(sessionId, token);
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                {
                    foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                        NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
                }
                else
                {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
        }
    }
}