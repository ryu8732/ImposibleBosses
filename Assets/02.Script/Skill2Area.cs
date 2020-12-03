using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill2Area : MonoBehaviour
{
    protected float skill2Damage = 30f;

    public float skill2Area = 2.0f;
    protected float skill2Speed = 2.0f;

    private float startScale;

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        GetComponent<SphereCollider>().enabled = false;
    }

    void Update()
    {
        // skill2Area만큼 2초간 커진다. (skill2Area는 KingMush의 mode 상태에 따라 달라진다.)
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(skill2Area, skill2Area, skill2Area), Time.deltaTime * 2.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 데미지를 주는 OnDamage 메소드는 호스트에서만 동작한다.
        if (other.transform.tag == "Player" && PhotonNetwork.IsMasterClient)
        {
            //IDamageable damageable = other.GetComponent<IDamageable>();
            //damageable.OnDamage(skill2Damage, Vector3.zero, Vector3.zero);

            // 데미지를 가하는 OnDamage 메소드는 마스터 클라이언트에서만 실행한다.
            LivingEntity livingEntity = other.GetComponent<LivingEntity>();
            livingEntity.photonView.RPC("OnDamage", RpcTarget.MasterClient, skill2Damage, Vector3.zero, Vector3.zero);
        }
    }
    public IEnumerator DestroySkill2()
    {
        yield return new WaitForEndOfFrame();
        ObjectPoolingManager.instance.InsertQueue(gameObject, "mushSkill2");
    }
}
