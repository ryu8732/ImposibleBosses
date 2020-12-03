using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// 스테이지 클리어 시 로비로 돌아가기 위한 포탈 클래스
public class Portal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 캐릭터에 닿을 경우 네트워크를 종료하고 로비로 돌아간다.
        if (other.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Lobby");
        }
    }
}
