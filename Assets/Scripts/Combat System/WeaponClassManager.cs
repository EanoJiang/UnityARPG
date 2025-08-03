using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponClassManager : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private GameObject weapon;

    [Header("Ŀ�길��")]
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform rightHandPosition;

    [SerializeField] private Transform backpack;  // ��
    [SerializeField] private Transform backpackPosition;  // ��


    void OnWeaponSwitch(InputValue value)
    {
        if (value.isPressed)
        {
            if(weapon.transform.parent == backpack)
            {
                //�Ӻ��Ƶ�����
                MoveWeaponToParent(rightHand,rightHandPosition);
            }
            else
            {
                //�������Ƶ���
                MoveWeaponToParent(backpack,backpackPosition);
            }
        }

    }


    /// <summary>
    /// �������ƶ���ָ������
    /// </summary>
    /// <param name="newParent">�µĸ���Transform</param>
    public void MoveWeaponToParent(Transform newParent,Transform newPosition)
    {
        if (weapon == null || newParent == null)
        {
            Debug.LogError("�������¸���Ϊ�գ�");
            return;
        }

        weapon.transform.parent = newParent;
        weapon.transform.position = newPosition.position;

    }




}
