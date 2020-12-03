using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSM : LivingEntity, IPunObservable
{
    // Enemy 객체가 가지는 상태
    public enum State
    {
        Idle,
        Move,
        Attack,
        SkillReady,
        Skill1,
        Skill2
    };

    public State currentState = State.Idle;

    protected WaitForSeconds Delay500 = new WaitForSeconds(0.5f);
    protected WaitForSeconds Delay250 = new WaitForSeconds(0.25f);

    protected bool isTargetIn = false;  // 설정한 타겟이 공격범위 내에 있는지 확인하는 변수
    protected bool isAttack = false;    // 현재 공격 애니메이션이 동작 중인지 확인하는 변수

    protected DetectArea attackArea;      // 위의 오브젝트의 충돌 감지를 위하여 콜백메서드를 구현한 스크립트

    protected LivingEntity targetEntity;    // 타겟으로 설정한 오브젝트가 담길 변수

    protected NavMeshAgent navAgent;

    protected Animator enemyAnimator;

    protected LayerMask playerLayer;

    private float rotateSpeed = 3.0f;

    public GameObject hitEffect;    // 데미지를 입을 때 (OnDamage 메소드가 호출될 때) 나타날 효과 이펙트와 텍스트를 위한 변수
    public Transform damageTextPos;

    protected virtual void Start()
    {
        enemyAnimator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        attackArea = GetComponentInChildren<DetectArea>();
        attackArea.CollisionStayEvent += AttackOnTriggerStay;       // 액션 이벤트 추가
        attackArea.CollisionExitEvent += AttackOnTriggerExit;       // 액션 이벤트 추가

        playerLayer = LayerMask.NameToLayer("Player");


        // 타겟을 설정하고 Enemy 객체를 FSM을 통해 제어하는 것은 모두 호스트 클라이언트에서 담당한다.
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        UpdateTarget();
        StartCoroutine(FSM());
        //Fsm();
    }

    protected virtual void Update()
    {
        UpdateMana();
    }

    private void UpdateMana()
    {
        if (mana <= 1.0f)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                mana += manaRecoverySpeed * Time.deltaTime;
            }

            UIManager.instance.UpdateTargetMana();

            //Debug.Log(mana);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            stream.SendNext(health);
            stream.SendNext(mana);
        }
        else
        {
            // 리모트 오브젝트라면 읽기 부분이 실행됨         

            // 네트워크를 통해 값 받기
            health = (float)stream.ReceiveNext();
            UIManager.instance.UpdateTargetHealth();
            mana = (float)stream.ReceiveNext();
        }
    }

    // Enemy 객체는 공격이 끝날 때 마다 해당 메소드를 호출하여 타겟을 재설정한다.
    protected void UpdateTarget()
    {
        targetEntity = null;
        isTargetIn = false;

        // 가장 가까운 플레이어를 타겟으로 지정한다.
        Collider[] colliders = Physics.OverlapSphere(transform.position, 100f, 1 << playerLayer);

        float maxDistance = 9999999f;

        for (int i = 0; i < colliders.Length; i++)
        {
            LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

            if (livingEntity != null && !livingEntity.dead && maxDistance > Vector3.Distance(transform.position, livingEntity.transform.position))
            {
                maxDistance = Vector3.Distance(transform.position, livingEntity.transform.position);

                targetEntity = livingEntity;
            }
        }
    }

    // FSM 코루틴
    protected virtual IEnumerator FSM()
    {
        while (!dead)
        {
            yield return StartCoroutine(currentState.ToString());
        }
    }


    // Idle 상태에 대한 코루틴
    protected virtual IEnumerator Idle()
    {
        yield return Delay500;

        navAgent.velocity = Vector3.zero;
        enemyAnimator.SetBool("Move", false);

        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
        {
            enemyAnimator.SetTrigger("Idle");
        }

        if (targetEntity != null)
        {
            // 공격범위내에 타겟이 존재하면
            if (isTargetIn)
            {
                // Attack으로 전이
                currentState = State.Attack;
            }

            // 공격 범위내에 타겟이 존재하지 않으면
            else
            {
                // Move로 전이
                currentState = State.Move;
            }
        }

        // 만약 타겟이 없을 경우 타겟 재설정
        else
        {
            UpdateTarget();
        }
    }

    // Move 상태에 대한 코루틴
    private IEnumerator Move()
    {
        // 추적을 수행하다가 범위내에 타겟이 존재할 경우
        if (isTargetIn)
        {
            // Attack으로 전이
            currentState = State.Attack;
        }

        // 범위내에 타겟이 존재하지 않으면 계속 추적
        else
        {
            // 타겟과 자신에 대한 거리를 저장
            float dist = Vector3.Distance(transform.position, targetEntity.transform.position);

            // 타겟과의 거리가 아직 stoppingDistance보다 클 경우 계속 이동한다.
            if (navAgent.stoppingDistance < dist)
            {
                if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("run"))
                {
                    enemyAnimator.SetBool("Move", true);
                }

                navAgent.SetDestination(targetEntity.transform.position);
            }

            // 타겟과의 거리는 충족되었지만 타겟을 바라보지 않기 때문에 이를 위한 메소드 호출
            else
            {
                RotateEnemy();
            }
        }

        yield return null;
    }

    // Attack 상태에 대한 코루틴
    protected virtual IEnumerator Attack()
    {
        navAgent.velocity = Vector3.zero;

        // 공격 애니메이션 동작
        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("atkA"))
        {
            enemyAnimator.SetTrigger("Attack");
        }

        // 공격 후 Idle로 상태전이
        currentState = State.Idle;
        yield return null;
    }

    private void RotateEnemy()
    {
        // 타겟을 바라본다.
        if (!isAttack)
        {
            Vector3 dir = targetEntity.transform.position - transform.position;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotateSpeed);
        }
    }

    // 공격 범위 내에 트리거가 들어올 경우 호출된다.
    private void AttackOnTriggerStay(Collider collider)
    {
        // 호스트 클라이언트에서만 동작한다.
        if (PhotonNetwork.IsMasterClient)
        {
            LivingEntity colEntity = collider.gameObject.GetComponent<LivingEntity>();

            // 맞닿은 트리거에 대한 객체가 타겟 객체이며, 해당 타겟이 죽지 않았다면 isTargetIn을 true로 만든다.
            if (colEntity)
            {
                if (colEntity == targetEntity && !colEntity.dead)
                {
                    isTargetIn = true;
                }
            }
        }
    }

    // 공격 범위 내에서 트리거가 나갈 경우 호출된다.
    private void AttackOnTriggerExit(Collider collider)
    {
        // 호스트 클라이언트에서만 동작한다.
        if (PhotonNetwork.IsMasterClient)
        {
            LivingEntity colEntity = collider.gameObject.GetComponent<LivingEntity>();

            // 맞닿은 트리거에 대한 객체가 타겟 객체라면 isTargetIn을 false로 만든다.
            if (colEntity == targetEntity)
            {
                isTargetIn = false;
            }
        }
    }

    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        // 데미지를 입을 시 이펙트와 데미지 텍스트를 출력한다. 오브젝트 풀링 X
        //GameObject hitEffectClone = Instantiate(hitEffect, hitPoint, Quaternion.Euler(Vector3.zero));

        //GameObject damageTextClone = Instantiate(damageText, damageTextPos.position, Quaternion.Euler(Vector3.zero));
        //damageTextClone.GetComponent<DamageText>().DisplayDamage(damage);


        // 데미지를 입을 시 이펙트와 데미지 텍스트를 출력한다. 오브젝트 풀링 O
        GameObject hitEffectObj = ObjectPoolingManager.instance.GetQueue("hitEffect");
        hitEffectObj.transform.position = hitPoint;
        hitEffectObj.transform.rotation = Quaternion.identity;
        hitEffectObj.GetComponent<ParticleSystem>().Play();


        GameObject damageTextObj = ObjectPoolingManager.instance.GetQueue("hitText");
        damageTextObj.transform.position = damageTextPos.position;
        damageTextObj.transform.rotation = Quaternion.identity;
        damageTextObj.GetComponent<DamageText>().DisplayDamage(damage);


        // 데미지 텍스트의 경우 자체적으로 스크립트에서 Destroy 되므로 해당 과정 생략
        //Destroy(hitEffectClone, 0.5f);
    }

    public override void Die()
    {
        // LivingEntity의 Die() 실행(사망 적용)
        base.Die();

        // Die 애니메이션 동작
        enemyAnimator.SetTrigger("Die");

        // 적이 죽었으므로 StageClear 메소드 호출
        GameManager.instance.StageClear();

        // 5초 뒤 Enemy 객체 제거
        Destroy(this.gameObject, 5f);
    }
}
