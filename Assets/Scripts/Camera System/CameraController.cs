using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //摄像机跟随的目标
    [SerializeField] Transform followTarget;
    [SerializeField] float rotationSpeed = 1.5f;
    //距离
    [SerializeField] float distance;

    //绕y轴的旋转角度——水平视角旋转
    float rotationY;
    //绕x轴的旋转角度——垂直视角旋转
    float rotationX;
    //限制rotationX幅度
    [SerializeField] float minVerticalAngle = -20;
    [SerializeField] float maxVerticalAngle = 45;
    //框架偏移向量——摄像机位置视差偏移
    [SerializeField] Vector2 frameOffset;

    //视角控制反转
    [Header("视角控制反转:invertX是否反转垂直视角,invertY是否反转水平视角")]
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float invertXValue;
    float invertYValue;

    // 帧同步相关参数
    [Header("帧同步设置")]
    [SerializeField] float smoothSpeed = 10f; // 平滑插值速度
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isInitialized = false;

    private void Start()
    {
        //隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 初始化目标位置和旋转
        if (followTarget != null)
        {
            InitializeCamera();
        }
    }

    private void InitializeCamera()
    {
        // 计算初始旋转
        var initialRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(frameOffset.x, frameOffset.y, 0);
        
        // 设置初始位置和旋转
        transform.position = focusPosition - initialRotation * new Vector3(0, 0, distance);
        transform.rotation = initialRotation;
        
        // 设置目标位置和旋转
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        
        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (followTarget == null || !isInitialized) return;

        //视角控制反转参数
        invertXValue = (invertX) ? -1 : 1;
        invertYValue = (invertY) ? -1 : 1;

        //水平视角控制——鼠标(手柄)x轴控制rotationY
        rotationY += Input.GetAxis("Camera X") * rotationSpeed * invertYValue * Time.fixedDeltaTime;
        //垂直视角控制——鼠标(手柄)y轴控制rotationX
        rotationX += Input.GetAxis("Camera Y") * rotationSpeed * invertXValue * Time.fixedDeltaTime;
        //限制rotationX幅度
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        //视角旋转参量
        //想要水平旋转视角，所以需要的参量为绕y轴旋转角度
        targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //摄像机的焦点位置
        var focusPosition = followTarget.position + new Vector3(frameOffset.x, frameOffset.y, 0);
        
        //摄像机放在目标后面指定距离的位置
        targetPosition = focusPosition - targetRotation * new Vector3(0, 0, distance);

        // 使用平滑插值更新相机位置和旋转
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }



    //水平方向的旋转，返回摄像机的水平旋转四元数。
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

    /// <summary>
    /// 设置相机距离
    /// </summary>
    public void SetDistance(float newDistance)
    {
        distance = newDistance;
    }

    // 获取当前相机距离
    public float GetDistance => distance;

    /// <summary>
    /// 设置平滑速度
    /// </summary>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = speed;
    }
}
