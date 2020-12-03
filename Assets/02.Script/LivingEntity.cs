using System;
using UnityEngine;
using Photon.Pun;

// 생명체로서 동작할 게임 오브젝트들을 위한 뼈대를 제공
// 체력, 데미지 받아들이기, 사망 기능, 사망 이벤트를 제공
public abstract class LivingEntity : MonoBehaviourPun, IDamageable

{
    public virtual float startingHealth { get; protected set; } // 시작 체력
    public virtual float health { get; protected set; } // 현재 체력
    public virtual float mana { get; protected set; }  // 현재 마나
    public virtual float manaRecoverySpeed { get; protected set; }  // 마나 회복 속도
    public virtual float meleeDamage { get; protected set; }
    public virtual bool dead { get; protected set; } // 사망 상태
    //public virtual event Action onDeath; // 사망시 발동할 이벤트

    // 생명체가 활성화될때 상태를 리셋
    protected virtual void OnEnable()
    {
        // 사망하지 않은 상태로 시작
        dead = false;

        // 체력을 시작 체력으로 초기화
        health = startingHealth;
    }

    // 캐릭터의 기본 스텟을 설정할 때 사용 될 메소드
    [PunRPC]
    public virtual void Setup(float newHealth, float newDamage, float newManaRecoverySpeed)
    {
        startingHealth = newHealth;
        health = startingHealth;

        meleeDamage = newDamage;

        mana = 0f;
        manaRecoverySpeed = newManaRecoverySpeed;
    }

    // 호스트에서 미리 적용된 health를 다른 클라이언트에게도 적용하기 위한 RPC
    [PunRPC]
    public void ApplyUpdateHealth(float newHealth, bool newDead)
    {
        health = newHealth;
        dead = newDead;
    }

    [PunRPC]
    // 데미지를 입는 기능
    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 호스트에서 먼저 단독 실행
        if (PhotonNetwork.IsMasterClient)
        {
            // 데미지만큼 체력 감소
            health -= damage;

            // 호스트에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdateHealth", RpcTarget.Others, health, dead);

            // 다른 클라이언트에서도 OnDamage 실행
            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPoint, hitNormal);
        }

        // 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    // 체력을 회복하는 기능
    public virtual void RestoreHealth(float newHealth)
    {
        if (dead)
        {
            // 이미 사망한 경우 체력을 회복할 수 없음
            return;
        }

        // 체력 추가
        health += newHealth;
    }

    // 사망 처리
    public virtual void Die()
    {
        // onDeath 이벤트에 등록된 메서드가 있다면 실행
        //if (onDeath != null)
        //{
        //    onDeath();
        //}

        // 사망 상태를 참으로 변경
        dead = true;
    }
}