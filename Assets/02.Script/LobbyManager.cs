using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public const int maxPlayerCount = 4;

    private string gameVersion = "1";

    //public Text connectionInfoText;
    public TextMeshProUGUI connectionInfoText;
    public Button joinButton;

    public GameObject connectPanel;
    //public GameObject roomPanel;
    //public GameObject layoutPanel;
    public GameObject slotPrefab;

    //public InputField myNickNameField;
    public TMP_InputField myNickNameField;
    private int myRoomNum;
    private string myNickName;

    //public GridLayout playerListLayout;

    List<GameObject> playerList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();   // 설정한 정보로 마스터 서버 접속 시도

        joinButton.interactable = false;
        //connectionInfoText.text = "마스터 서버 접속 중...";
        connectionInfoText.text = "Connecting...";
    }


    // 마스터 서버 접속 시 자동 실행
    public override void OnConnectedToMaster()
    {
        joinButton.interactable = true;
        //connectionInfoText.text = "온라인: 마스터 서버와 연결됨";
        connectionInfoText.text = "Online: Connected to server";
    }

    // 접속 실패 시 자동 실행
    public override void OnDisconnected(DisconnectCause cause)
    {
        joinButton.interactable = false;
        //connectionInfoText.text = "오프라인: 마스터 서버와 연결되지 않음\n접속 재시도 중...";
        connectionInfoText.text = "Offline: Failed to connect to server\nReconnecting ...";

        PhotonNetwork.ConnectUsingSettings();
    }

    // 룸 접속 시도
    public void Connect()
    {
        joinButton.interactable = false;    // 중복 접속 시도를 막기 위하여 비활성화

        // 마스터 서버에 접속중이라면
        if(PhotonNetwork.IsConnected)
        {
            connectionInfoText.text = "Connecting Room...";

            // 방에 접속하기 전 플레이어의 정보를 설정한다.
            SetPlayerInfo();
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            //connectionInfoText.text = "오프라인: 마스터 서버와 연결되지 않음\n접속 재시도 중...";
            connectionInfoText.text = "Offline: Failed to connect to server\nReconnecting ...";

            PhotonNetwork.ConnectUsingSettings();
        }
    }


    // 플레이어의 닉네임 및 준비 상태에 대한 커스텀 프로퍼티를 저장하기 위한 작업을 수행한다.
    public void SetPlayerInfo()
    {
        myNickName = myNickNameField.text;
        PhotonNetwork.NickName = myNickName;

        ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
        playerCustomProperties["playerReady"] = false;
        PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);
    }

    // 방이 없어서 참가에 실패한 경우 자동 실행
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionInfoText.text = "There are no empty rooms.\nCreating new room...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerCount });
    }

    // 룸에 참가 완료 한 뒤 자동 실행
    public override void OnJoinedRoom()
    {
        connectionInfoText.text = "Success to connect to room";


        PhotonNetwork.LoadLevel("Room");
    }

    // 다른 플레이어가 방에 접속 하면 자동으로 콜백
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Enter");
        
        // 플레이어가 접속할 때 마다 방의 모든 플레이어의 닉네임을 콘솔에 출력
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
        }
    }

    // 다른 플레이어가 방을 나가면 자동으로 콜백
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Left");

        // 플레이어가 떠날 때 마다 방의 모든 플레이어의 닉네임을 콘솔에 출력
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
        }
    }

    // 스마트폰의 ExitButton에 대한 작업을 수행
    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
