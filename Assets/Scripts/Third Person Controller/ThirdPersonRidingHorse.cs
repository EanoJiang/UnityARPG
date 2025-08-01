using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonRidingHorse : MonoBehaviour
{
    [Header("�������")]
    public GameObject horse;
    public bool isOnHorse;

    //void OnInteract()
    //{
    //    var thirdPersonMove = GetComponent<ThirdPersonMove>();
    //    thirdPersonMove.enabled = false;
    //    var pos = horse.transform.Find("Pos_UpToHorse");

    //}

    CharacterController characterController;
    Animator animator;
    ThirdPersonMove thirdPersonMove;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        thirdPersonMove = GetComponent<ThirdPersonMove>();
    }

    private void Update()
    {
        if (isOnHorse)
        {
            var axisX = Input.GetAxis("Horizontal");
            var axisY = Input.GetAxis("Vertical");
            animator.SetFloat("AxisX", axisX);
            animator.SetFloat("AxisY", axisY);
        }
        Ride();
    }

    void Ride()
    {
        //����
        if (!isOnHorse)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOnHorse = true;
                transform.rotation = horse.transform.rotation;
                transform.position = horse.transform.position;
                //����ɫ�ŵ�����
                var playerPoint = horse.transform.Find("PlayerPoint");
                transform.SetParent(playerPoint);
                transform.localPosition = Vector3.zero;
                //�����Ͻ��ý�ɫ��characterController��move
                characterController.enabled = false;
                thirdPersonMove.enabled = false;
                //�������������ƽű�
                horse.GetComponent<MalbersInput>().enabled = true;

                //�л����϶���״̬,��Ȩ�ش�0��1
                animator.SetLayerWeight(2, 1f);
            }
        }
        //����
        else
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOnHorse = false;
                if (horse != null)
                {
                    //ɾ����֮ǰ���ý�ɫλ��
                    transform.SetParent(null);
                    transform.position = horse.transform.position;
                    transform.rotation = horse.transform.rotation;
                    //�����ָ���ɫ��characterController��move
                    characterController.enabled = true;
                    thirdPersonMove.enabled = true;
                    //�ر����������ƽű�
                    horse.GetComponent<MalbersInput>().enabled = false;
                    //�ر����϶�����
                    animator.SetLayerWeight(2, 0f);

                }
            }
        }

    }
}
