using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Network : MonoBehaviourPunCallbacks
{
    public CameraFollow playerCamera;
    [SerializeField] public TextMeshProUGUI status;
    [SerializeField] public TextMeshProUGUI amountOfPlayers;
    [SerializeField] public VotingManager votingManager;

    private void Start()
    {
        status.text = "Connecting";
        PhotonNetwork.NickName = "Player" + Random.Range(0, 5000);
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        status.text = "Connected to Master / joining room";
        PhotonNetwork.JoinOrCreateRoom(
              "GameRoom",
              new RoomOptions() { MaxPlayers = 4 }, null);
    }

    public override void OnJoinedRoom()
    {
        status.text = "Connected";
        playerCamera.target =
             PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0), Quaternion.identity).transform;
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Can't connect to server");
        }
        amountOfPlayers.text = PhotonNetwork.CurrentRoom.Players.Count.ToString();
        votingManager.ActivateEmergencyWindow();
    }
}
