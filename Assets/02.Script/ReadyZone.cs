using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 각 플레이어가 준비되었음을 알리기 위해 올라서는 발판에 대한 클래스이다.
public class ReadyZone : MonoBehaviour
{
    private ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
    private bool playerReady = false;

    private void OnTriggerEnter(Collider other)
    {
        // 발판에 자신의 플레이어가 닿을 경우 playerReady를 true한 뒤 프로퍼티에 담는다.
        if(other.tag == "Player" && other.GetComponent<PhotonView>().IsMine)
        {
            playerReady = true;
            playerCustomProperties["playerReady"] = playerReady;
            PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 발판에 자신의 플레이어가 닿을 경우 playerReady를 false한 뒤 프로퍼티에 담는다.
        if (other.tag == "Player" && other.GetComponent<PhotonView>().IsMine)
        {
            playerReady = false;
            playerCustomProperties["playerReady"] = playerReady;
            PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);
        }
    }
}
