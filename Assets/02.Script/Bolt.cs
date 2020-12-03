using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bolt : MonoBehaviourPun
{
    public float boltDamage = 10f;
    public float boltSpeed = 3f;

    private void OnEnable()
    {
        // 볼트가 발사 된 뒤 2초후 사라지는(오브젝트풀로 되돌아가는) 코루틴 실행
        StartCoroutine(DestroyBolt(2.0f));
    }

    IEnumerator DestroyBolt(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolingManager.instance.InsertQueue(gameObject, "bolt");
    }

    // 볼트 투사체가 플레이어에 닿을 경우 해당 플레이어에데 데미지를 입힌다.
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Player")
        {
            // 데미지를 가하는 OnDamage 메소드는 마스터 클라이언트에서만 실행한다.
            if (PhotonNetwork.IsMasterClient)
            {

                //IDamageable damageable = other.GetComponent<IDamageable>();
                //damageable.OnDamage(boltDamage, Vector3.zero, Vector3.zero);

                // 데미지를 가하는 OnDamage 메소드는 마스터 클라이언트에서만 실행한다.
                LivingEntity livingEntity = other.GetComponent<LivingEntity>();
                livingEntity.photonView.RPC("OnDamage", RpcTarget.MasterClient, boltDamage, Vector3.zero, Vector3.zero);
            }

            // 모든 클라이언트들이 볼트를 생성했기 떄문에, 볼트가 충돌 시 제거하는 효과는 모든 클라이언트에서 수행되어야한다.
            // 마스터 클라이언트에서 RPC를 모든 클라이언트에서 호출하여 제거하는 방법도 가능 -> 대신 Bolt를 생성할 때, Photon.Instatiate를 사용해야하며, 모든 볼트가 PhotonView 컴포넌트를 가지고있어야 하므로 부하가 심해질것이라고 생각하여 이 방식을 채택함.
            StartCoroutine(DestroyBolt(0.0f));
        }
    }
}
