# 游戏地图2改动文档

## 项目信息

**项目名称**: GameJar 2D平台跳跃游戏  
**目标文件**: `Assets/Scripts/Level/S2PrototypeBootstrap.cs`  
**改动类型**: 关卡设计 - 柱子碰撞体和随机外形  
**开发环境**: Unity 2D, C#

---

## 项目结构

```
gamejar-2d-platformer/
├── Assets/
│   ├── Scripts/
│   │   ├── Level/
│   │   │   └── S2PrototypeBootstrap.cs          ← 主要改动文件
│   │   ├── Player/
│   │   │   ├── PrototypePlayerController.cs
│   │   │   └── PrototypeFrameAnimator.cs
│   │   └── ...
│   ├── Art/
│   │   └── Environment/
│   │       └── RomanColumns/
│   │           ├── roman_column_01.png          ← 完整柱子变体1
│   │           ├── roman_column_02.png          ← 完整柱子变体2
│   │           ├── roman_column_thin.png        ← 完整柱子变体3
│   │           ├── roman_column_broken.png      ← 残缺柱子（唯一）
│   │           └── ...其他资源
│   └── ...
└── ...
```

---

## 必须知道的事项

### 1. 常量定义位置
文件: `S2PrototypeBootstrap.cs` 第37-41行

**旧常量（需要删除）**:
```csharp
private const float BrokenColumnColliderWidthRatio = 0.5f;
private const float BrokenColumnColliderHeightRatio = 0.32f;
```

**新常量（需要添加）**:
```csharp
private const float BrokenColumnColliderBottomWidthRatio = 0.375f;  // 0.5 * 0.75
private const float BrokenColumnColliderTopWidthRatio = 0.25f;
private const float BrokenColumnColliderTotalHeightRatio = 0.40f;   // 0.32 * 1.25
```

### 2. 关键数值说明

| 参数 | 值 | 说明 |
|------|-----|------|
| ColumnTargetWidth | PlayerWorldWidth * 7f | 柱子宽度（约3.85世界单位） |
| FullColumnHeight | MainTreeHeight * (2/3) | 完整柱子高度 |
| BrokenColumnHeight | FullColumnHeight * 0.86 | 残缺柱子高度 |
| BrokenColumnColliderBottomWidthRatio | 0.375 | 下层碰撞体宽度比例（原宽度的75%） |
| BrokenColumnColliderTopWidthRatio | 0.25 | 上层碰撞体宽度比例（下层的50%） |
| BrokenColumnColliderTotalHeightRatio | 0.40 | 总碰撞体高度比例（原高度的125%） |

### 3. 物理行为

**残缺柱子碰撞体设计**:
- 采用**两层堆叠BoxCollider2D**模拟弧形顶部
- 下层（宽）：宽度较宽，高度占总高的60%
- 上层（窄）：宽度为下层50%，高度占总高的40%
- 效果：玩家踩到窄的上层会因为物理推力向两侧滑落

**完整柱子**:
- 可站立（有PlatformEffector2D）
- 随机三种外形之一

### 4. 随机算法

使用**确定性伪随机**（基于X坐标）:
```csharp
private static float PseudoRandom01(int seed)
{
    var value = Mathf.Sin(seed * 12.9898f + 78.233f) * 43758.5453f;
    return value - Mathf.Floor(value);
}
```

优点：
- 同一位置每次生成相同外形
- 不同位置外形不同
- 无需保存状态

---

## 改动详情

### 改动1: 更新常量（第37-41行）

**操作**: 替换两个旧常量为三个新常量

```csharp
// 删除这两行:
private const float BrokenColumnColliderWidthRatio = 0.5f;
private const float BrokenColumnColliderHeightRatio = 0.32f;

// 添加这三行:
private const float BrokenColumnColliderBottomWidthRatio = 0.375f;
private const float BrokenColumnColliderTopWidthRatio = 0.25f;
private const float BrokenColumnColliderTotalHeightRatio = 0.40f;
```

