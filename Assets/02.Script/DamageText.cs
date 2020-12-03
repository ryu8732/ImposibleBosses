using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    TextMesh damageTextMesh;
    Color startColor;
    Color alpha;

    private void OnEnable()
    {
        damageTextMesh = GetComponent<TextMesh>();
        startColor = damageTextMesh.color;

        alpha = damageTextMesh.color;

        StartCoroutine(DestroyText());
    }


    private void Update()
    {
        // 텍스트는 점점 올라가는 동작을 한다.
        transform.Translate(Vector3.up * Time.deltaTime * 2.0f);
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * 2.0f);

        damageTextMesh.color = alpha;
    }

    public void DisplayDamage(float damage)
    {
        GetComponent<TextMesh>().text = "-" + damage;
    }

    IEnumerator DestroyText()
    {
        yield return new WaitForSeconds(1.0f);

        // 비활성화 직전에 처음 색깔로 초기화한다.
        damageTextMesh.color = startColor;
        ObjectPoolingManager.instance.InsertQueue(gameObject, "hitText");
    }
}
