# 集成骑乘系统 (Integrated Riding System)

## 概述

集成骑乘系统是一个将 `ThirdPersonRidingHorse.cs` 的上下马逻辑与 `IRidingSystem` 接口和 `RidingManager` 类集成的完整解决方案。该系统提供了统一的骑乘管理接口，支持动画触发、状态管理和事件系统。

## 文件结构

```
Assets/Scripts/RidingSystem/
├── IRidingSystem.cs              # 骑乘系统接口定义
├── RidingManager.cs              # 骑乘管理器实现
├── IntegratedRidingSystem.cs     # 集成骑乘系统主文件
├── IntegratedRidingSystemExample.cs  # 使用示例
└── README.md                     # 本文档
```

## 主要特性

### 1. 统一接口
- 实现 `IRidingSystem` 接口，提供标准化的骑乘操作
- 支持上马、下马、强制下马等核心功能
- 提供状态查询和触发点管理

### 2. 动画集成
- 自动播放上马/下马动画
- 支持动画层权重控制
- 提供动画完成事件回调

### 3. 状态管理
- 完整的骑乘状态跟踪
- 支持上马中、下马中等过渡状态
- 自动更新可上马/可下马状态

### 4. 组件集成
- 与 `RidingManager` 和 `MRider` 组件协同工作
- 自动管理角色控制器和移动组件
- 支持马的输入控制切换

## 安装和设置

### 1. 添加组件
将 `IntegratedRidingSystem` 组件添加到角色GameObject上：

```csharp
// 自动获取或手动指定组件
IntegratedRidingSystem ridingSystem = gameObject.AddComponent<IntegratedRidingSystem>();
```

### 2. 配置参数
在Inspector中配置以下参数：

#### 基础骑乘参数
- **Horse**: 马的GameObject引用
- **Is On Horse**: 当前是否在马上（运行时状态）

#### 组件引用
- **Riding Manager**: RidingManager组件引用（可选，会自动获取）
- **MRider**: MRider组件引用（可选，会自动获取）

#### 动画设置
- **Mount Animation Name**: 上马动画名称
- **Dismount Animation Name**: 下马动画名称
- **Mount Idle Animation Name**: 骑乘待机动画名称

#### 输入设置
- **Mount Key**: 上马按键（默认F）
- **Dismount Key**: 下马按键（默认F）

#### 调试
- **Debug**: 启用调试模式，显示详细日志

## 使用方法

### 1. 基本使用

```csharp
// 获取集成骑乘系统组件
IntegratedRidingSystem ridingSystem = GetComponent<IntegratedRidingSystem>();

// 检查是否可以上马
if (ridingSystem.CanMount)
{
    ridingSystem.MountAnimal();
}

// 检查是否可以下马
if (ridingSystem.CanDismount)
{
    ridingSystem.DismountAnimal();
}

// 强制下马
ridingSystem.ForceDismount();
```

### 2. 状态查询

```csharp
// 检查当前骑乘状态
bool isRiding = ridingSystem.IsRiding;
bool isMounting = ridingSystem.IsMounting;
bool isDismounting = ridingSystem.IsDismounting;

// 检查操作权限
bool canMount = ridingSystem.CanMount;
bool canDismount = ridingSystem.CanDismount;
bool canCallAnimal = ridingSystem.CanCallAnimal;

// 获取当前坐骑信息
Mount currentMount = ridingSystem.CurrentMount;
MountTriggers currentTrigger = ridingSystem.CurrentMountTrigger;
```

### 3. 触发点处理

```csharp
// 进入上马触发点
public void OnMountTriggerEnter(Mount mount, MountTriggers trigger)
{
    ridingSystem.MountTriggerEnter(mount, trigger);
}

// 离开上马触发点
public void OnMountTriggerExit()
{
    ridingSystem.MountTriggerExit();
}
```

### 4. 动画事件回调

在动画器中添加事件，调用以下方法：

```csharp
// 上马动画完成时调用
public void OnMountAnimationComplete()
{
    ridingSystem.OnMountAnimationComplete();
}

// 下马动画完成时调用
public void OnDismountAnimationComplete()
{
    ridingSystem.OnDismountAnimationComplete();
}
```

