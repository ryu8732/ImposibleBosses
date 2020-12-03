using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 캐릭터에 붙어다니는 UI에 대한 클래스
public class CharaterUI : MonoBehaviourPun
{
    private GameObject targetCharacter;

    private void Start()
    {
        targetCharacter = transform.parent.Find("Character").gameObject;

        if (photonView.IsMine)
        {
            // 캐릭터 상단에 띄워 줄 닉네임 텍스트를 설정하는 RPC 호출
            photonView.RPC("SetNickNameTextRPC", RpcTarget.All, PhotonNetwork.NickName);
        }      
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = targetCharacter.transform.position + new Vector3(0f, 0.05f, 0f);
    }

    //[PunRPC]
    //private void RequestSetNickNameTextRPC()
    //{
    //    if (photonView.IsMine)
    //    {
    //        photonView.RPC("SetNickNameTextRPC", RpcTarget.All, PhotonNetwork.NickName);
    //    }
    //}

    [PunRPC]
    private void SetNickNameTextRPC(string nickName)
    {
        GetComponentInChildren<TextMesh>().text = nickName;
    }


}
