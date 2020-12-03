using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Archer : PlayerAttack
{
    //public GameObject skill1;
    private float skill1Cooltime = 10.0f;
    private float skill1Durationtime = 1.5f;

    private LivingEntity target;        // 공격 범위에 들어온 적을 인식

    public Transform arrowGenPosition;  // 화살이 발사되는 시작 위치
    public Transform arrowSkill1GenPosition;    // 스킬1의 화살이 발사되는 시작 위치
    private float arrowSpeed = 5.0f;

    public override void Awake()
    {
        base.Awake();
    }

    public override bool Attack()
    {
        if (base.Attack())
        {
            // 모든 클라이언트에서 공격에 대한 RPC를 실행한다.
            photonView.RPC("AttackRPC", RpcTarget.All);
        }
        return false;
    }

    // 화살을 생성하여 발사한다.
    [PunRPC]
    public void AttackRPC()
    {
        // 오브젝트 풀링 방식 사용
        GameObject arrowObj = ObjectPoolingManager.instance.GetQueue("arrow");

        arrowObj.transform.position = arrowGenPosition.position;
        arrowObj.transform.rotation = arrowGenPosition.rotation;
        arrowObj.GetComponent<Arrow>().arrowDamage = GetComponent<LivingEntity>().meleeDamage;
        arrowObj.GetComponent<Arrow>().arrowQueueKey = "arrow";
        arrowObj.GetComponent<Rigidbody>().velocity = transform.forward * arrowSpeed;
    }
    
    public override bool Skill1()
    {
        if (!base.Skill1())
        {
            return false;
        }

        // 모든 클라이언트에서 스킬1에 대한 RPC를 실행한다.
        photonView.RPC("Skill1RPC", RpcTarget.All);

        // 스킬1의 쿨타임 코루틴을 실행한다.
        StartCoroutine("CoolTime", skill1Cooltime);

        return true;
    }

    [PunRPC]
    private void Skill1RPC()
    {
        // 스킬1이 동작하는 시간에 대한 코루틴을 실행한다.
        StartCoroutine("Skill1Duration", skill1Durationtime);
    }


    // 스킬1에 대한 오브젝트를 생성하고 발사한다.
    IEnumerator Skill1Duration(float duration)
    {
        // 오브젝트 풀링 방식 사용
        GameObject arrowSkill1Obj = ObjectPoolingManager.instance.GetQueue("arrowSkill1");
        arrowSkill1Obj.GetComponent<Collider>().enabled = false;

        arrowSkill1Obj.transform.position = arrowSkill1GenPosition.position;
        arrowSkill1Obj.transform.rotation = arrowSkill1GenPosition.rotation;
        arrowSkill1Obj.GetComponent<Arrow>().arrowDamage = GetComponent<LivingEntity>().meleeDamage * 2.0f;
        arrowSkill1Obj.GetComponent<Arrow>().arrowQueueKey = "arrowSkill1";

        yield return new WaitForSeconds(duration);

        arrowSkill1Obj.GetComponent<Rigidbody>().velocity = transform.forward * arrowSpeed;
        arrowSkill1Obj.GetComponent<Collider>().enabled = true;
    }
}
