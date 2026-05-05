using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InputButtons
{
    Jump = 0
}

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public float lookYaw;
    public float lookPitch;
    public NetworkButtons buttons;
}

public enum PapelJogador { Forca, Inteligencia }

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;

    public static Vector2 TouchMoveInput;
    public static bool JumpPressed;
    public static float YawInput;
    public static float PitchInput;

    public static PapelJogador PapelLocal { get; private set; } = PapelJogador.Forca;
    public static int JogadoresConectados { get; private set; } = 0;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private NetworkRunner _runner;

    private void Start()
    {
        if (_runner == null)
            StartGame(GameMode.Shared);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        Vector3 dir = new Vector3(TouchMoveInput.x, 0f, TouchMoveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        data.direction = dir;

        data.buttons.Set(InputButtons.Jump, JumpPressed);
        data.lookYaw = YawInput;
        data.lookPitch = PitchInput;

        input.Set(data);
        JumpPressed = false;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        JogadoresConectados++;

        if (player == runner.LocalPlayer)
        {
            string papelSalvo = PlayerPrefs.GetString("PapelEscolhido", "");
            if (papelSalvo == "Forca")
                PapelLocal = PapelJogador.Forca;
            else if (papelSalvo == "Inteligencia")
                PapelLocal = PapelJogador.Inteligencia;
            else
            {
                int outrosJogadores = 0;
                foreach (var p in runner.ActivePlayers)
                    if (p != runner.LocalPlayer) outrosJogadores++;
                PapelLocal = outrosJogadores == 0 ? PapelJogador.Forca : PapelJogador.Inteligencia;
            }

            runner.Spawn(_playerPrefab, transform.position, transform.rotation, player);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        JogadoresConectados--;
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    async void StartGame(GameMode mode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 10
        });
    }
}
