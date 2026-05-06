# 游戏玩法和机制

## 玩家控制

### 输入方式

| 输入 | 功能 | 说明 |
|------|------|------|
| A / 左箭头 | 向左移动 | 连续按住移动 |
| D / 右箭头 | 向右移动 | 连续按住移动 |
| 空格 | 跳跃 | 地面跳跃或空中二段跳 |

### 移动参数

```csharp
moveSpeed = 5f              // 水平移动速度
jumpVelocity = 8f           // 地面跳跃初速度
doubleJumpHeight = 3.25f    // 二段跳目标高度
```

---

## 动画状态机

### 5种动画状态

```
┌─────────────────────────────────────────┐
│         PrototypeFrameAnimator          │
├─────────────────────────────────────────┤
│ Idle (待机)                             │
│  - FPS: 6                               │
│  - 首帧延迟: 0.4秒                      │
│  - 循环播放                             │
│                                         │
│ Run (跑步)                              │
│  - FPS: 16                              │
│  - 循环播放                             │
│                                         │
│ Jump (跳跃)                             │
│  - FPS: 12                              │
│  - 循环播放                             │
│                                         │
│ Land (着陆) ⭐ 一次性                   │
│  - FPS: 12                              │
│  - 播放完后停止                         │
│  - 触发条件: 从空中落地                 │
│                                         │
│ Turn (转身) ⭐ 一次性                   │
│  - FPS: 18                              │
│  - 播放完后停止                         │
│  - 触发条件: 地面上改变方向且有速度     │
└─────────────────────────────────────────┘
```

### 状态转换逻辑

```
地面 + 无输入 → Idle
地面 + 有水平输入 → Run
地面 + 改变方向 + 有速度 → Turn (优先级最高)
地面 + 跳跃 → Jump
空中 → Jump
着陆 → Land (一次性，完成后回到Idle/Run)
```

### 动画播放代码

```csharp
// 在PrototypePlayerController.Update()中
if (!grounded)
{
    frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Jump);
}
else if (Mathf.Abs(horizontal) > 0.01f)
{
    frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Run);
}
else
{
    frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Idle);
}
```

---

## 物理系统

### 玩家物理参数

```csharp
gravityScale = 2.4f         // 重力倍数（比默认更重）
collider.size = (1.1, 3.0)  // 碰撞体大小
collider.offset = (0, -0.2) // 碰撞体偏移
```

### 地面检测

```csharp
private bool IsGrounded()
{
    // 1. 检查垂直速度（向上则不接地）
    if (body.linearVelocity.y > 0.05f)
        return false;
    
    // 2. 从玩家下方0.15单位处发射射线
    var origin = (Vector2)transform.position + Vector2.down * 0.15f;
    
    // 3. 向下投射0.55单位，检测groundMask层
    return Physics2D.Raycast(origin, Vector2.down, 0.55f, groundMask);
}
```

### 跳跃计算

```csharp
// 二段跳高度计算
private float CalculateJumpVelocity(float jumpHeight)
{
    var gravity = Mathf.Abs(Physics2D.gravity.y * body.gravityScale);
    return Mathf.Sqrt(2f * gravity * jumpHeight);
}

// 使用: 
// 地面跳跃: jumpVelocity = 8f (固定)
// 二段跳: CalculateJumpVelocity(3.25f) (计算得出)
```

---

## 碰撞体设计

### 完整柱子（可站立）

```
┌─────────────────────┐
│   顶部平台          │  ← PlatformEffector2D (单向)
│   宽度: 72% × 柱宽  │     只能从下方踩上
│   高度: 0.15单位    │
├─────────────────────┤
│                     │
│   柱子视觉          │  ← SpriteRenderer
│   宽度: 100%        │
│   高度: FullHeight  │
│                     │
├─────────────────────┤
│   Solid碰撞体       │  ← BoxCollider2D (solid)
│   宽度: 80% × 柱宽  │     不可穿过
│   高度: FullHeight  │
└─────────────────────┘
```

### 残缺柱子（障碍物）

```
┌─────────────────────┐
│   上层碰撞体        │  ← BoxCollider2D (窄)
│   宽度: 25% × 柱宽  │     玩家踩到会滑落
│   高度: 40% × 总高  │
├─────────────────────┤
│   下层碰撞体        │  ← BoxCollider2D (宽)
│   宽度: 37.5% × 柱宽│     支撑上层
│   高度: 60% × 总高  │
├─────────────────────┤
│                     │
│   柱子视觉          │  ← SpriteRenderer
│   宽度: 100%        │
│   高度: BrokenHeight│
│                     │
└─────────────────────┘
```

**设计原理**: 两层堆叠模拟弧形顶部，玩家踩到窄的上层会因物理推力向两侧滑落

---

## 关卡流程

### S2_RomanColumns 关卡地图

```
起点                                          终点
(-12)                                        (54.5)
  ↓                                            ↓
┌─────────────────────────────────────────────────┐
│ 🌳 树木  🪨 岩石  🏛️ 柱子  ⚠️ 残缺柱  🏠 房子 │
│                                                 │
│ -4.2  -5.7  13.2  18.0  20.8  25.8  30.8  ... │
└─────────────────────────────────────────────────┘
```

