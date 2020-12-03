using UnityEngine;
using UnityEngine.UI; // UI 관련 코드
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerStatement : LivingEntity, IPunObservable
{
    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    //public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    //public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerAttack playerAttack;  // 플레이어 공격 컴포넌트

    public GameObject damageText;

    private int deadPlayerNum = 0;

    public enum State
    {
        Idle,
        Move,
        Jump,
        Attack,
        Skill
    };

    public State currentState;

    private void Awake()
    {
        Setup(100f, 100f, 0.1f);
        // 사용할 컴포넌트를 가져오기
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponents<AudioSource>()[1];

        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        Debug.Log(currentState);
    }

    public override void Setup(float newHealth, float newDamage, float newManaRecoverySpeed)
    {
        base.Setup(newHealth, newDamage, newManaRecoverySpeed);
    }

    protected override void OnEnable()
    {
        // LivingEntity의 OnEnable() 실행 (상태 초기화)
        base.OnEnable();

        healthSlider.gameObject.SetActive(true);
        healthSlider.maxValue = startingHealth;
        healthSlider.value = health;

        playerMovement.enabled = true;
        playerAttack.enabled = true;

        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        //GetComponent<Collider>().enabled = true;

        // Enemy 태그를 가진 오브젝트와의 충돌을 다시 활성화한다.
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), enemy.GetComponent<Collider>(), false);
        }

        currentState = State.Idle;

        UIManager.instance.ResetButtons();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }
        else
        {
            // 리모트 오브젝트라면 읽기 부분이 실행됨         

            // 네트워크를 통해 값 받기
            health = (float)stream.ReceiveNext();
            healthSlider.value = health;
        }
    }

    // 체력 회복
    public override void RestoreHealth(float newHealth)
    {
        // LivingEntity의 RestoreHealth() 실행 (체력 증가)
        base.RestoreHealth(newHealth);

        healthSlider.value = health;
    }

    [PunRPC]
    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (!dead)
        {
            // 자신의 캐릭터에 대하여 애니메이션 및 오디오 출력
            if (!playerAudioPlayer.isPlaying && photonView.IsMine)
            {
                playerAudioPlayer.PlayOneShot(hitClip, 0.5f);

                // 공격, 스킬 상태에서는 애니메이션을 생략한다.
                if (currentState != State.Attack && currentState != State.Skill)
                {
                    playerAnimator.SetTrigger("Damage");
                }
            }

            // LivingEntity의 OnDamage() 실행(실데미지 적용)
            base.OnDamage(damage, hitPoint, hitDirection);
            healthSlider.value = health;
        }
    }

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의 Die() 실행(사망 적용)
        base.Die();

        // 애니메이션 동작
        if (photonView.IsMine)
        {
            playerAnimator.ResetTrigger("Damage");
            playerAnimator.SetTrigger("Die");
        }

        // 체력 UI 비활성화
        healthSlider.gameObject.SetActive(false);

        // 캐릭터 동작을 위한 컴포넌트 비활성화
        playerMovement.enabled = false;
        playerAttack.enabled = false;

        // Enemy 태그를 가진 오브젝트와의 충돌을 무시해준다.
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies) {
            Physics.IgnoreCollision(GetComponent<Collider>(), enemy.GetComponent<Collider>());
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // 게임오버를 체크하는 메소드 호출
            CheckGameOver();
        }
        
    }

    // 게임오버를 체크하는 메소드
    private void CheckGameOver()
    {
        // 죽은 플레이어 숫자를 1 증가시키고
        GameManager.instance.deadPlayerNum++;

        // 방의 인원과 죽은 플레이어의 숫자를 비교하여 모두 죽었을 경우 GameManager의 EndGame RPC를 호출하여 스테이지를 종료한다.
        if (PhotonNetwork.PlayerList.Length <= GameManager.instance.deadPlayerNum)
        {
            GameManager.instance.photonView.RPC("EndGame", RpcTarget.All);
        }
    }
}