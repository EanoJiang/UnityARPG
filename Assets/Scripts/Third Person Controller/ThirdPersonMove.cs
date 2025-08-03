using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMove : MonoBehaviour
{
    //锁定目标
    public Transform LockTarget;

    [Header("输入参数")]
    public float dashSpeed = 1.5f;
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
        //速度控制
        animator.speed /= animator.humanScale;
    }


    // 目标旋转角度
    float targetRotation = 0.0f;
    // 平滑旋转时间
    public float RotationSmoothTime = 0.1f;
    // 平滑旋转速度
    float rotationVelocity;
    void Update()
    {
        if (!inputEnabled) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //animator.speed = moveSpeed * 1.5f / animator.humanScale;
            //只改变走路的速度，在混合树上乘这个SprintSpeed参数
            animator.SetFloat("SprintSpeed", dashSpeed / animator.humanScale);
        }
        else
        {
            animator.SetFloat("SprintSpeed", 1 / animator.humanScale);
        }
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
