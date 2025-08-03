using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponClassManager : MonoBehaviour
{
    [Header("武器引用")]
    [SerializeField] private GameObject weapon;

    [Header("目标父级")]
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform rightHandPosition;

    [SerializeField] private Transform backpack;  // 后背
    [SerializeField] private Transform backpackPosition;  // 后背


    void OnWeaponSwitch(InputValue value)
    {
        if (value.isPressed)
        {
            if(weapon.transform.parent == backpack)
            {
                //从后背移到右手
                MoveWeaponToParent(rightHand,rightHandPosition);
            }
            else
            {
                //从右手移到后背
                MoveWeaponToParent(backpack,backpackPosition);
            }
        }

    }


    /// <summary>
    /// 将武器移动到指定父级
    /// </summary>
    /// <param name="newParent">新的父级Transform</param>
    public void MoveWeaponToParent(Transform newParent,Transform newPosition)
    {
        if (weapon == null || newParent == null)
        {
            Debug.LogError("武器或新父级为空！");
            return;
        }

        weapon.transform.parent = newParent;
        weapon.transform.position = newPosition.position;

    }




}