## 事件系统

### 1. 状态变化事件
系统会自动调用 `RidingManager` 和 `MRider` 的相应方法，触发以下事件：

- `OnStartMounting`: 开始上马
- `OnEndMounting`: 结束上马
- `OnStartDismounting`: 开始下马
- `OnEndDismounting`: 结束下马
- `OnFindMount`: 找到可骑乘的坐骑
- `OnCanMount`: 可以上马状态变化
- `OnCanDismount`: 可以下马状态变化

### 2. 事件监听示例

```csharp
public class RidingEventListener : MonoBehaviour
{
    public RidingManager ridingManager;

    void Start()
    {
        // 订阅事件
        ridingManager.OnStartMounting.AddListener(OnMountingStarted);
        ridingManager.OnEndMounting.AddListener(OnMountingEnded);
        ridingManager.OnStartDismounting.AddListener(OnDismountingStarted);
        ridingManager.OnEndDismounting.AddListener(OnDismountingEnded);
    }

    void OnMountingStarted()
    {
        Debug.Log("开始上马");
        // 播放上马音效、显示特效等
    }

    void OnMountingEnded()
    {
        Debug.Log("上马完成");
        // 启用马的输入控制等
    }

    void OnDismountingStarted()
    {
        Debug.Log("开始下马");
        // 禁用马的输入控制等
    }

    void OnDismountingEnded()
    {
        Debug.Log("下马完成");
        // 恢复角色输入控制等
    }
}
```

## 调试功能

### 1. 调试模式
启用 `Debug` 选项后，系统会输出详细的调试信息：

```
骑乘状态 - 在马上: True, 已上马: True, 可上马: False, 可下马: True
开始上马
开始上马过程
结束上马过程
```

### 2. 可视化调试
在Scene视图中会显示调试图标：
- 绿色圆圈：正在骑乘
- 黄色圆圈：可以上马

### 3. 状态监控
使用 `IntegratedRidingSystemExample` 组件可以在游戏运行时查看实时状态信息。

## 最佳实践

### 1. 组件配置
- 确保角色有 `CharacterController`、`Animator` 和 `ThirdPersonMove` 组件
- 确保马有 `MalbersInput` 组件和 `PlayerPoint` 子对象
- 正确设置动画层和动画名称

### 2. 动画设置
- 将骑乘动画放在第2层（索引为2）
- 确保动画名称与代码中设置的一致
- 在动画中添加完成事件回调

### 3. 输入处理
- 使用统一的按键进行上下马操作
- 在马上时禁用角色的移动输入
- 启用马的输入控制

### 4. 错误处理
- 检查组件引用是否正确
- 验证动画名称是否存在
- 确保触发点配置正确

## 故障排除

### 常见问题

1. **无法上马**
   - 检查是否在触发点范围内
   - 验证马的 `CanBeMountedByState` 是否为true
   - 确认动画名称是否正确

2. **动画不播放**
   - 检查动画层索引是否正确
   - 验证动画名称是否存在
   - 确认动画器状态机配置

3. **位置不正确**
   - 检查 `PlayerPoint` 子对象是否存在
   - 验证父级设置是否正确
   - 确认本地位置是否为零

4. **输入不响应**
   - 检查按键设置是否正确
   - 验证输入处理逻辑
   - 确认组件启用状态

### 调试步骤

1. 启用 `Debug` 模式
2. 检查控制台输出
3. 验证组件引用
4. 测试触发点功能
5. 检查动画播放

## 扩展功能

### 1. 自定义动画
可以通过修改动画名称参数来支持不同的上马/下马动画。

### 2. 多坐骑支持
系统支持多个坐骑，通过触发点自动切换。

### 3. 音效集成
可以在动画事件中添加音效播放逻辑。

### 4. 特效系统
可以在状态变化时添加粒子特效。

## 版本历史

- **v1.0**: 初始版本，集成基本骑乘功能
- 支持上马/下马动画触发
- 集成IRidingSystem接口
- 与RidingManager和MRider协同工作

## 许可证

本系统基于Malbers Animations插件开发，遵循相应的许可证条款。 