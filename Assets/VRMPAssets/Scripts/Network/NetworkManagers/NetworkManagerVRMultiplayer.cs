using Unity.Netcode;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRMultiplayer
{
    /// <summary>
    /// Manages the network functionality for VR multiplayer.
    /// </summary>
    public class NetworkManagerVRMultiplayer : NetworkManager
    {
        [SerializeField, Tooltip("Set this to control how much logging is generated")]
        LogLevel m_LogLevel;

        [SerializeField, Tooltip("This should almost always be set to true")]
        bool m_RunInBackground = true;

        [SerializeField]
        NetworkConfig m_NetworkConfig;

        // NetworkManager's own Awake is private. Declaring Awake here hides it, so Unity never
        // calls it. Since Netcode for GameObjects 2.13 that base Awake creates the internal logger
        // and initializes the network prefabs; without it, StartClient()/OnDestroy() throw a
        // NullReferenceException in player builds (the Editor masks this via OnValidate).
        // We therefore invoke the base Awake explicitly after applying our settings.
        static readonly System.Reflection.MethodInfo s_BaseAwake = typeof(NetworkManager).GetMethod(
            "Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        ///<inheritdoc/>
        void Awake()
        {
            LogLevel = m_LogLevel;
            RunInBackground = m_RunInBackground;
            NetworkConfig = m_NetworkConfig;
            Utils.s_LogLevel = LogLevel;

            if (s_BaseAwake != null)
                s_BaseAwake.Invoke(this, null);
            else
                Debug.LogError("[NetworkManagerVRMultiplayer] Could not find NetworkManager.Awake - networking will fail in builds.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkManagerVRMultiplayer))]
    class VRMutliplayerTemplateNetworkManagerEditor : Editor
    {
        /// <summary>
        /// This function is called when the inspector is drawn.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                switch (XRINetworkGameManager.CurrentConnectionState.Value)
                {
                    case XRINetworkGameManager.ConnectionState.None:
                        GUILayout.Box("Authenticating");
                        break;
                    case XRINetworkGameManager.ConnectionState.Authenticating:
                        GUILayout.Box("Authenticating");
                        break;
                    case XRINetworkGameManager.ConnectionState.Authenticated:
                        if (GUILayout.Button("Connect"))
                        {
                            XRINetworkGameManager.Instance.QuickJoinLobby();
                        }
                        break;
                    case XRINetworkGameManager.ConnectionState.Connecting:
                        GUILayout.Box("Connecting");
                        break;
                    case XRINetworkGameManager.ConnectionState.Connected:
                        if (GUILayout.Button("Disconnect"))
                        {
                            XRINetworkGameManager.Instance.Disconnect();
                        }
                        break;
                }
            }
            else
            {
                GUILayout.Box("Game not running.");
            }
        }
    }
#endif
}
