using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class VotingManager : MonoBehaviourPun
{
    public static VotingManager Instance;

    [SerializeField] private GameObject _meetingWindow;
    [SerializeField] private Button _skipVoteBtn;
    [SerializeField] private VotePlayerItem _votePlayerItemPrefab;
    [HideInInspector] private bool HasAlreadyVoted;
    [SerializeField] private GameObject _actionWithVotedPlayerWindow;
    [SerializeField] private GameObject _actionedPlayerWindow;
    [SerializeField] private TextMeshProUGUI _actionWithPlayerText;
    [SerializeField] private Transform _votePlayerItemContainer;
    [SerializeField] public TextMeshProUGUI amountOfPlayers;
    private List<VotePlayerItem> _votePlayerItemList = new List<VotePlayerItem>();
    private List<int> _playersThatVotedList = new List<int>();
    private List<int> _playersThatHaveBeenVotedList = new List<int>();

    public void Awake()
    {
        Instance = this;
    }

    public void ActivateEmergencyWindow()
    {
        photonView.RPC("UpdateStatusRPC", RpcTarget.All);
        PopulatePlayerList();
        _meetingWindow.SetActive(true);
    }

    private void PopulatePlayerList()
    {
        // Clear previous player list
        for (int i = 0; i < _votePlayerItemList.Count; i++)
        {
            Destroy(_votePlayerItemList[i].gameObject);
        }

        _votePlayerItemList.Clear();
        Debug.Log(PhotonNetwork.CurrentRoom.Players);

        // Create new player list
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            // if (player.Value.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            // {
            //     continue;
            // }
            Debug.Log("VotePlayerItem has been instantiated");
            VotePlayerItem newPlayerItem = Instantiate(_votePlayerItemPrefab, _votePlayerItemContainer);
            newPlayerItem.Initialize(player.Value, this);

            _votePlayerItemList.Add(newPlayerItem);
        }
    }

    private void ToggleAllButtons(bool areOn)
    {
        _skipVoteBtn.interactable = areOn;
        Debug.Log(_votePlayerItemList);
        foreach (VotePlayerItem votePlayerItem in _votePlayerItemList)
        {
            votePlayerItem.ToggleButton(areOn);
        }
    }

    public void CastVote(int targetActionNumber)
    {
        if (HasAlreadyVoted)
        {
            return;
        }

        HasAlreadyVoted = true;
        ToggleAllButtons(false);
        photonView.RPC("CastPlayerVoteRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, targetActionNumber);
    }

    [PunRPC]
    public void UpdateStatusRPC()
    {
        PopulatePlayerList();
        amountOfPlayers.text = PhotonNetwork.CurrentRoom.Players.Count.ToString();
    }
    [PunRPC]
    public void CastPlayerVoteRPC(int actorNumber, int targetActorNumber)
    {
        int remainingPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        // Set the status of the player that have just voted
        foreach (VotePlayerItem votePlayerItem in _votePlayerItemList)
        {
            if (votePlayerItem.ActorNumber == actorNumber)
            {
                votePlayerItem.UpdateStatus(targetActorNumber == -1 ? "SKIPPED" : "VOTED");
            }
        }

        // Log the players that just voted and for which voted 
        if (!_playersThatVotedList.Contains(actorNumber))
        {
            _playersThatVotedList.Add(actorNumber);
            _playersThatHaveBeenVotedList.Add(actorNumber);
        }

        if (!PhotonNetwork.IsMasterClient) { return; }
        if (_playersThatVotedList.Count < remainingPlayers) { return; }

        // Count all the votes
        Dictionary<int, int> playerVoteCount = new Dictionary<int, int>();
        foreach (int votedPlayer in _playersThatHaveBeenVotedList)
        {
            if (!playerVoteCount.ContainsKey(votedPlayer))
            {
                playerVoteCount.Add(votedPlayer, 0);
            }
            playerVoteCount[votedPlayer]++;
        }
        int mostVotedPlayer = -1;
        int mostVotes = int.MinValue;
        foreach (KeyValuePair<int, int> playerVote in playerVoteCount)
        {
            if (playerVote.Value > mostVotes)
            {
                mostVotes = playerVote.Value;
                mostVotedPlayer = playerVote.Key;
            }
        }

        // End the voting session
        if (mostVotes >= remainingPlayers / 2)
        {
            // Make decision or skip
            photonView.RPC("ActionWithVotedPlayerRPC", RpcTarget.All, mostVotedPlayer);
        }
    }

    [PunRPC]
    public void ActionWithVotedPlayerRPC(int actorNumber)
    {
        _meetingWindow.SetActive(false);
        _actionWithVotedPlayerWindow.SetActive(true);

        string playerName = string.Empty;
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            if (player.Value.ActorNumber == actorNumber)
            {
                playerName = player.Value.NickName;
                break;
            }
        }
        _actionWithPlayerText.text = actorNumber == -1 ? "No one has been actioned" : $"Player {playerName} has been actioned";

        // Action with voted player
        StartCoroutine(ActionWithPlayer(actorNumber));
    }
    private IEnumerator ActionWithPlayer(int actorNumber)
    {
        yield return new WaitForSeconds(3f);
        _actionWithVotedPlayerWindow.SetActive(false);
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            // ...some action with voted player
            _actionedPlayerWindow.SetActive(true);
            _playersThatVotedList.Clear();
            _playersThatHaveBeenVotedList.Clear();
            HasAlreadyVoted = false;
            ToggleAllButtons(true);
        }
    }
}
