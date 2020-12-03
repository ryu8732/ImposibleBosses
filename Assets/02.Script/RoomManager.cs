using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // 플레이어 커스텀 프로퍼티가 담길 해쉬 테이블
    private ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private bool isNoClicked = false;
    public GameObject GameStartPannel;
   

    private void Start()
    {
        // 방에 접속하면 플레이어의 준비 상태는 false가 된다.
        playerCustomProperties["playerReady"] = false;
        PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);
    }

    private void Update()
    {
        // 호스트 클라이언트는 모든 플레이어들이 준비상태인지 확인하며, 모두 준비 되었다면 게임 시작을 위한 패널을 활성화한다. (호스트에서 단독 실행)
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Player photonPlayer in PhotonNetwork.PlayerList)
            {
                if (!(bool)photonPlayer.CustomProperties["playerReady"])
                {
                    isNoClicked = false;
                    GameStartPannel.SetActive(false);
                    return;
                }
            }

            if (!isNoClicked)
            {
                GameStartPannel.SetActive(true);
            }
        }
    }

    // Main 씬을 호출한다(스테이지)
    [PunRPC]
    private void LoadMainSceneRPC()
    {
        PhotonNetwork.LoadLevel("Main");
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Enter " + newPlayer.NickName);

        // 플레이어 리스트를 가져와 저장했던 닉네임을 텍스트에 담는다.
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
        }
    }

    // 게임 시작 패널에서 Yes 버튼을 누를 경우
    public void OnStartYesButton()
    {
        // 방의 입장을 제한한 뒤
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // Main 씬을 호출한다.
        photonView.RPC("LoadMainSceneRPC", RpcTarget.All);
        this.gameObject.SetActive(false);
    }

    // 게임 시작 패널에서 No 버튼을 누를 경우
    public void OnStartNoButton()
    {
        // 패널을 비활성화 한다.
        isNoClicked = true;
        GameStartPannel.SetActive(false);
    }
}
