using Photon.Pun;
using Photon.Pun.Demo.SlotRacer.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<ObjectPoolingManager>();
            }

            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    private static ObjectPoolingManager m_instance; // 싱글톤이 할당될 static 변수

    // 오브젝트 풀(큐)를 좀 더 효율적으로 관리하기 위한 딕셔너리
    Dictionary<string, Queue<GameObject>> objPoolDictionary = new Dictionary<string, Queue<GameObject>>();

    public GameObject bolt;
    public Queue<GameObject> boltQueue = new Queue<GameObject>();

    public GameObject boltLine;
    public Queue<GameObject> boltLineQueue = new Queue<GameObject>();

    public GameObject hitEffect;
    public Queue<GameObject> hitEffectQueue = new Queue<GameObject>();

    public GameObject hitText;
    public Queue<GameObject> hitTextQueue = new Queue<GameObject>();

    public GameObject mushSkill2;
    public Queue<GameObject> mushSkill2Queue = new Queue<GameObject>();

    public GameObject arrow;
    public Queue<GameObject> arrowQueue = new Queue<GameObject>();

    public GameObject arrowSkill1;
    public Queue<GameObject> arrowSkill1Queue = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        objPoolDictionary.Add("bolt", boltQueue);
        objPoolDictionary.Add("boltLine", boltLineQueue);
        objPoolDictionary.Add("hitEffect", hitEffectQueue);
        objPoolDictionary.Add("hitText", hitTextQueue);
        objPoolDictionary.Add("mushSkill2", mushSkill2Queue);
        objPoolDictionary.Add("arrow", arrowQueue);
        objPoolDictionary.Add("arrowSkill1", arrowSkill1Queue);

        for (int i = 0; i < 20; i++)
        {
            GameObject boltClone = Instantiate(bolt, Vector3.zero, Quaternion.identity);
            GameObject boltLineClone = Instantiate(boltLine, Vector3.zero, Quaternion.identity);
            GameObject hitTextClone = Instantiate(hitText, Vector3.zero, Quaternion.identity);
            GameObject hitEffectClone = Instantiate(hitEffect, Vector3.zero, Quaternion.identity);
            GameObject arrowClone = Instantiate(arrow, Vector3.zero, Quaternion.identity);
            GameObject arrowSkill1Clone = Instantiate(arrowSkill1, Vector3.zero, Quaternion.identity);

            //GameObject boltClone = PhotonNetwork.Instantiate("Bolt", Vector3.zero, Quaternion.identity);
            //GameObject boltLineClone = PhotonNetwork.Instantiate("BoltLine", Vector3.zero, Quaternion.identity);
            //GameObject hitTextClone = PhotonNetwork.Instantiate("Damage Text", Vector3.zero, Quaternion.identity);
            //GameObject hitEffectClone = PhotonNetwork.Instantiate("mushHitEffect", Vector3.zero, Quaternion.identity);

            boltClone.transform.SetParent(gameObject.transform);
            //boltClone.transform.parent = gameObject.transform;
            boltLineClone.transform.SetParent(gameObject.transform);
            //boltLineClone.transform.parent = gameObject.transform;
            hitTextClone.transform.SetParent(gameObject.transform);
            //hitTextClone.transform.parent = gameObject.transform;
            hitEffectClone.transform.SetParent(gameObject.transform);
            //hitEffectClone.transform.parent = gameObject.transform;
            arrowClone.transform.SetParent(gameObject.transform);
            //arrowClone.transform.parent = gameObject.transform;
            arrowSkill1Clone.transform.SetParent(gameObject.transform);

            boltQueue.Enqueue(boltClone);
            boltLineQueue.Enqueue(boltLineClone);
            hitTextQueue.Enqueue(hitTextClone);
            hitEffectQueue.Enqueue(hitEffectClone);
            arrowQueue.Enqueue(arrowClone);
            arrowSkill1Queue.Enqueue(arrowSkill1Clone);


            boltClone.SetActive(false);
            boltLineClone.SetActive(false);
            hitTextClone.SetActive(false);
            hitEffectClone.SetActive(false);
            arrowClone.SetActive(false);
            arrowSkill1Clone.SetActive(false);
        }

        GameObject mushSkill2Clone = Instantiate(mushSkill2, Vector3.zero, Quaternion.identity);
        mushSkill2Clone.transform.SetParent(gameObject.transform);
        mushSkill2Queue.Enqueue(mushSkill2Clone);
        mushSkill2Clone.SetActive(false);
    }

    // 함수를 호출할 때 키 값을 패러미터로 호출하여 모든 큐마다 함수를 구현할 필요 없이 키값을 통해 구분하도록 한다. (딕셔너리의 활용)
    public void InsertQueue(GameObject queueObj, string key)
    {
        objPoolDictionary[key].Enqueue(queueObj);
        queueObj.SetActive(false);
    }

    // 함수를 호출할 때 키 값을 패러미터로 호출하여 모든 큐마다 함수를 구현할 필요 없이 키값을 통해 구분하도록 한다. (딕셔너리의 활용)
    public GameObject GetQueue(string key)
    {
        GameObject queueObj = objPoolDictionary[key].Dequeue();
        queueObj.SetActive(true);
        return queueObj;
    }

}