---

### 改动2: 添加列数组和随机函数（第244行之前）

**位置**: `AddReferenceSceneDressing`方法之前

**添加代码**:
```csharp
private static readonly string[] ColumnVariants = {
    "roman_column_01.png",
    "roman_column_02.png",
    "roman_column_thin.png"
};

private static string GetRandomColumnFileName(float x)
{
    var idx = Mathf.FloorToInt(PseudoRandom01((int)(x * 100f) + 3) * ColumnVariants.Length);
    return ColumnVariants[Mathf.Clamp(idx, 0, ColumnVariants.Length - 1)];
}
```

---

### 改动3: 修改AddColumn方法签名（第327行）

**旧方法**:
```csharp
private static void AddColumn(Transform parent, float x, string fileName, float height)
{
    CreateSpriteSize(parent, fileName, "Assets/Art/Environment/RomanColumns/" + fileName, 
        new Vector2(x, GroundTopY - GroundedPropSink), 
        new Vector2(ColumnTargetWidth, height), PillarSortingOrder);
    AddTopPlatform(parent, fileName + " standable top", x, 
        GroundTopY + height - 0.05f, ColumnTargetWidth * 0.72f);
}
```

**新方法**:
```csharp
private static void AddColumn(Transform parent, float x, float height)
{
    var fileName = GetRandomColumnFileName(x);
    CreateSpriteSize(parent, fileName, "Assets/Art/Environment/RomanColumns/" + fileName, 
        new Vector2(x, GroundTopY - GroundedPropSink), 
        new Vector2(ColumnTargetWidth, height), PillarSortingOrder);
    AddTopPlatform(parent, fileName + " standable top", x, 
        GroundTopY + height - 0.05f, ColumnTargetWidth * 0.72f);
}
```

**改动点**:
- 删除参数 `string fileName`
- 添加一行: `var fileName = GetRandomColumnFileName(x);`

---

### 改动4: 修改AddBrokenColumnObstacle方法（第333行）

**旧方法**:
```csharp
private static void AddBrokenColumnObstacle(Transform parent, float x)
{
    CreateSpriteSize(parent, "roman_column_broken obstacle", 
        "Assets/Art/Environment/RomanColumns/roman_column_broken.png", 
        new Vector2(x, GroundTopY - GroundedPropSink), 
        new Vector2(ColumnTargetWidth, BrokenColumnHeight), PillarSortingOrder);

    var obj = new GameObject("roman_column_broken obstacle collider");
    obj.transform.SetParent(parent);
    obj.transform.position = new Vector3(x, GroundTopY + BrokenColumnHeight * BrokenColumnColliderHeightRatio * 0.5f, 0f);
    var box = obj.AddComponent<BoxCollider2D>();
    box.size = new Vector2(ColumnTargetWidth * BrokenColumnColliderWidthRatio, 
        BrokenColumnHeight * BrokenColumnColliderHeightRatio);
}
```

**新方法**:
```csharp
private static void AddBrokenColumnObstacle(Transform parent, float x)
{
    CreateSpriteSize(parent, "roman_column_broken obstacle", 
        "Assets/Art/Environment/RomanColumns/roman_column_broken.png", 
        new Vector2(x, GroundTopY - GroundedPropSink), 
        new Vector2(ColumnTargetWidth, BrokenColumnHeight), PillarSortingOrder);

    var obj = new GameObject("roman_column_broken obstacle collider");
    obj.transform.SetParent(parent);
    obj.transform.position = new Vector3(x, 0f, 0f);

    var bottomHeight = BrokenColumnHeight * BrokenColumnColliderTotalHeightRatio * 0.6f;
    var topHeight = BrokenColumnHeight * BrokenColumnColliderTotalHeightRatio * 0.4f;
    var bottomY = GroundTopY + bottomHeight * 0.5f;
    var topY = bottomY + bottomHeight * 0.5f + topHeight * 0.5f;

    var bottom = obj.AddComponent<BoxCollider2D>();
    bottom.offset = new Vector2(0f, bottomY);
    bottom.size = new Vector2(ColumnTargetWidth * BrokenColumnColliderBottomWidthRatio, bottomHeight);

    var top = obj.AddComponent<BoxCollider2D>();
    top.offset = new Vector2(0f, topY);
    top.size = new Vector2(ColumnTargetWidth * BrokenColumnColliderTopWidthRatio, topHeight);
}
```

