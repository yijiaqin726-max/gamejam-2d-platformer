# 代码实现细节

## S2PrototypeBootstrap.cs 详解

### 类概述
静态类，负责程序化生成S2_RomanColumns关卡的所有元素。

### 常量定义

#### 相机参数
```csharp
private const float CameraY = -0.05f;              // 相机Y位置
private const float CameraOrthoSize = 3.05f;       // 相机正交大小
private const float CameraAspect = 16f / 9f;       // 宽高比
private const float ScreenHeight = CameraOrthoSize * 2f;  // 屏幕高度
```

#### 地面参数
```csharp
private const float GroundVisibleHeight = ScreenHeight * (1f / 10f);  // 地面可见高度
private const float GroundTopY = CameraY - CameraOrthoSize + GroundVisibleHeight;  // 地面顶部Y
private const float GroundArtHeight = GroundVisibleHeight;  // 地面美术高度
```

#### 柱子参数
```csharp
private const float ColumnTargetWidth = PlayerWorldWidth * 7f;  // 柱子宽度 ≈ 3.85
private const float FullColumnHeight = MainTreeHeight * (2f / 3f);  // 完整柱子高度
private const float BrokenColumnHeight = FullColumnHeight * 0.86f;  // 残缺柱子高度

// 残缺柱子碰撞体参数
private const float BrokenColumnColliderBottomWidthRatio = 0.375f;  // 下层宽度比例
private const float BrokenColumnColliderTopWidthRatio = 0.25f;      // 上层宽度比例
private const float BrokenColumnColliderTotalHeightRatio = 0.40f;   // 总高度比例
```

#### 其他参数
```csharp
private const float MapLeftX = -12f;       // 地图左边界
private const float MapRightX = 60f;       // 地图右边界
private const float GroundedPropSink = 0.18f;  // 道具下沉量
private const int PillarSortingOrder = 30;     // 柱子排序层级
```

---

### 主要方法

#### Build()
```csharp
public static void Build()
{
    if (GameObject.Find(RootName) != null) return;  // 防止重复生成
    
    var root = new GameObject(RootName);
    var cameraTransform = CreateCamera(root.transform);
    CreateBackground(root.transform);
    CreateLevel(root.transform);
    var playerTransform = CreatePlayer(root.transform);
    AttachCameraFollow(cameraTransform, playerTransform);
}
```
**流程**: 创建根对象 → 相机 → 背景 → 关卡 → 玩家 → 相机跟随

---

#### CreateLevel()
```csharp
private static void CreateLevel(Transform parent)
{
    var midground = new GameObject("Midground").transform;
    var foreground = new GameObject("Foreground").transform;
    var lightOrbs = new GameObject("Light Orbs").transform;
    
    // 创建地面瓷砖
    CreateTiledGroundArt(parent, "ground art", ..., MapLeftX, MapRightX, ...);
    
    // 创建地面碰撞体
    AddFloorCollider(parent, "floor", MapLeftX, MapRightX);
    
    // 创建边界墙
    CreateBoxTop(parent, "left wall", ...);
    CreateBoxTop(parent, "right wall", ...);
    
    // 放置所有道具
    AddReferenceSceneDressing(midground, foreground);
    
    // 放置光源
    for (int i = 0; i < 12; i++) CreateLight(...);
}
```

---

#### AddColumn() - 完整柱子
```csharp
private static void AddColumn(Transform parent, float x, float height)
{
    // 1. 随机选择柱子外形
    var fileName = GetRandomColumnFileName(x);
    
    // 2. 创建视觉精灵
    CreateSpriteSize(parent, fileName, 
        "Assets/Art/Environment/RomanColumns/" + fileName,
        new Vector2(x, GroundTopY - GroundedPropSink),
        new Vector2(ColumnTargetWidth, height), 
        PillarSortingOrder);
    
    // 3. 添加顶部单向平台（可站立）
    AddTopPlatform(parent, fileName + " standable top", x,
        GroundTopY + height - 0.05f, 
        ColumnTargetWidth * 0.72f);
    
    // 4. 添加solid碰撞体（不可穿过）
    var colliderObj = new GameObject(fileName + " solid collider");
    colliderObj.transform.SetParent(parent);
    colliderObj.transform.position = new Vector3(x, GroundTopY + height * 0.5f - GroundedPropSink, 0f);
    var collider = colliderObj.AddComponent<BoxCollider2D>();
    collider.size = new Vector2(ColumnTargetWidth * 0.8f, height);
}
```

