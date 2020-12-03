using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Knight : PlayerAttack
{
    public GameObject skill1Obj;
    public float skill1Cooltime = 10.0f;
    public float skill1Durationtime = 5.0f;

    public GameObject attackAreaObj;    // 기본 공격의 범위를 담당하는 오브젝트
    private DetectArea attackArea;      // 위의 오브젝트의 충돌 감지를 위하여 콜백메서드를 구현한 스크립트

    private LivingEntity target;        // 공격 범위에 들어온 적을 인식

    public override void Awake()
    {
        base.Awake();

        attackArea = attackAreaObj.GetComponent<DetectArea>();
        attackArea.CollisionStayEvent += AttackOnTriggerStay;       // 액션 이벤트 추가
        attackArea.CollisionExitEvent += AttackOnTriggerExit;       // 액션 이벤트 추가
    }

    public override bool Attack()
    {
        if (base.Attack() && target != null)      // 범위내에 적이 있을 경우 target의 OnDamage 메소드를 실행하여 해당 객체에 데미지를 가한다.
        {
            //RaycastHit hit;

            //// 캐릭터 -> 타겟 방향으로 레이를 쏘아 맞은 표면의 좌표를 구한다. 해당 위치에 이펙트를 생성하기 위함
            //if (Physics.Raycast(transform.position + new Vector3(0f, 0.1f, 0f), target.transform.position - transform.position, out hit, 10f, 1 << LayerMask.NameToLayer("Enemy")))
            //{
            //    target.photonView.RPC("OnDamage", RpcTarget.MasterClient, GetComponent<LivingEntity>().meleeDamage, hit.point, Vector3.zero);
            //}


            // ClosestPoint 메소드를 활용하여 target의 콜라이더에서 Knight 오브젝트와 가장 가까운 점의 위치정보를 패러미터로 넘겨준다.(피격 이펙트 위치)
            // RayCast를 활용하는 것 보다 직관적이며 RayCast에 대한 부하를 줄여 줄 것이다.
            target.photonView.RPC("OnDamage", RpcTarget.MasterClient, GetComponent<LivingEntity>().meleeDamage, target.GetComponent<Collider>().ClosestPoint(transform.position), Vector3.zero);

            return true;
        }

        return false;
    }
    public override bool Skill1()
    {
        if (!base.Skill1())
        {
            return false;
        }

        // knight의 스킬1을 작동시키기 위한 RPC를 모든 클라이언트에서 실행한다.
        photonView.RPC("Skill1RPC", RpcTarget.All);

        // 스킬1의 쿨타임 코루틴을 실행한다.
        StartCoroutine("CoolTime", skill1Cooltime);

        return true;
    }

    [PunRPC]
    public void Skill1RPC()
    {
        // 스킬1의 오브젝트를 활성화하고, 스킬1의 지속시간을 패러미터로 하는 코루틴을 수행한다.
        skill1Obj.SetActive(true);
        StartCoroutine("Skill1Duration", skill1Durationtime);
    }

    IEnumerator Skill1Duration(float duration)
    {
        // 지속시간이 끝나면 스킬1을 비활성화한다.
        yield return new WaitForSeconds(duration);
        skill1Obj.SetActive(false);
    }

    // Knight 오브젝트는 적을 감지하는 오브젝트(DetectArea 스크립트)를 자식으로 두고있으며, 이 오브젝트의 OnTriggerStay 이벤트가 발생 할 경우 콜백함수로서 해당 함수를 호출한다.
    private void AttackOnTriggerStay(Collider collider)
    {
        if (collider.transform.tag == "Enemy")
        {
            target = collider.GetComponent<LivingEntity>();      // 범위 내에 적이 있을 경우 target에 해당 적의 IDamagable 컴포넌트를 할당          
        }
    }

    private void AttackOnTriggerExit(Collider collider)
    {
        if (collider.transform.tag == "Enemy")
        {
            target = null;
        }
    }
}
