using System.Collections;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System;

// 점수와 게임 오버 여부를 관리하는 게임 매니저
public class GameManager : MonoBehaviourPunCallbacks
{
    // 싱글톤 접근용 프로퍼티
    public static GameManager instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<GameManager>();
            }

            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    private static GameManager m_instance; // 싱글톤이 할당될 static 변수

    private string playerPrefabName;

    public float shakeDuration = 0.3f;      // 카메라 쉐이킹 시간
    public float shakeAmplitude = 1.2f;     // 시네머신 노이즈의 파라미터 값
    public float shakeFrequency = 2.0f;     // 시네머신 노이즈의 파라미터 값

    public float shakeElapsedTime = 0f;

    public CinemachineVirtualCamera virtualCamera;  // 캐릭터를 따라다닐 가상카메라
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;  // 카메라의 흔들림을 구현하기 위한 변수

    public GameObject myCharacter;  // 자신의 캐릭터가 담길 오브젝트

    public GameObject playerSpawnPos;   // 캐릭터가 스폰 될 위치를 담은 오브젝트
    public GameObject[] playerSpawnPosArr = new GameObject[4];

    private ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable(); // 플레이어 커스텀 프로퍼티에 대한 해쉬 테이블

    public int deadPlayerNum = 0;   // 게임오버를 판단하기 위한 죽은 플레이어의 숫자

    public bool isGameover { get; private set; } // 게임 오버 상태

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if(virtualCamera != null)
        {
            // 가상카메라로부터 컴포넌트를 가져온다.
            virtualCameraNoise = virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        }

        // 방에 접속한 모든 플레이어의 숫자만큼 반복
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];


            if (player.IsLocal)
            {
                // 자신의 캐릭터가 스폰 될 위치를 결정
                Transform SpawnPosition = playerSpawnPos.transform.childCount == 0 ? playerSpawnPos.transform : playerSpawnPosArr[i].transform;

                // Room 씬에서 자신의 커스텀 프로퍼티에 캐릭터가 등록되어있지 않다면 knight로 설정한 뒤 프로퍼티에 적용하고, 캐릭터 생성
                if (player.CustomProperties["playerCharacter"] == null)
                {
                    Debug.Log("in");
                    playerCustomProperties["playerCharacter"] = "knight";
                    PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);

                    myCharacter = PhotonNetwork.Instantiate("knight", SpawnPosition.position, Quaternion.identity);
                }

                // 등록되어 있다면 해당 캐릭터 생성
                else
                {
                    myCharacter = PhotonNetwork.Instantiate((string)player.CustomProperties["playerCharacter"], SpawnPosition.transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void Update()
    {
        // 카메라의 흔들림을 구현하기 위함
        // CameraShake 메소드로부터 받아온 패러미터를 이용하여 노이즈 구현
        if (shakeElapsedTime > 0f)
        {
            virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
            virtualCameraNoise.m_FrequencyGain = shakeFrequency;

            shakeElapsedTime -= Time.deltaTime;
        }

        else
        {
            virtualCameraNoise.m_AmplitudeGain = 0f;
            shakeElapsedTime = 0f;
        }
    }

    // 카메라 흔들림(노이즈)을 위한 변수를 설정하는 메소드
    public void CameraShake(float duration, float amplitude, float frequency)
    {
        shakeDuration = duration;      // 카메라 쉐이킹 시간
        shakeAmplitude = amplitude;     // 시네머신 노이즈의 파라미터 값
        shakeFrequency = frequency;     // 시네머신 노이즈의 파라미터 값

        shakeElapsedTime = shakeDuration;
    }

    [PunRPC]
    // 게임 오버 처리
    public void EndGame()
    {
        // 게임 오버 상태를 참으로 변경
        isGameover = true;
        Debug.Log("EndGame");
        // 게임 오버 UI를 활성화
        UIManager.instance.SetActiveGameoverUI();
    }

    // 스테이지 클리어 시 수행되는 메소드
    public void StageClear()
    {
        // 자신의 캐릭터가 죽어있다면 RPC를 통해 모든 클라이언트에서 되살린다.
        if(myCharacter.GetComponentInChildren<PlayerStatement>().dead)
        {
            photonView.RPC("StageClearRPC", RpcTarget.All, myCharacter.GetComponent<PhotonView>().ViewID);
        }
    }

    // 받아온 ViewID에 해당하는 캐릭터의 OnEnable을 적용하여 캐릭터를 되살린다.
    [PunRPC]
    public void StageClearRPC(int viewID)
    {
        PhotonView.Find(viewID).gameObject.SetActive(false);
        PhotonView.Find(viewID).gameObject.SetActive(true);
    }
}