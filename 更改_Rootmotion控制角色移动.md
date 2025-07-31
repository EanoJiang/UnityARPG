把原先的PlayerController脚本替换为这个

ThirdPersonMove

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMove : MonoBehaviour
{
    //锁定目标
    public Transform LockTarget;

    //输入控制
    public bool inputEnabled = true;


    GameObject mainCamera;
    Animator animator;
    CharacterController characterController;

    void Start()
    {
        //获取当前场景的主相机
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }


    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }


    // 目标旋转角度
    float targetRotation = 0.0f;
    // 平滑旋转时间
    public float RotationSmoothTime = 0.1f;
    // 平滑旋转速度
    float rotationVelocity;
    void Update()
    {
        if (!inputEnabled)
            return;

        #region 水平方向
        if (LockTarget == null)
            FreeMove();
        else
            LockMove();
        #endregion

    }



    /// <summary>
    /// 新版Input输入系统————移动
    /// </summary>
    Vector2 moveAmount;
    void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();

    }

    /// <summary>
    /// 自由移动，方向
    /// </summary>
    private void FreeMove()
    {
        if (moveAmount != Vector2.zero)
        {
            // 计算水平方向输入方向，将其转换为世界空间坐标系——x和z是水平坐标轴
            Vector3 inputDir = new Vector3(moveAmount.x, 0.0f, moveAmount.y).normalized;
            // 计算目标朝向
            targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                                  mainCamera.transform.eulerAngles.y;
            // 平稳地旋转玩家，使其面向他们移动的方向。
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);
            // 旋转玩家
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        //Y轴输入值——只影响前进的速度控制，朝向由上面的if中的语句决定
        var axisY = animator.GetFloat("AxisY");
        //平滑过渡到Y轴目标值
        axisY = Mathf.MoveTowards(axisY, moveAmount.magnitude, Time.deltaTime * 5f);
        animator.SetFloat("AxisY", axisY);
    }

    /// <summary>
    /// 锁定目标时的移动——八向移动
    /// </summary>
    private void LockMove()
    {
        //角色位置到目标的向量
        var dir = LockTarget.position - transform.position;
        //去除y轴的影响
        dir.y = 0;
        //目标朝向
        var rotation = Quaternion.LookRotation(dir);
        //从 当前朝向到目标朝向 平滑旋转
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationSmoothTime);
        //获取当前轴向数值
        var axisX = animator.GetFloat("AxisX");
        var axisY = animator.GetFloat("AxisY");
        //平滑改变轴向
        axisX = Mathf.MoveTowards(axisX, moveAmount.x, Time.deltaTime * 5f);
        axisY = Mathf.MoveTowards(axisY, moveAmount.y, Time.deltaTime * 5f);
        //更新动画参数
        animator.SetFloat("AxisX", axisX);
        animator.SetFloat("AxisY", axisY);
    }

}

```

发现这时候如果加入跳跃动画，只会播放动画，characterController的胶囊体并不会跟着上下移动

OnAnimatorMove()方法用来脚本控制RootMotion

并使用characterController.SimpleMove(animator.velocity);来自动加入重力，不然单纯使用.Move(animator.deltaPosition)会失去重力

PlayerJumpController

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpController : MonoBehaviour
{
    //flag
    //是否跳跃
    public bool jump;
    public bool lockPlannar;

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
    void OnAnimatorMove()
    {
        characterController.SimpleMove(animator.velocity);
        #region 竖直方向——Jump
        jump = Input.GetButton("Jump");
        animator.SetBool("jump", jump);
        jump = false;
        #endregion
    }

    /// <summary>
    /// 起跳和落地角色控制权
    /// </summary>
    public void OnJumpEnter()
    {
        Debug.Log("起跳");
        thirdPersonMove.inputEnabled = false;
        lockPlannar = true;
    }
    public void OnJumpExit()
    {
        Debug.Log("落地");
        thirdPersonMove.inputEnabled = true;
        lockPlannar = false;
    }
}

```

## 修复bug——跳跃用代码控制

