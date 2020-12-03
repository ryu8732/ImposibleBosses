using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 공격 이펙트에 대한 클래스
public class HitEffect : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<ParticleSystem>().Play();

        StartCoroutine(DestroyEffect());
    }

    // 1초 후 사라진다.
    IEnumerator DestroyEffect()
    {
        yield return new WaitForSeconds(1.0f);

        ObjectPoolingManager.instance.InsertQueue(gameObject, "hitEffect");
    }
}
