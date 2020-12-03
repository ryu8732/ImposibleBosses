using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    // 싱글톤 접근용 프로퍼티
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }

    public GameObject player;
    private PlayerMovement playerMovement;
    private PlayerStatement playerStatement;
    private PlayerAttack playerAttack;

    public GameObject targetObj;
    private LivingEntity target;
    public Slider targetHealthSlider;
    public Slider targetManaSlider;

    public GameObject gameOverPanel;

    public Image CoolImage;

    private static UIManager m_instance; // 싱글톤이 할당될 변수

    public GameObject Buttons;

    public void Start()
    {
        SetupPlayer();

        // 타겟 정보 UI를 위해 타겟을 설정한다. (룸에서는 타겟이 없으므로 동작하지않음)
        if (SceneManager.GetActiveScene().name != "Room")
        {
            target = targetObj.GetComponent<LivingEntity>();
        }
    }

    public void SetupPlayer()
    {
        player = FindObjectOfType<PlayerMovement>().gameObject;

        playerMovement = player.GetComponent<PlayerMovement>();
        playerAttack = player.GetComponent<PlayerAttack>();
        playerStatement = player.GetComponent<PlayerStatement>();
    }

    // 타겟의 체력에 대한 UI 업데이트를 위한 메소드
    public void UpdateTargetHealth()
    {
        targetHealthSlider.maxValue = target.startingHealth;
        targetHealthSlider.value = target.health;

        if(targetHealthSlider.value <= 0)
        {
            targetHealthSlider.transform.Find("Fill Area").gameObject.SetActive(false);
        }
    }


    // 타겟의 마나에 대한 UI 업데이트를 위한 메소드
    public void UpdateTargetMana()
    {
        targetManaSlider.value = target.mana;
    }

    // 점프 버튼이 클릭 된 경우
    public void OnClickJumpButtonDown()
    {

        if (!GameManager.instance.isGameover)
        {
            playerMovement.Jump();
        }
    }

    public void OnClickJumpButtonUp()
    {
        if (!GameManager.instance.isGameover) {
            //Debug.Log("Jump Up");
        }
    }

    // 공격 버튼이 클릭 된 경우
    public void OnClickAtkButtonDown()
    {
        if (!GameManager.instance.isGameover)
        {
            playerAttack.Attack();
        }
    }
    public void OnClickAtkButtonUp()
    {
        if (!GameManager.instance.isGameover)
        {
            //Debug.Log("AtkUp");
        }
    }

    // 스킬1 버튼이 클릭 된 경우
    public void OnClickSkill1ButtonDown()
    {
        if (!GameManager.instance.isGameover)
        {
            playerAttack.Skill1();
        }
    }

    // 
    public void OnClickSkill1ButtonUp()
    {
        if (!GameManager.instance.isGameover)
        {
            //Debug.Log("AtkUp");
        }
    }

    // 게임오버 패널을 활성화 하기 위한 메소드
    public void SetActiveGameoverUI()
    {
        gameOverPanel.SetActive(true);
    }

    // 게임오버 패널에서 OK버튼을 클릭 한 경우
    public void OnGameOverOkClicked()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Lobby");
    }

    // 캐릭터가 변경 될 때 마다 버튼 설정을 리셋
    public void ResetButtons()
    {
        CoolImage.fillAmount = 0f;
    }
}