**改动点**:
- 将单个BoxCollider2D改为两个堆叠的BoxCollider2D
- 使用offset而不是position来定位碰撞体
- 计算下层和上层的高度、Y位置
- 下层宽度使用BrokenColumnColliderBottomWidthRatio
- 上层宽度使用BrokenColumnColliderTopWidthRatio

---

### 改动5: 更新AddColumn调用点（第255, 262, 266行）

**位置**: `AddReferenceSceneDressing`方法内

**旧代码**:
```csharp
AddColumn(midground, 18.0f, "roman_column_01.png", FullColumnHeight);
AddColumn(midground, 30.8f, "roman_column_01.png", FullColumnHeight);
AddColumn(midground, 44.6f, "roman_column_01.png", FullColumnHeight);
```

**新代码**:
```csharp
AddColumn(midground, 18.0f, FullColumnHeight);
AddColumn(midground, 30.8f, FullColumnHeight);
AddColumn(midground, 44.6f, FullColumnHeight);
```

**改动点**:
- 删除第二个参数 `"roman_column_01.png"`
- 保留X坐标和高度参数

---

## 验证清单

完成改动后，需要验证以下内容：

- [ ] 代码编译无错误
- [ ] 打开Unity编辑器
- [ ] 进入Play模式
- [ ] 观察三个完整柱子是否显示不同外形
- [ ] 跑向残缺柱子，确认玩家会滑落而不是站立
- [ ] 使用菜单 "GameJam > Build S2 Prototype Preview" 重建预览
- [ ] 在Scene视图中查看碰撞体Gizmo，确认两层堆叠结构

---

## 文件修改总结

| 文件 | 行号 | 改动类型 | 说明 |
|------|------|--------|------|
| S2PrototypeBootstrap.cs | 37-41 | 常量替换 | 更新碰撞体比例常量 |
| S2PrototypeBootstrap.cs | 244-254 | 新增代码 | 添加列数组和随机函数 |
| S2PrototypeBootstrap.cs | 327-331 | 方法修改 | AddColumn移除fileName参数 |
| S2PrototypeBootstrap.cs | 333-342 | 方法重写 | AddBrokenColumnObstacle改为两层碰撞体 |
| S2PrototypeBootstrap.cs | 255, 262, 266 | 调用更新 | 移除AddColumn的fileName参数 |

---

## 关键概念解释

### BoxCollider2D的offset和size
- **offset**: 碰撞体相对于GameObject位置的偏移
- **size**: 碰撞体的宽度和高度
- 多个BoxCollider2D可以在同一GameObject上，形成复合碰撞体

### PseudoRandom01函数
- 输入: 整数seed
- 输出: 0-1之间的浮点数
- 特性: 相同seed总是返回相同值（确定性）

### PlatformEffector2D
- 使碰撞体成为单向平台
- 玩家只能从下方踩上去
- 不能从上方穿过

---

## 注意事项

1. **不要修改**:
   - 其他常量值
   - 玩家控制器代码
   - 相机跟随代码
   - 其他关卡元素

2. **确保**:
   - 三个列变体文件存在于 `Assets/Art/Environment/RomanColumns/`
   - 文件名完全匹配（包括大小写）
   - 没有多余的空格或换行

3. **测试时**:
   - 多次Play/Stop以确保随机性正常
   - 检查碰撞体是否与视觉资源对齐
   - 确认玩家物理行为符合预期

