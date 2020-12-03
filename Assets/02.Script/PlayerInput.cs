using UnityEngine;
using Photon.Pun;

// 플레이어 캐릭터를 조작하기 위한 사용자 입력을 감지
// 감지된 입력값을 다른 컴포넌트들이 사용할 수 있도록 제공
public class PlayerInput : MonoBehaviourPun
{
    public string moveVerticalName = "Vertical"; // 앞뒤 움직임을 위한 입력축 이름
    public string moveHorizontalName = "Horizontal"; // 앞뒤 움직임을 위한 입력축 이름
    public string fireButtonName = "Fire1"; // 발사를 위한 입력 버튼 이름

    // 값 할당은 내부에서만 가능
    public float moveVertical { get; private set; } // 감지된 움직임 입력값
    public float moveHorizontal { get; private set; }

    // 매프레임 사용자 입력을 감지
    private void Update()
    {
        // 로컬 플레이어가 아닌 경우 입력 받지 않음
        if(!photonView.IsMine)
        {
            return;
        }

        // 게임오버 상태에서는 사용자 입력을 감지하지 않는다
        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            moveVertical = 0;
            moveHorizontal = 0;
            return;
        }


        // move에 관한 입력 감지
        moveVertical = Input.GetAxis(moveVerticalName);
        moveVertical = Input.GetAxis(moveHorizontalName);

    }
}