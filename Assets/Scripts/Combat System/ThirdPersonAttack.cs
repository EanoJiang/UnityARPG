using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonAttack : MonoBehaviour
{
    [Header("��������б�")]
    [SerializeField] private AnimationClip[] Attack1Clips;
    [Header("�ػ������б�")]
    [SerializeField] private AnimationClip[] Attack2Clips;


    Animator animator;
    ThirdPersonMove thirdPersonMove;
    CharacterController characterController;

    void Awake()
    {
        animator = GetComponent<Animator>();
        thirdPersonMove = GetComponent<ThirdPersonMove>();
        characterController = GetComponent<CharacterController>();
    }

    //����������洢ֵ���ᱻ�µ�����ֵ����
    int inputAttackType = 0;
    
    //���
    void OnFire1(InputValue value)
    {
        if (value.isPressed)
        {
            inputAttackType = 1;

        }
    }

    //�ػ�
    void OnFire2(InputValue value)
    {
        if (value.isPressed)
        {
            inputAttackType = 2;
        }
    }

    //������
    public int currentAttack = 0;
    //��ǰ�����Ķ���ʱ����ʱ��
    float animTimer = 0;
    /// <summary>
    /// �������������߼�
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputAttackType"></param>
    void PlayerAttack(int index,int inputAttackType)
    {
        //����ʱȡ������
        //thirdPersonMove.inputEnabled = false;
        AnimationClip animationclip;
        if (inputAttackType == 1)
        {
            //���
            //��������
            animationclip = Attack1Clips[index];
            //��������1
            currentAttack++;
        }
        else
        {
            //�ػ�
            //��������
            animationclip = Attack2Clips[index];
            //����������
            currentAttack = 0;
        }
        animator.CrossFade(animationclip.name, 0.2f);
        animTimer = animationclip.length;
    }

    void Update()
    {
        if (!characterController.isGrounded)
            return;
        //����ʱ����ʱ��֡�Լ�
        animTimer -= Time.deltaTime;
        if(animTimer <= 0)
        {
            //�����������
            //�ָ�����
            //thirdPersonMove.inputEnabled = true;
            //����������
            currentAttack = 0;
        }
        //Ԥ�����߼�
        if(inputAttackType != 0)
        {
            //����й���������
            //�ڵ�ǰ�����������Ž���ǰ0.4s��(���ҵ�ǰ��������<���������б�ĳ���)
            //���Ŷ�Ӧ�����Ĺ�������
            if(animTimer <= 0.4f && currentAttack < Attack1Clips.Length)
            {
                PlayerAttack(currentAttack, inputAttackType);
            }
            //����������洢ֵ����
            inputAttackType = 0;
        }

    }

    //�ⲿ���õ�����
    public int InputAttackType => inputAttackType;

}
