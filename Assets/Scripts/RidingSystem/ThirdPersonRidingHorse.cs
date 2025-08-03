using MalbersAnimations;
using MalbersAnimations.HAP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonRidingHorse : MonoBehaviour
{
    [Header("�������")]
    public GameObject horse;
    public bool isOnHorse = false;

    [Header("��������")]
    public int mountLayerIndex = 3;
    public int ridingLayerIndex = 2;
    public int dismountLayerIndex = 4; // ��������

    CharacterController characterController;
    Animator animator;
    ThirdPersonMove thirdPersonMove;
    
    // ����״̬����
    private bool isMounting = false;
    private bool isDismounting = false;
    private MountTriggers currentMountTrigger;
    private Coroutine mountCoroutine;
    private Coroutine dismountCoroutine;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        thirdPersonMove = GetComponent<ThirdPersonMove>();
    }

    private void Update()
    {
        HandleInput();
        
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
        if (isOnHorse && !isDismounting)
        {
            Update_MountPlayerPosition();
        }
        //����
        else if (!isOnHorse && !isMounting)
        {
            // �����߼��Ƶ�HandleInput�д���
        }
    }

    /// <summary>
    /// ��������ʱ��λ�ó���
    /// </summary>
    private void Update_MountPlayerPosition()
    {
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
        animator.SetLayerWeight(ridingLayerIndex, 1f);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        //����������봥����
        if(other.gameObject.tag == "MountTrigger" && !isOnHorse && !isMounting)
        {
            // ��ȡMountTriggers���
            MountTriggers mountTrigger = other.GetComponent<MountTriggers>();
            if (mountTrigger != null)
            {
                currentMountTrigger = mountTrigger;
                Debug.Log("�������������򣬰�F������");
            }
        }
    }

    /// <summary>
    /// �뿪��������
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "MountTrigger" && !isOnHorse && !isMounting)
        {
            currentMountTrigger = null;
            Debug.Log("�뿪����������");
        }
    }

    /// <summary>
    /// ��ʼ��������
    /// </summary>
    private void StartMounting()
    {
        if (currentMountTrigger == null || isMounting || isOnHorse) return;

        isMounting = true;
        
        // ��ȡ��ҵ�Animator���
        Animator playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // ����Mount LayerȨ��Ϊ1
            playerAnimator.SetLayerWeight(mountLayerIndex, 1f);
            
            // ����������
            playerAnimator.Play(currentMountTrigger.MountAnimation, mountLayerIndex);
            
            // ��ʼЭ�̼��������������
            mountCoroutine = StartCoroutine(WaitForMountAnimationComplete());
        }
    }

    /// <summary>
    /// �ȴ��������������
    /// </summary>
    private IEnumerator WaitForMountAnimationComplete()
    {
        Animator playerAnimator = GetComponent<Animator>();
        
        // �ȴ������������
        yield return new WaitForSeconds(0.1f); // �ȴ�������ʼ
        
        // ��ȡ����״̬��Ϣ
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(mountLayerIndex);
        
        // �ȴ��������ŵ�50%���������
        while (stateInfo.normalizedTime < 0.5f)
        {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(mountLayerIndex);
        }
        
        // �������ŵ�50%��ִ����������߼�
        CompleteMounting();
    }

    /// <summary>
    /// �������
    /// </summary>
    private void CompleteMounting()
    {
        // ����ֹͣMount Layer����������Ȩ��Ϊ0
        animator.SetLayerWeight(mountLayerIndex, 0f);
        animator.Play("Empty", mountLayerIndex); // ���ſն�����ֹͣ��ǰ����
        
        // ��������״̬
        isOnHorse = true;
        isMounting = false;
        
        // �ƶ���PlayerPointλ��
        var playerPoint = horse.transform.Find("PlayerPoint");
        if (playerPoint != null)
        {
            transform.SetParent(playerPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        // ���ý�ɫ���������ƶ��ű�
        characterController.enabled = false;
        thirdPersonMove.enabled = false;
        
        // �������������ƽű�
        horse.GetComponent<MalbersInput>().enabled = true;
        
        // ��������RidingHorse������Ȩ��Ϊ1�����ɵ�����״̬
        animator.SetLayerWeight(ridingLayerIndex, 1f);
        
        currentMountTrigger = null;
        mountCoroutine = null;
        
        Debug.Log("�������");
    }

    /// <summary>
    /// ��ʼ��������
    /// </summary>
    private void StartDismounting()
    {
        if (!isOnHorse || isDismounting) return;

        isDismounting = true;
        
        // ��ȡ��ҵ�Animator���
        Animator playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // ����Dismount LayerȨ��Ϊ1
            playerAnimator.SetLayerWeight(dismountLayerIndex, 1f);
            
            // ���������������Ը�����Ҫ���ò�ͬ����������
            playerAnimator.Play("Rider_Dismount_Right", dismountLayerIndex);
            
            // ��ʼЭ�̼��������������
            dismountCoroutine = StartCoroutine(WaitForDismountAnimationComplete());
        }
    }

    /// <summary>
    /// �ȴ��������������
    /// </summary>
    private IEnumerator WaitForDismountAnimationComplete()
    {
        Animator playerAnimator = GetComponent<Animator>();
        
        // �ȴ������������
        yield return new WaitForSeconds(0.1f); // �ȴ�������ʼ
        
        // ��ȡ����״̬��Ϣ
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(dismountLayerIndex);
        
        // �ȴ��������ŵ�50%���������
        while (stateInfo.normalizedTime < 0.5f)
        {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(dismountLayerIndex);
        }
        
        // �������ŵ�50%��ִ����������߼�
        CompleteDismounting();
    }

    /// <summary>
    /// �������
    /// </summary>
    private void CompleteDismounting()
    {
        // ����ֹͣDismount Layer����������Ȩ��Ϊ0
        animator.SetLayerWeight(dismountLayerIndex, 0f);
        animator.Play("Empty", dismountLayerIndex); // ���ſն�����ֹͣ��ǰ����
        
        // ��������״̬
        isOnHorse = false;
        isDismounting = false;
        
        if (horse != null)
        {
            // ���ý�ɫλ�ã�������������
            transform.SetParent(null);
            
            // ����������λ�ã����Ը�����Ҫ������
            Vector3 dismountPosition = horse.transform.position + horse.transform.forward * 2f;
            transform.position = dismountPosition;
            transform.rotation = horse.transform.rotation;
            
            // �����ָ���ɫ��characterController��move
            characterController.enabled = true;
            thirdPersonMove.enabled = true;
            
            // �ر����������ƽű�
            horse.GetComponent<MalbersInput>().enabled = false;
            
            // �����ر����϶����㣬���ɵ�����״̬
            animator.SetLayerWeight(ridingLayerIndex, 0f);
        }
        
        dismountCoroutine = null;
        
        Debug.Log("�������");
    }

    /// <summary>
    /// ��������
    /// </summary>
    private void HandleInput()
    {
        // �ڴ������ڰ�F����ʼ����
        if (currentMountTrigger != null && Input.GetKeyDown(KeyCode.F) && !isMounting && !isOnHorse)
        {
            StartMounting();
        }
        
        // �����ϰ�F����ʼ����
        if (isOnHorse && Input.GetKeyDown(KeyCode.F) && !isDismounting)
        {
            StartDismounting();
        }
    }
}