ThirdPersonJump

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonJump : MonoBehaviour
{
    //flag
    //是否跳跃
    public bool jump;
    //public bool lockPlannar;
  
    // 跳跃相关参数
    [Header("跳跃参数")]
    public float jumpForce = 7f;           // 跳跃力度
    public float gravity = -9.8f;           // 重力
    public float fallMultiplier = 2.5f;    // 正常下落倍率
    public float lowJumpMultiplier = 2f;   // 短按起跳键时的下落倍率
   
    private Vector3 velocity;              // 竖直速度

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
        // 处理跳跃输入
        HandleJumpInput();

        // 应用重力
        ApplyGravity();
  
        // 竖直移动手动控制
        ApplyVerticalMovement();
    }

    void OnAnimatorMove()
    {
        // 水平移动自动和物理系统同步更新
        Vector3 horizontalVelocity = animator.velocity;
        horizontalVelocity.y = 0; // 忽略动画的竖直速度
        characterController.SimpleMove(horizontalVelocity);
    }



    /// <summary>
    /// 处理跳跃输入
    /// </summary>
    void HandleJumpInput()
    {
        jump = Input.GetButtonDown("Jump");

        if (jump)
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
    /// 应用重力
    /// </summary>
    void ApplyGravity()
    {
        if (velocity.y < 0)
        {
            // 下落时应用更大的重力
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // 上升时如果松开跳跃键，应用短跳重力
            velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            // 正常重力
            velocity.y += gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 应用竖直移动
    /// </summary>
    void ApplyVerticalMovement()
    {
        Vector3 verticalMovement = new Vector3(0, velocity.y, 0);
        characterController.Move(verticalMovement * Time.deltaTime);
    }

    /// <summary>
    /// 起跳和落地角色控制权
    /// </summary>
    public void OnJumpEnter()
    {
        Debug.Log("起跳");
        //lockPlannar = true;
    }
  
    public void OnJumpExit()
    {
        Debug.Log("落地");
        //lockPlannar = false;
    }
  

    // 获取当前竖直速度
    public float GetVerticalVelocity => velocity.y;

}

```

## 修复BUG——锁定目标时角色朝向问题

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMove : MonoBehaviour
{
    //锁定目标
    public Transform LockTarget;

    //输入控制
    public bool inputEnabled = true;
  
    // 锁定转向参数
    [Header("锁定转向参数")]
    public float lockRotationSpeed = 10f;      // 锁定时的旋转速度
    public float minLockDistance = 1f;         // 最小锁定距离
    public float maxLockDistance = 20f;        // 最大锁定距离

    GameObject mainCamera;
    Animator animator;
    CharacterController characterController;

    void Start()
    {
        //获取当前场景的主相机
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }


    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }


    // 目标旋转角度
    float targetRotation = 0.0f;
    // 平滑旋转时间
    public float RotationSmoothTime = 0.1f;
    // 平滑旋转速度
    float rotationVelocity;
    void Update()
    {

        #region 水平方向
        if (LockTarget == null)
            FreeMove();
        else
            LockMove();
        #endregion

    }



    /// <summary>
    /// 新版Input输入系统————移动
    /// </summary>
    Vector2 moveAmount;
    void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();

    }

    /// <summary>
    /// 自由移动，方向
    /// </summary>
    private void FreeMove()
    {
        if (moveAmount != Vector2.zero)
        {
            // 计算水平方向输入方向，将其转换为世界空间坐标系——x和z是水平坐标轴
            Vector3 inputDir = new Vector3(moveAmount.x, 0.0f, moveAmount.y).normalized;
            // 计算目标朝向
            targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                                  mainCamera.transform.eulerAngles.y;
            // 平稳地旋转玩家，使其面向他们移动的方向。
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);
            // 旋转玩家
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        //Y轴输入值——只影响前进的速度控制，朝向由上面的if中的语句决定
        var axisY = animator.GetFloat("AxisY");
        //平滑过渡到Y轴目标值
        axisY = Mathf.MoveTowards(axisY, moveAmount.magnitude, Time.deltaTime * 5f);
        animator.SetFloat("AxisY", axisY);
    }

    /// <summary>
    /// 锁定目标时的移动——八向移动
    /// </summary>
    private void LockMove()
    {
        if (LockTarget == null) return;
  
        //角色位置到目标的向量
        var dir = LockTarget.position - transform.position;
        float distance = dir.magnitude;
  
        // 检查距离是否在有效范围内
        if (distance < minLockDistance || distance > maxLockDistance)
        {
            // 距离太近或太远时，不进行锁定转向
            return;
        }
  
        //去除y轴的影响
        dir.y = 0;
  
        // 确保方向向量不为零
        if (dir.magnitude > 0.1f)
        {
            //目标朝向
            var targetRotation = Quaternion.LookRotation(dir);
            //从 当前朝向到目标朝向 平滑旋转
            // 使用lockRotationSpeed作为旋转速度
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockRotationSpeed * Time.deltaTime);
        }
  
        //获取当前轴向数值
        var axisX = animator.GetFloat("AxisX");
        var axisY = animator.GetFloat("AxisY");
        //平滑改变轴向
        axisX = Mathf.MoveTowards(axisX, moveAmount.x, Time.deltaTime * 5f);
        axisY = Mathf.MoveTowards(axisY, moveAmount.y, Time.deltaTime * 5f);
        //更新动画参数
        animator.SetFloat("AxisX", axisX);
        animator.SetFloat("AxisY", axisY);
    }

    /// <summary>
    /// 调试绘制 - 在Scene视图中显示锁定信息
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (LockTarget != null)
        {
            // 绘制到目标的连线
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, LockTarget.position);
      
            // 绘制锁定距离范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minLockDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxLockDistance);
      
            // 绘制角色朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}

```

## 从原项目移植攻击系统代码——ThirdPersonAttack

ThirdPersonAttack

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonAttack : MonoBehaviour
{
    [Header("轻击动画列表")]
    [SerializeField] private AnimationClip[] Attack1Clips;
    [Header("重击动画列表")]
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

    //攻击类输入存储值，会被新的输入值覆盖
    int inputAttackType = 0;
  
    //轻击
    void OnFire1(InputValue value)
    {
        if (value.isPressed)
        {
            inputAttackType = 1;

        }
    }

    //重击
    void OnFire2(InputValue value)
    {
        if (value.isPressed)
        {
            inputAttackType = 2;
        }
    }

    //连招数
    public int currentAttack = 0;
    //当前攻击的动画时长计时器
    float animTimer = 0;
    /// <summary>
    /// 攻击动画加载逻辑
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputAttackType"></param>
    void PlayerAttack(int index,int inputAttackType)
    {
        //攻击时取消输入
        //thirdPersonMove.inputEnabled = false;
        AnimationClip animationclip;
        if (inputAttackType == 1)
        {
            //轻击
            //动画载入
            animationclip = Attack1Clips[index];
            //连招数加1
            currentAttack++;
        }
        else
        {
            //重击
            //动画载入
            animationclip = Attack2Clips[index];
            //连招数归零
            currentAttack = 0;
        }
        animator.CrossFade(animationclip.name, 0.2f);
        animTimer = animationclip.length;
    }

    void Update()
    {
        if (!characterController.isGrounded)
            return;
        //动画时长计时器帧自减
        animTimer -= Time.deltaTime;
        if(animTimer <= 0)
        {
            //动画播放完毕
            //恢复输入
            //thirdPersonMove.inputEnabled = true;
            //连招数归零
            currentAttack = 0;
        }
        //预输入逻辑
        if(inputAttackType != 0)
        {
            //如果有攻击键输入
            //在当前攻击动画播放结束前0.4s，(并且当前的连招数<攻击动画列表的长度)
            //播放对应索引的攻击动画
            if(animTimer <= 0.4f && currentAttack < Attack1Clips.Length)
            {
                PlayerAttack(currentAttack, inputAttackType);
            }
            //攻击类输入存储值归零
            inputAttackType = 0;
        }

    }

    //外部调用的属性
    public int InputAttackType => inputAttackType;

}

```




> 代办：

连招动画计数器有点问题，每次重新启用脚本又好了，应该是某个变量没有归零？
