using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 구현한 KingMush Enemy에 대한 클래스
public class EnemyKingMush : EnemyFSM
{
    private float attackDelay = 3.0f;

    private HitArea hitArea;        // 공격 범위에 대한 오브젝트의 동작을 위한 클래스
    private GameObject hitAreaObj;  // 공격 범위에 대한 오브젝트

    private int boltCount;      // EnemyKing의 체력에 따라 mode 상태가 전이되고 이에 따라 사출되는 볼트의 수가 달라진다. 이를 위한 변수
    private float boltSpeed = 10f;
    private List<RaycastHit> boltHits = new List<RaycastHit>();     // 스킬1 볼트의 사출 범위를 표시할 때, 맵을 벗어나지 않게 표시하기 위한 변수

    public Transform boltGenPosition;   // 스킬1 볼트가 사출 될 위치

    private GameObject skill2Obj;   // 스킬2에 대한 오브젝트

    private AudioSource audioSource;
    public AudioClip hitClip;
    public AudioClip meleeClip;
    public AudioClip skill1Clip;
    public AudioClip skill2Clip;

    // 스테이지 클리어 시 활성화 되는 로비로 돌아가는 포탈 객체
    public GameObject portal;

    public enum mode
    {
        normal,
        rage
    }

    public mode currentMode = mode.normal;

    private void Awake()
    {
        // KingMush에 대한 기본 설정
        Setup(1000f, 10f, 0.1f);
    }
    protected override void Start()
    {
        base.Start();

        // UI에 체력을 업데이트
        UIManager.instance.UpdateTargetHealth();


        hitArea = GetComponentInChildren<HitArea>();

        hitArea.CollisionStayEvent += HitOnTriggerEnter;       // 액션 이벤트 추가
        //hitArea.CollisionExitEvent += AttackOnTriggerExit;       // 액션 이벤트 추가

        hitAreaObj = hitArea.gameObject;

        audioSource = GetComponent<AudioSource>();
    }

    protected override IEnumerator Idle()
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
            // 마나가 모두 가득차면 스킬 패턴을 사용한다.
            if (mana >= 1.0f)
            {
                if (Random.value < 0.5)
                {
                    currentState = State.Skill1;
                }
                else
                {
                    currentState = State.Skill2;
                }
                // 스킬을 사용 한 뒤, 마나를 0으로 초기화
                mana = 0f;
            }

