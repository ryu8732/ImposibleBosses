using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviourPun
{
    public float arrowDamage;
    public string arrowQueueKey;

    private void OnEnable()
    {
        // 화살이 발사 된 뒤 5초후 사라지는(오브젝트풀로 되돌아가는) 코루틴 실행
        StartCoroutine(DestroyArrow(5.0f));
    }

    IEnumerator DestroyArrow(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolingManager.instance.InsertQueue(gameObject, arrowQueueKey);
    }

    // 화살 투사체가 플레이어에 닿을 경우 해당 적에게 데미지를 입힌다.
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            // 데미지를 가하는 OnDamage RPC메소드를 호출하는 동작은 마스터 클라이언트에서만 동작하도록 한다.
            if (PhotonNetwork.IsMasterClient)
            {
                LivingEntity livingEntity = other.GetComponent<LivingEntity>();
                livingEntity.photonView.RPC("OnDamage", RpcTarget.MasterClient, arrowDamage, other.ClosestPointOnBounds(transform.position), Vector3.zero);
            }

            // 화살 제거
            StartCoroutine(DestroyArrow(0.0f));
        }
    }
}
