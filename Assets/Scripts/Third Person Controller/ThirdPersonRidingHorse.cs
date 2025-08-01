using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonRidingHorse : MonoBehaviour
{
    [Header("骑马参数")]
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
        //上马
        if (!isOnHorse)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOnHorse = true;
                transform.rotation = horse.transform.rotation;
                transform.position = horse.transform.position;
                //将角色放到马上
                var playerPoint = horse.transform.Find("PlayerPoint");
                transform.SetParent(playerPoint);
                transform.localPosition = Vector3.zero;
                //在马上禁用角色的characterController和move
                characterController.enabled = false;
                thirdPersonMove.enabled = false;
                //开启马的输入控制脚本
                horse.GetComponent<MalbersInput>().enabled = true;

                //切换马上动作状态,即权重从0到1
                animator.SetLayerWeight(2, 1f);
            }
        }
        //下马
        else
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOnHorse = false;
                if (horse != null)
                {
                    //删除马之前设置角色位置
                    transform.SetParent(null);
                    transform.position = horse.transform.position;
                    transform.rotation = horse.transform.rotation;
                    //下马后恢复角色的characterController和move
                    characterController.enabled = true;
                    thirdPersonMove.enabled = true;
                    //关闭马的输入控制脚本
                    horse.GetComponent<MalbersInput>().enabled = false;
                    //关闭马上动作层
                    animator.SetLayerWeight(2, 0f);

                }
            }
        }

    }
}