            else
            {
                // 공격범위내에 타겟이 존재하며
                if (isTargetIn)
                {
                    // 공격 중이 아니라면
                    if (currentState != State.Attack)
                    {
                        // Attack으로 전이
                        currentState = State.Attack;
                    }

                    // 그 외의 상황에는 
                    else
                    {
                        currentState = State.Idle;
                    }
                }

                // 공격 범위내에 타겟이 존재하지 않으면
                else
                {
                    // Move로 전이
                    currentState = State.Move;
                }
            }
        }

        else
        {
            UpdateTarget();
        }
    }

    protected override IEnumerator Attack()
    {
        enemyAnimator.SetBool("Move", false);
        navAgent.velocity = Vector3.zero;

        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("atkA"))
        {
            enemyAnimator.SetBool("Attack", true);
        }

        yield return new WaitForSeconds(attackDelay);

        // 공격이 끝난 뒤 타겟을 재설정하고 Idle로 상태 전이
        UpdateTarget();
        currentState = State.Idle;
    }

    // 마스터 클라이언트에서 필요한 방향 변수 계산
    [PunRPC]
    private void skill1Calc() {
        // boltHits를 초기화한다.
        boltHits.Clear();

        // mode 상태에 따라 볼트 갯수 설정
        switch (currentMode)
        {
            case mode.normal:
                boltCount = 5;
                break;

            case mode.rage:
                boltCount = 11;
                break;
        }

        // skill1Effect RPC를 모든 클라이언트에서 실행
        photonView.RPC("skill1Effect", RpcTarget.All, boltCount, targetEntity.transform.position);
    }

    [PunRPC]
    private void skill1Effect(int boltCount, Vector3 targetTransfrom)
    {
        transform.LookAt(targetTransfrom);

        Vector3 drawPosition = new Vector3(boltGenPosition.position.x, boltGenPosition.position.y, boltGenPosition.position.z);

        for (int i = 0; i < boltCount; i++)
        {
            // 여러개의 볼트를 다양한 각도로 사출하기 위함
            Vector3 newDir = Quaternion.Euler(0, (30f * i) - (boltCount * 15f), 0) * boltGenPosition.transform.forward;

            // Wall 레이어에 대한 마스크를 씌운 레이캐스트
            Physics.Raycast(drawPosition, newDir, out RaycastHit hit, 30f, 1 << 10);
            // Wall 레이어에 닿는 레이에 대한 정보를 저장한다.
            boltHits.Add(hit);

            // 볼트가 사출될 라인을 트레일 렌더러를 활용하여 미리 표시해준다.
            if (hit.transform.tag == "Wall")
            {
                // 오브젝트 풀링 방식 사용 X
                //GameObject boltLineClone = Instantiate(boltLine, drawPosition, Quaternion.Euler(90f, 0f, 0f));
                //boltLineClone.GetComponent<BoltLine>().EndPosition = hit.point;

                // 오브젝트 풀링 방식 사용 O
                GameObject lineObj = ObjectPoolingManager.instance.GetQueue("boltLine");
                lineObj.transform.position = drawPosition;
                lineObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                lineObj.GetComponent<BoltLine>().EndPosition = hit.point;
            }
        }
    }

    // Skill1 상태에 대한 코루틴
    private IEnumerator Skill1()
    {
        //  스킬을 쓰기 전 예비 동작에 대한 애니메이션을 작동
        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("skillReady"))
        {
            enemyAnimator.SetBool("SkillReady", true);
        }

        // 스킬1에 대하여 라인을 그리기 위한 계산을 수행할 RPC 호출
        photonView.RPC("skill1Calc", RpcTarget.MasterClient);


        // 라인을 그린 뒤 1초 후 볼트를 사출한다.
        yield return new WaitForSeconds(1.0f);

        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("skill1"))
        {
            enemyAnimator.SetBool("SkillReady", false);
            enemyAnimator.SetBool("Skill1", true);
        }


        yield return new WaitForSeconds(attackDelay);

        // 타겟을 재설정 하고 Idle 상태로 전이
        UpdateTarget();
        currentState = State.Idle;
    }

    // 스킬2의 이펙트에 대한 RPC
    [PunRPC]
    private void skill2Effect()
    {
        // 오브젝트 풀링 사용
        skill2Obj = ObjectPoolingManager.instance.GetQueue("mushSkill2");
        skill2Obj.transform.position = transform.position;
        skill2Obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 스킬2의 범위를 mode 상태에 따라 설정한다.
        switch (currentMode)
        {
            case mode.normal:
                skill2Obj.GetComponent<Skill2Area>().skill2Area = 9.0f;
                break;

            case mode.rage:
                skill2Obj.GetComponent<Skill2Area>().skill2Area = 12.0f;
                break;
        }
    }

    // Skill2 상태에 대한 코루틴
    private IEnumerator Skill2()
    {

        //  스킬을 쓰기 전 예비 동작에 대한 애니메이션을 작동
        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("skillReady"))
        {
            enemyAnimator.SetBool("SkillReady", true);
        }


        // 모든 플레이어에서 skill2Effect RPC 호출 (스킬 2의 범위를 표시해준다.)
        photonView.RPC("skill2Effect", RpcTarget.All);


        yield return new WaitForSeconds(2.0f);

        // 2초 후, Skill2 동작 (Skill2 애니메이션의 이벤트 메소드를 통해 데미지를 입힌다.)
        if (!enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("skill2"))
        {
            enemyAnimator.SetBool("SkillReady", false);
            enemyAnimator.SetBool("Skill2", true);
        }


        yield return new WaitForSeconds(attackDelay);

        // 타겟을 재설정 하고 Idle 상태로 전이
        UpdateTarget();
        currentState = State.Idle;
    }

    // 스킬2의 마지막 프레임에서 호출되는 이벤트 메소드
    private void Skill2EndEvent()
    {
        enemyAnimator.SetBool("Skill2", false);

        // 카메라 노이즈를 통해 흔들림 구현
        GameManager.instance.CameraShake(0.5f, 10.0f, 3.0f);

        // 콜라이더가 enable 되면, skill2Obj의 OnTriggerEnter가 발동되어 Player 태그의 객체에 데미지를 입힌다.
        skill2Obj.GetComponent<SphereCollider>().enabled = true;
        audioSource.PlayOneShot(skill2Clip);


        // 데미지를 입힌 뒤 skill2Obj를 제거하기 위한 코루틴 실행
        StartCoroutine(skill2Obj.GetComponent<Skill2Area>().DestroySkill2());
    }

    // Skill1 애니메이션 도중 볼트가 사출되는 프레임에서 호출되는 이벤트 메소드
    private void BoltEvent()
    {
        enemyAnimator.SetBool("Skill1", false);

        for (int i = 0; i < boltHits.Count; i++)
        {
            audioSource.PlayOneShot(skill1Clip, 0.05f);

            // 오브젝트 풀링 방식 사용 X
            //GameObject boltClone = Instantiate(bolt, boltGenPosition.position, boltGenPosition.rotation);
            //boltClone.GetComponent<Rigidbody>().velocity = (boltHits[i].point - boltGenPosition.position).normalized * boltSpeed;

            // 볼트를 생성하고 사출한다.
            // 오브젝트 풀링 방식 사용 O
            GameObject boltObj = ObjectPoolingManager.instance.GetQueue("bolt");
            boltObj.transform.position = boltGenPosition.position;
            boltObj.transform.rotation = boltGenPosition.rotation;
            boltObj.GetComponent<Rigidbody>().velocity = (boltHits[i].point - boltGenPosition.position).normalized * boltSpeed;
        }
    }

    private void EndOfAttackEvent()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentState = State.Idle;

            hitAreaObj.GetComponent<Collider>().enabled = false;
        }
    }

    private void HitOfEvent()
    {
        enemyAnimator.SetBool("Attack", false);
        GameManager.instance.CameraShake(0.3f, 1.2f, 2.0f);
        hitAreaObj.GetComponent<Collider>().enabled = true;
        audioSource.PlayOneShot(meleeClip, 0.2f);
    }

    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!dead) {
            base.OnDamage(damage, hitPoint, hitNormal);
            currentMode = health <= startingHealth / 2.0 ? mode.rage : mode.normal;
            Debug.Log("King");

            audioSource.PlayOneShot(hitClip, 0.1f);

            UIManager.instance.UpdateTargetHealth();


            //photonView.RPC("OnDamageRPC", RpcTarget.MasterClient, damage, hitPoint, hitNormal);
        }
    }

    //[PunRPC]
    //public void OnDamageRPC(float damage, Vector3 hitPoint, Vector3 hitNormal)
    //{
    //    base.OnDamage(damage, hitPoint, hitNormal);
    //    currentMode = health <= startingHealth / 2.0 ? mode.rage : mode.normal;
    //    Debug.Log("King");

    //    audioSource.PlayOneShot(hitClip, 0.1f);

    //    UIManager.instance.UpdateTargetHealth();
    //}

    private void HitOnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag == "Player" && PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("Hit Enter");
            IDamageable damageable = collider.GetComponent<IDamageable>();
            damageable.OnDamage(meleeDamage, Vector3.zero, Vector3.zero);

            //LivingEntity livingEntity = collider.GetComponent<LivingEntity>();
            //livingEntity.photonView.RPC("OnDamage", RpcTarget.MasterClient, meleeDamage, Vector3.zero, Vector3.zero);

        }
    }

    private void HitOnTriggerExit(Collider collider)
    {
        //Debug.Log("Hit Exit");
    }

    public override void Die()
    {
        // LivingEntity의 Die() 실행(사망 적용)
        base.Die();

        portal.SetActive(true);

        if (skill2Obj != null)
        {
            StartCoroutine(skill2Obj.GetComponent<Skill2Area>().DestroySkill2());
        }
    }
}
