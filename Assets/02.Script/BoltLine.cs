using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltLine : MonoBehaviour
{
    TrailRenderer tr;
    public Vector3 EndPosition;

    private void OnEnable()
    {
        tr = GetComponent<TrailRenderer>();

        // 볼트 투사체의 라인을 사라지게(오브젝트 풀로 되돌아가게) 하는 코루틴 실행
        StartCoroutine(DestroyLine());
    }

    // Update is called once per frame
    void Update()
    {
        // 3.5초간 현재 위치에서 EndPosition을 향해 라인을 그려준다.
        transform.position = Vector3.Lerp(transform.position, EndPosition, Time.deltaTime * 3.5f);
    }

    IEnumerator DestroyLine()
    {
        yield return new WaitForSeconds(2.0f);
        ObjectPoolingManager.instance.InsertQueue(gameObject, "boltLine");
    }
}