**关键点**:
- 使用 `GetRandomColumnFileName()` 随机选择外形
- 顶部平台宽度为柱子宽度的72%
- Solid碰撞体宽度为柱子宽度的80%

---

#### AddBrokenColumnObstacle() - 残缺柱子
```csharp
private static void AddBrokenColumnObstacle(Transform parent, float x)
{
    // 1. 创建视觉精灵
    CreateSpriteSize(parent, "roman_column_broken obstacle",
        "Assets/Art/Environment/RomanColumns/roman_column_broken.png",
        new Vector2(x, GroundTopY - GroundedPropSink),
        new Vector2(ColumnTargetWidth, BrokenColumnHeight),
        PillarSortingOrder);
    
    // 2. 创建碰撞体对象
    var obj = new GameObject("roman_column_broken obstacle collider");
    obj.transform.SetParent(parent);
    obj.transform.position = new Vector3(x, 0f, 0f);
    
    // 3. 计算两层碰撞体的尺寸
    var bottomHeight = BrokenColumnHeight * BrokenColumnColliderTotalHeightRatio * 0.6f;
    var topHeight = BrokenColumnHeight * BrokenColumnColliderTotalHeightRatio * 0.4f;
    var bottomY = GroundTopY + bottomHeight * 0.5f;
    var topY = bottomY + bottomHeight * 0.5f + topHeight * 0.5f;
    
    // 4. 添加下层碰撞体（宽）
    var bottom = obj.AddComponent<BoxCollider2D>();
    bottom.offset = new Vector2(0f, bottomY);
    bottom.size = new Vector2(ColumnTargetWidth * BrokenColumnColliderBottomWidthRatio, bottomHeight);
    
    // 5. 添加上层碰撞体（窄）
    var top = obj.AddComponent<BoxCollider2D>();
    top.offset = new Vector2(0f, topY);
    top.size = new Vector2(ColumnTargetWidth * BrokenColumnColliderTopWidthRatio, topHeight);
}
```

**设计原理**:
- 两层堆叠模拟弧形顶部
- 下层宽度 = 原宽度 × 0.375 (75%)
- 上层宽度 = 下层宽度 × 0.5 (50%)
- 玩家踩到窄的上层会因物理推力向两侧滑落

---

#### GetRandomColumnFileName() - 随机选择
```csharp
private static readonly string[] ColumnVariants = {
    "roman_column_01.png",
    "roman_column_02.png",
    "roman_column_thin.png"
};

private static string GetRandomColumnFileName(float x)
{
    // 基于X坐标的确定性伪随机
    var idx = Mathf.FloorToInt(PseudoRandom01((int)(x * 100f) + 3) * ColumnVariants.Length);
    return ColumnVariants[Mathf.Clamp(idx, 0, ColumnVariants.Length - 1)];
}
```

**特性**:
- 确定性: 同一X坐标总是返回相同外形
- 分布性: 不同X坐标返回不同外形
- 无状态: 不需要保存随机种子

---

#### PseudoRandom01() - 伪随机函数
```csharp
private static float PseudoRandom01(int seed)
{
    var value = Mathf.Sin(seed * 12.9898f + 78.233f) * 43758.5453f;
    return value - Mathf.Floor(value);
}
```

**原理**: 使用正弦函数的周期性生成伪随机数
**输入**: 整数seed
**输出**: 0-1之间的浮点数

---

#### AddTopPlatform() - 单向平台
```csharp
private static void AddTopPlatform(Transform parent, string name, float x, float topY, float width)
{
    var obj = new GameObject(name);
    obj.transform.SetParent(parent);
    obj.transform.position = new Vector3(x, topY, 0f);
    
    // 碰撞体
    var box = obj.AddComponent<BoxCollider2D>();
    box.size = new Vector2(width, 0.15f);
    box.usedByEffector = true;
    
    // 单向平台效果器
    var effector = obj.AddComponent<PlatformEffector2D>();
    effector.useOneWay = true;
    effector.surfaceArc = 160f;  // 160度弧形
}
```

**效果**: 玩家只能从下方踩上去，不能从上方穿过

---

