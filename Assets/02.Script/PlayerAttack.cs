using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerAttack : MonoBehaviourPun
{
    protected Animator playerAnimator;

    private int attackCombo;          // 기본 공격의 모션을 나누어 버튼 클릭마다 다른 모션이 나오도록 하기 위함
    protected float attackDelay = 0.8f;   // 기본 공격의 딜레이 변수

    protected float currentAttackTime;    // 기본 공격의 딜레이를 계산하기 위한 변수

    private AudioSource audioSource;    // 오디오 소스 오브젝트
    public AudioClip[] atkClips;        // 공격할 때의 오디오 클립

    protected PlayerStatement playerStatement;

    private bool isSkill1Cooldown = false;  // 스킬1에 대한 쿨타임을 확인하기 위한 변수

    public virtual void Awake()
    {
        attackCombo = 0;
        currentAttackTime = Time.time;

        playerAnimator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        playerStatement = GetComponent<PlayerStatement>();
    }

    public virtual bool Attack()
    {
        // 자신의 로컬 캐릭터이며, 공격딜레이를 충족하고 점프,공격,스킬 상태가 아닐 때에만 공격을 수행한다.
        if (GameManager.instance.isGameover || !photonView.IsMine || Time.time < currentAttackTime + attackDelay || playerStatement.currentState == PlayerStatement.State.Jump || playerStatement.currentState == PlayerStatement.State.Attack || playerStatement.currentState == PlayerStatement.State.Skill)
        {
            return false;
        }

        // 공격 딜레이를 설정하기 위해 최근에 공격한 시간을 저장한다.
        currentAttackTime = Time.time;

        // 공격 시 마다 지정된 순서로 콤보 공격이 나가도록 한다.
        attackCombo++;
        if (attackCombo > atkClips.Length)
        {
            attackCombo = 1;
        }

        // 트리거를 통해 애니메이션 동작
        playerAnimator.SetTrigger("Attack");
        playerAnimator.SetFloat("AttackCombo", attackCombo);

        // Attack으로 상태전이
        playerStatement.currentState = PlayerStatement.State.Attack;

        // 공격 오디오 출력
        audioSource.PlayOneShot(atkClips[attackCombo - 1], 0.3f);

        return true;

        // 다른 플레이어에게도 공격 소리가 나게 하고 싶을 경우 RPC 활용
        //photonView.RPC("attackSoundRPC", RpcTarget.Others, attackCombo);
    }

    //[PunRPC]
    //private void attackSoundRPC(int combo)
    //{
    //    audioSource.PlayOneShot(atkClips[combo - 1], 0.1f);
    //}

    public virtual bool Skill1()
    {
        // 점프중이거나 쿨타임 일 때는 동작하지 않는다.
        if (!photonView.IsMine || playerStatement.currentState == PlayerStatement.State.Jump || playerStatement.currentState == PlayerStatement.State.Attack || isSkill1Cooldown)
        {
            return false;
        }

        // 스킬 상태로 상태전이 및 트리거를 통한 애니메이션 호출
        playerStatement.currentState = PlayerStatement.State.Skill;
        playerAnimator.SetTrigger("Skill1");

        // 현재 쿨타임이 돌아오지 않았음을 알리기 위하여 true로 설정
        isSkill1Cooldown = true;

        return true;
    }

    // 공격 모션이 끝나는 마지막 프레임에 동작하는 이벤트 메소드로, Idle 상태로 전이한다.
    public void EndOfAttack()
    {
        playerStatement.currentState = PlayerStatement.State.Idle;
    }

    // 패러미터로 받아온 cool만큼의 시간이 흐르면 쿨타임을 리셋한다.
    // 이때, CoolImage를 통해 쿨타임이 얼마나 남았는지를 UI를 통해 플레이어에게 보여준다.
    protected IEnumerator CoolTime(float cool)
    {
        float temp = cool;
        while (temp > 0.0f)
        {
            temp -= Time.deltaTime;
            UIManager.instance.CoolImage.fillAmount = temp / cool;

            yield return new WaitForFixedUpdate();
        }
        isSkill1Cooldown = false;
    }
}