### 关键位置

| X坐标 | 元素 | 说明 |
|-------|------|------|
| -12 ~ 0 | 起点区 | 玩家出生点，教学区 |
| -4.2 | 树木 | 装饰，第一个障碍 |
| -5.7 | 岩石 | 可站立 |
| 13.2 | 岩石 | 可站立 |
| 18.0 | 完整柱子 | 随机3种外形 |
| 20.8 | 残缺柱子 | 障碍物，弧形碰撞体 |
| 25.8 | 岩石 | 可站立 |
| 30.8 | 完整柱子 | 随机3种外形 |
| 34.4 | 残缺柱子 | 障碍物 |
| 44.6 | 完整柱子 | 随机3种外形 |
| 49.0 | 残缺柱子 | 障碍物 |
| 54.5 | 房子 | 通关点 |

---

## 相机系统

### 相机参数

```csharp
smoothTime = 0.18f          // 跟随平滑度（秒）
fixedY = -0.05f             // 固定Y坐标
minX = -12f                 // 左边界
maxX = 60f                  // 右边界
```

### 跟随逻辑

```csharp
private void LateUpdate()
{
    if (target == null) return;
    
    // 计算目标位置（限制在边界内）
    var desired = new Vector3(
        Mathf.Clamp(target.position.x, minX, maxX),
        fixedY,
        transform.position.z
    );
    
    // 平滑移动到目标位置
    transform.position = Vector3.SmoothDamp(
        transform.position, 
        desired, 
        ref velocity, 
        smoothTime
    );
}
```

**特性**:
- 水平跟随玩家（限制边界）
- Y轴固定不动
- 使用SmoothDamp实现平滑过渡

---

## 着陆和转身机制

### 着陆流程

```csharp
// 检测着陆
if (!wasGrounded && grounded && frameAnimator != null)
{
    isLanding = true;
    isTurning = false;
    frameAnimator.SetState(PrototypeFrameAnimator.MotionState.Land, true);
}

// 着陆期间
if (isLanding)
{
    // 停止水平移动
    var velocity = body.linearVelocity;
    velocity.x = 0f;
    body.linearVelocity = velocity;
    
    // 等待着陆动画完成
    if (frameAnimator == null || frameAnimator.IsLandingComplete)
    {
        isLanding = false;
    }
    else
    {
        return;  // 跳过本帧其他逻辑
    }
}
```

### 转身流程

```csharp
// 检测转身条件
if (!isTurning
    && grounded
    && frameAnimator != null
    && frameAnimator.HasTurnFrames
    && Mathf.Abs(horizontal) > 0.01f           // 有输入
    && (int)Mathf.Sign(horizontal) != facingDirection  // 改变方向
    && Mathf.Abs(body.linearVelocity.x) > 0.1f)  // 有速度
{
    isTurning = true;
    frameAnimator.SetState(PrototypeFrameAnimator.MotionState.Turn, true);
}

// 转身期间
if (isTurning)
{
    // 停止水平移动
    var v = body.linearVelocity;
    v.x = 0f;
    body.linearVelocity = v;
    
    // 等待转身动画完成
    if (frameAnimator == null || frameAnimator.IsTurnComplete)
    {
        facingDirection = -facingDirection;
        spriteRenderer.flipX = facingDirection < 0;
        isTurning = false;
    }
    else
    {
        return;  // 跳过本帧其他逻辑
    }
}
```

---

## 关键数值参考

### 玩家参数
| 参数 | 值 | 单位 |
|------|-----|------|
| 移动速度 | 5 | 世界单位/秒 |
| 地面跳跃速度 | 8 | 世界单位/秒 |
| 二段跳高度 | 3.25 | 世界单位 |
| 重力倍数 | 2.4 | 倍数 |
| 碰撞体宽度 | 1.1 | 世界单位 |
| 碰撞体高度 | 3.0 | 世界单位 |

### 关卡参数
| 参数 | 值 | 单位 |
|------|-----|------|
| 柱子宽度 | 3.85 | 世界单位 |
| 完整柱子高度 | ~2.0 | 世界单位 |
| 残缺柱子高度 | ~1.72 | 世界单位 |
| 地面Y坐标 | -2.95 | 世界单位 |
| 相机高度 | 6.1 | 世界单位 |
| 相机宽度 | 10.85 | 世界单位 |

---

## 调试技巧

### 查看碰撞体
1. 在Scene视图中启用Gizmos
2. 选择Physics2D → 显示所有碰撞体

### 调整参数
所有可调参数都在S2PrototypeBootstrap.cs顶部的常量中，修改后重新Play即可生效

### 常见问题排查
- **玩家穿过柱子**: 检查solid碰撞体是否正确添加
- **玩家卡在残缺柱子上**: 检查上层碰撞体宽度是否足够窄
- **着陆动画不播放**: 检查frameAnimator是否正确初始化
- **相机跟不上**: 调整smoothTime参数