#### CreateSpriteSize() - 创建指定大小精灵
```csharp
private static void CreateSpriteSize(Transform parent, string name, string path, 
    Vector2 bottomCenter, Vector2 size, int sortingOrder)
{
    var sprite = LoadSprite(path);
    if (sprite == null) return;
    
    var obj = new GameObject(name);
    obj.transform.SetParent(parent);
    
    // 计算缩放比例
    obj.transform.localScale = new Vector3(
        size.x / sprite.bounds.size.x,
        size.y / sprite.bounds.size.y,
        1f);
    
    // 位置：底部中心
    obj.transform.position = new Vector3(
        bottomCenter.x,
        bottomCenter.y + size.y * 0.5f,
        0f);
    
    var renderer = obj.AddComponent<SpriteRenderer>();
    renderer.sprite = sprite;
    renderer.sortingOrder = sortingOrder;
}
```

**参数说明**:
- `bottomCenter`: 精灵底部中心的世界坐标
- `size`: 目标宽度和高度
- 自动计算缩放比例以适应目标大小

---

#### CreateSpriteHeight() - 创建指定高度精灵
```csharp
private static void CreateSpriteHeight(Transform parent, string name, string path, 
    Vector2 bottomCenter, float height, int sortingOrder)
{
    var sprite = LoadSprite(path);
    if (sprite == null) return;
    
    var scale = height / sprite.bounds.size.y;
    
    var obj = new GameObject(name);
    obj.transform.SetParent(parent);
    obj.transform.localScale = new Vector3(scale, scale, 1f);
    obj.transform.position = new Vector3(
        bottomCenter.x,
        bottomCenter.y + height * 0.5f,
        0f);
    
    var renderer = obj.AddComponent<SpriteRenderer>();
    renderer.sprite = sprite;
    renderer.sortingOrder = sortingOrder;
}
```

**与CreateSpriteSize的区别**:
- 只指定高度，宽度按比例缩放
- 保持精灵的宽高比

---

## PrototypePlayerController.cs 详解

### 主要属性
```csharp
private Rigidbody2D body;                    // 刚体
private SpriteRenderer spriteRenderer;       // 精灵渲染器
private PrototypeFrameAnimator frameAnimator; // 动画系统
private bool wasGrounded;                    // 上一帧是否接地
private bool isLanding;                      // 是否在着陆
private bool isTurning;                      // 是否在转身
private int facingDirection = 1;             // 朝向（1=右，-1=左）
private int remainingAirJumps;               // 剩余空中跳跃次数
```

### 关键方法

#### Update() - 主更新循环
```csharp
private void Update()
{
    // 1. 检测接地状态
    var grounded = IsGrounded();
    if (grounded && body.linearVelocity.y <= 0.01f)
    {
        remainingAirJumps = 1;  // 重置双跳
    }
    
    // 2. 处理着陆动画
    if (!wasGrounded && grounded && frameAnimator != null)
    {
        isLanding = true;
        isTurning = false;
        frameAnimator.SetState(PrototypeFrameAnimator.MotionState.Land, true);
    }
    
    if (isLanding)
    {
        // 着陆时停止水平移动
        var velocity = body.linearVelocity;
        velocity.x = 0f;
        body.linearVelocity = velocity;
        
        if (frameAnimator == null || frameAnimator.IsLandingComplete)
        {
            isLanding = false;
        }
        else
        {
            wasGrounded = grounded;
            return;  // 着陆期间不处理其他输入
        }
    }
    
    // 3. 处理转身动画
    var horizontal = GetHorizontalInput();
    if (!isTurning && grounded && frameAnimator != null && frameAnimator.HasTurnFrames
        && Mathf.Abs(horizontal) > 0.01f
        && (int)Mathf.Sign(horizontal) != facingDirection
        && Mathf.Abs(body.linearVelocity.x) > 0.1f)
    {
        isTurning = true;
        frameAnimator.SetState(PrototypeFrameAnimator.MotionState.Turn, true);
    }
    
    if (isTurning)
    {
        // 转身时停止水平移动
        var v = body.linearVelocity;
        v.x = 0f;
        body.linearVelocity = v;
        
        if (frameAnimator == null || frameAnimator.IsTurnComplete)
        {
            facingDirection = -facingDirection;
            spriteRenderer.flipX = facingDirection < 0;
            isTurning = false;
        }
        else
        {
            wasGrounded = grounded;
            return;  // 转身期间不处理其他输入
        }
    }
    
    // 4. 处理移动
    var moveVelocity = body.linearVelocity;
    moveVelocity.x = horizontal * moveSpeed;
    body.linearVelocity = moveVelocity;
    
    if (Mathf.Abs(horizontal) > 0.01f)
    {
        facingDirection = horizontal < 0f ? -1 : 1;
        spriteRenderer.flipX = facingDirection < 0;
    }
    
    // 5. 处理跳跃
    var jumpPressed = WasJumpPressed();
    if (jumpPressed && grounded)
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
        grounded = false;
    }
    else if (jumpPressed && !grounded && remainingAirJumps > 0)
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, 
            CalculateJumpVelocity(doubleJumpHeight));
        remainingAirJumps--;
        grounded = false;
    }
    
    // 6. 更新动画状态
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
    
    wasGrounded = grounded;
}
```

