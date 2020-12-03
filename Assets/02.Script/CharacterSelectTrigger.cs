using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 캐릭터를 변경하기 위해 트리거를 감지 할 발판에 대한 클래스
public class CharacterSelectTrigger : MonoBehaviourPun
{

    // 변경할 캐릭터 프리팹
    public GameObject characterPrefab;
    public GameObject characterSelectPanel;

    private CharacterSelect characterSelect;

    private void Start()
    {
        characterSelect = FindObjectOfType<CharacterSelect>();
    }

    // 발판에 트리거가 들어올 경우
    private void OnTriggerEnter(Collider other)
    {
        // 접촉한 콜라이더 오브젝트가 플레이어 오브젝트이며 자신의 로컬 플레이어일 경우
        if (other.tag == "Player" && other.GetComponent<PhotonView>().IsMine)
        {
            // 캐릭터 변경을 위해 필요한 오브젝트 할당
            characterSelect.myCharacterObj = other.gameObject;
            characterSelect.selectedPrefab = characterPrefab;


            // 캐릭터 변경 패널 활성화
            characterSelectPanel.SetActive(true);
        }
    }

    // 발판에서 트리거가 나갈 경우
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && other.GetComponent<PhotonView>().IsMine)
        {
            // 캐릭터 변경 패널 비활성화
            characterSelectPanel.SetActive(false);
        }
    }
}
