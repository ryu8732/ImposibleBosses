using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelect : MonoBehaviourPun
{
    public GameObject selectedPrefab;
    public GameObject characterSelectPanel;
    public Transform spawnPos;

    public GameObject myCharacterObj;

    private ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();

    // 캐릭터 변경에 대하여 Yes 버튼을 클릭할 경우
    public void OnChangeYesClicked()
    {
        // 현재 자신의 캐릭터를 삭제한다.
        PhotonNetwork.Destroy(myCharacterObj.transform.parent.gameObject);

        // 플레이어 커스텀 프로퍼티에서 변경할 캐릭터에 대한 정보를 업데이트 한다.
        playerCustomProperties["playerCharacter"] = selectedPrefab.name;
        PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);

        // 선택한 캐릭터를 생성한다.
        PhotonNetwork.Instantiate(selectedPrefab.name, spawnPos.position, Quaternion.identity);

        characterSelectPanel.SetActive(false);

        // 새로운 캐릭터에 대하여 UIManager 스크립트에서 요구되는 스크립트들을 재설정해준다.
        UIManager.instance.SetupPlayer();
    }


    public void OnChangeNoClicked()
    {
        // 패널을 닫는다.
        characterSelectPanel.SetActive(false);
    }
}
