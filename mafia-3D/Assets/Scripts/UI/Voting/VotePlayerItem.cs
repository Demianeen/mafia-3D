using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;
using TMPro;

public class VotePlayerItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _statusText;

    private int _actorNumber;

    public int ActorNumber
    {
        get { return _actorNumber; }
    }

    private Button _voteButton;
    private VotingManager _votingManager;

    private void Awake()
    {
        _voteButton = GetComponentInChildren<Button>();
        _voteButton.onClick.AddListener(OnVotePressed);

        Debug.Log("VotePlayerItem has been awoken");
    }

    private void OnVotePressed()
    {
        _votingManager.CastVote(_actorNumber);
        Debug.Log("Vote was pressed");

    }

    public void Initialize(Player player, VotingManager votingManager)
    {
        _actorNumber = player.ActorNumber;
        _playerNameText.text = player.NickName;
        _statusText.text = "Not decided";
        _votingManager = votingManager;
        Debug.Log("VotePlayerItem has been initialized");
    }

    public void UpdateStatus(string status)
    {
        _statusText.text = status;
    }

    public void ToggleButton(bool isInteractable)
    {
        _voteButton.interactable = isInteractable;
        Debug.Log("Button status was changed");
    }

}