#### IsGrounded() - 接地检测
```csharp
private bool IsGrounded()
{
    // 向上运动时不算接地
    if (body != null && body.linearVelocity.y > 0.05f)
    {
        return false;
    }
    
    // 从玩家下方0.15单位处向下射线检测0.55单位
    var origin = (Vector2)transform.position + Vector2.down * 0.15f;
    return Physics2D.Raycast(origin, Vector2.down, 0.55f, groundMask);
}
```

#### CalculateJumpVelocity() - 跳跃速度计算
```csharp
private float CalculateJumpVelocity(float jumpHeight)
{
    var gravity = Mathf.Abs(Physics2D.gravity.y * body.gravityScale);
    return Mathf.Sqrt(2f * gravity * jumpHeight);
}
```

**公式**: v = √(2 × g × h)
- 根据目标高度计算所需初速度

---

## PrototypeFrameAnimator.cs 详解

### 动画状态机
```csharp
public enum MotionState
{
    Idle,   // 待机
    Run,    // 跑步
    Jump,   // 跳跃
    Land,   // 着陆（一次性）
    Turn    // 转身（一次性）
}
```

### 帧播放逻辑
```csharp
private void Update()
{
    var frames = GetFrames();
    if (targetRenderer == null || frames.Length == 0) return;
    
    timer += Time.deltaTime;
    var frameTime = 1f / Mathf.Max(1f, GetFps());
    
    // 待机首帧延迟
    if (currentState == MotionState.Idle && frameIndex == 0)
    {
        frameTime += Mathf.Max(0f, idleFirstFrameHoldSeconds);
    }
    
    if (timer < frameTime) return;
    
    timer -= frameTime;
    
    // 一次性动画处理
    if (currentState == MotionState.Land)
    {
        if (frameIndex >= frames.Length - 1)
        {
            landingComplete = true;
            return;
        }
        frameIndex++;
    }
    else if (currentState == MotionState.Turn)
    {
        if (frameIndex >= frames.Length - 1)
        {
            turnComplete = true;
            return;
        }
        frameIndex++;
    }
    else
    {
        // 循环动画
        frameIndex = (frameIndex + 1) % frames.Length;
    }
    
    ApplyFrame();
}
```

**关键点**:
- 循环动画: 帧索引循环
- 一次性动画: 帧索引到达末尾后停止，设置完成标志

---

## PrototypeCameraFollow.cs 详解

### 跟随逻辑
```csharp
private void LateUpdate()
{
    if (target == null) return;
    
    // 计算目标位置（X受限，Y固定）
    var desired = new Vector3(
        Mathf.Clamp(target.position.x, minX, maxX),
        fixedY,
        transform.position.z);
    
    // 平滑阻尼跟随
    transform.position = Vector3.SmoothDamp(
        transform.position, 
        desired, 
        ref velocity, 
        smoothTime);
}
```

**参数**:
- `smoothTime = 0.18秒`: 跟随平滑度
- `minX/maxX`: 相机X轴边界
- `fixedY`: 相机固定Y坐标

---

## 关键算法总结

| 算法 | 位置 | 用途 |
|------|------|------|
| 伪随机 | PseudoRandom01() | 确定性随机 |
| 接地检测 | IsGrounded() | 判断玩家是否接地 |
| 跳跃计算 | CalculateJumpVelocity() | 根据高度计算速度 |
| 帧播放 | PrototypeFrameAnimator.Update() | 控制动画播放 |
| 平滑跟随 | Vector3.SmoothDamp() | 相机平滑跟随 |
