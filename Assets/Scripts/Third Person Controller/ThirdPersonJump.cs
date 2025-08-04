using HoaxGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonJump : MonoBehaviour
{
    //flag
    //�Ƿ���Ծ
    public bool jump;
    //public bool lockPlannar;
    
    // ��Ծ��ز���
    [Header("��Ծ����")]
    public float jumpForce = 7f;           // ��Ծ����
    public float gravity = -9.8f;           // ����
    public float fallMultiplier = 0.1f;    // �������䱶��
    public float lowJumpMultiplier = 2f;   // �̰�������ʱ�����䱶��
   
    private Vector3 velocity;              // ��ֱ�ٶ�
    private bool isJumpAnimationPlaying = false; // ��Ծ�����Ƿ����ڲ���

    Animator animator;
    ThirdPersonMove thirdPersonMove;
    CharacterController characterController;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        thirdPersonMove = GetComponent<ThirdPersonMove>();
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isGrounded", characterController.isGrounded);
        //Debug.Log(characterController.isGrounded);
        // ������Ծ����
        HandleJumpInput();

    }

    void OnAnimatorMove()
    {
        // ˮƽ�ƶ��Զ�������ϵͳͬ������
        Vector3 horizontalVelocity = animator.velocity;
        horizontalVelocity.y = 0; // ���Զ�������ֱ�ٶ�
        characterController.SimpleMove(horizontalVelocity);

        // Ӧ������
        ApplyGravity();
        // ��ֱ�ƶ��ֶ�����
        ApplyVerticalMovement();
    }



    /// <summary>
    /// ������Ծ����
    /// </summary>
    void HandleJumpInput()
    {
        jump = Input.GetButtonDown("Jump");

        // �����Ծ�����Ƿ����ڲ���
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        isJumpAnimationPlaying = stateInfo.IsName("Jump") && stateInfo.normalizedTime < 1.0f;

        if (jump && characterController.isGrounded && !isJumpAnimationPlaying)
        {
            velocity.y = jumpForce;
            animator.SetBool("jump", true);
        }
        else
        {
            animator.SetBool("jump", false);
        }
    }

    /// <summary>
    /// Ӧ������
    /// </summary>
    void ApplyGravity()
    {
        if (velocity.y < 0)
        {
            // ����ʱ������
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // ����ʱ����ɿ���Ծ����Ӧ�ö�������
            velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            // ��������
            velocity.y += gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Ӧ����ֱ�ƶ�
    /// </summary>
    void ApplyVerticalMovement()
    {
        Vector3 verticalMovement = new Vector3(0, velocity.y, 0);
        characterController.Move(verticalMovement * Time.deltaTime);
    }

    /// <summary>
    /// ��������ؽ�ɫ����Ȩ
    /// </summary>
    public void OnJumpEnter()
    {
        Debug.Log("����");
        //lockPlannar = true;
    }
    
    public void OnJumpExit()
    {
        Debug.Log("���");
        //lockPlannar = false;
    }
    

    // ��ȡ��ǰ��ֱ�ٶ�
    public float GetVerticalVelocity => velocity.y;

}
