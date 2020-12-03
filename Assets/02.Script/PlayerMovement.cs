using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    private Joystick joyStick;

    private float moveSpeed = 2.0f;
    private float jumpForce = 5.0f;

    private Animator playerAnimator;
    private Rigidbody playerRigidbody;

    private AudioSource audioSource;
    public AudioClip stepClip;
    public AudioClip jumpClip;

    private Vector3 currentTilePosition;

    private PlayerStatement playerStatement;

    // Start is called before the first frame update
    void Start()
    {
        joyStick = FindObjectOfType<Joystick>().GetComponent<Joystick>();

        playerAnimator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();

        playerStatement = GetComponent<PlayerStatement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 로컬 플레이어만이 움직임 조정
        if(GameManager.instance.isGameover || !photonView.IsMine || playerStatement.currentState == PlayerStatement.State.Attack || playerStatement.currentState == PlayerStatement.State.Skill)
        {
            return;
        }

        Move();
    }

    private void Move()
    {
        playerAnimator.SetFloat("Move", joyStick.joyDisRatio);

        if (joyStick.isMove)
        {
            // 아탄을 이용하여 조이스틱의 각도를 구한 뒤 그만큼 y축에 대하여 캐릭터를 회전한다.
            transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(joyStick.JoyVec.x, joyStick.JoyVec.y) * Mathf.Rad2Deg, 0));
            transform.Translate(Vector3.forward * Time.deltaTime * (joyStick.joyDisRatio * moveSpeed));

            // 오디오 소스가 실행중이 아니고, 점프중이 아니며, 공격중이 아닌 경우에만 이동 가능
            if (!audioSource.isPlaying && playerStatement.currentState != PlayerStatement.State.Jump && playerStatement.currentState != PlayerStatement.State.Attack)
            {
                audioSource.PlayOneShot(stepClip, 0.1f);
                playerStatement.currentState = PlayerStatement.State.Move;
            }
        }

        else
        {
            // 조이스틱이 움직이지 않았으며 currentState가 Move일 경우 Idle로 상태전이
            if(playerStatement.currentState == PlayerStatement.State.Move)
            {
                playerStatement.currentState = PlayerStatement.State.Idle;
            }
        }
    }

 
    public void Jump()
    {
        // 점프, 공격, 스킬 상태가 아닌 경우에만 점프가 가능하다.
        if(playerStatement.currentState != PlayerStatement.State.Jump && playerStatement.currentState != PlayerStatement.State.Attack && playerStatement.currentState != PlayerStatement.State.Skill)
        {
            playerAnimator.SetBool("Jump", true);
            // 점프로 상태 전이
            playerStatement.currentState = PlayerStatement.State.Jump;
            audioSource.PlayOneShot(jumpClip);

            // 점프 직전엔 속도를 0으로 만드는 것이 슈퍼 점프와 같은 버그 발생을 방지할 수 있다.
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            // 오브젝트의 윗면 (캐릭터 기준으론 바닥) 에 닿았을 경우에만 (벽에 닿은 경우를 배제하기 위해서)
            if (collision.contacts[0].normal.y > 0.7f)
            {
                // 최근 상태가 점프 였다면 점프 후 바닥에 닿은 상태로 Idle 상태로 전이
                if (playerStatement.currentState == PlayerStatement.State.Jump)
                {
                    playerAnimator.SetBool("Jump", false);
                    playerStatement.currentState = PlayerStatement.State.Idle;
                }

                // 맵 밖을 벗어날 경우 낙하 데미지를 입게되는데, 이 때 캐릭터를 이동 시킬 위치를 가장 마지막에 밟은 타일로 설정하였으며 이 타일의 위치가 저장된다.
                currentTilePosition = collision.gameObject.transform.position;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            // 캐릭터가 낙하하여 특정 위치에 도달하여 FallZone 오브젝트에 닿았을 경우
            if (other.transform.tag == "Fall")
            {
                // 캐릭터를 마지막에 밟은 타일로 이동시킨다.
                playerRigidbody.velocity = Vector3.zero;
                transform.position = currentTilePosition + new Vector3(0.0f, 0.55f, 0.0f);

                // 캐릭터에게 최대체력의 절반의 피해를 입힌다.
                LivingEntity livingEntity = GetComponent<LivingEntity>();
                livingEntity.photonView.RPC("OnDamage", RpcTarget.MasterClient, livingEntity.startingHealth / 2.0f, Vector3.zero, Vector3.zero);
            }
        }
    }
}
