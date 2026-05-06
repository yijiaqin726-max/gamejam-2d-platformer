# GameJar 2D平台跳跃游戏 - 项目结构文档

## 项目概述

**项目名称**: GameJar 2D Platformer  
**引擎**: Unity 2D  
**语言**: C#  
**类型**: 2D平台跳跃游戏原型  
**当前关卡**: S2_RomanColumns（罗马柱主题）

---

## 完整项目结构

```
gamejar-2d-platformer/
├── Assets/                                    # Unity资源文件夹
│   ├── Animations/                           # 动画帧资源
│   │   └── Player/                           # 玩家动画
│   │       ├── Idle/Frames/                  # 待机动画帧（4帧）
│   │       ├── Run*/Frames/                  # 跑步动画帧
│   │       ├── Jump/Frames/                  # 跳跃动画帧（11帧）
│   │       ├── Land/Frames/                  # 着陆动画帧
│   │       ├── Turn/Frames/                  # 转身动画帧
│   │       ├── LeafTransform/Frames/         # 叶子变身动画帧（20帧）
│   │       └── VineForm/Frames/              # 藤蔓形态动画帧
│   │
│   ├── Art/                                  # 美术资源
│   │   └── Environment/
│   │       └── RomanColumns/                 # 罗马柱主题资源
│   │           ├── roman_column_01.png       # 完整柱子变体1
│   │           ├── roman_column_02.png       # 完整柱子变体2
│   │           ├── roman_column_thin.png     # 完整柱子变体3（细柱）
│   │           ├── roman_column_broken.png   # 残缺柱子（障碍物）
│   │           ├── roman_columns_ground.png  # 地面瓷砖
│   │           ├── roman_columns_grass.png   # 草丛装饰
│   │           ├── roman_columns_tree_*.png  # 树木资源
│   │           ├── roman_columns_house.png   # 房子/神殿（通关点）
│   │           ├── roman_columns_rolling_stone_animated.png  # 滚石
│   │           └── 其他环境资源
│   │
│   ├── Scripts/                              # 代码文件
│   │   ├── Level/
│   │   │   ├── S2PrototypeBootstrap.cs       # 关卡生成器（主要改动文件）
│   │   │   └── PrototypeCameraFollow.cs      # 相机跟随系统
│   │   │
│   │   └── Player/
│   │       ├── PrototypePlayerController.cs  # 玩家控制器
│   │       └── PrototypeFrameAnimator.cs     # 帧动画系统
│   │
│   └── Scenes/                               # 场景文件
│       └── S2_RomanColumns.unity             # 罗马柱关卡场景
│
├── ProjectSettings/                          # Unity项目设置
├── Packages/                                 # 依赖包
├── Library/                                  # Unity库文件（自动生成）
├── Temp/                                     # 临时文件（自动生成）
├── doc/                                      # 文档文件夹
│   └── PROJECT_STRUCTURE.md                  # 本文件
│
└── 其他配置文件
    ├── Assembly-CSharp.csproj                # C#项目文件
    ├── .gitignore                            # Git忽略文件
    └── README.md                             # 项目说明
```

---

## 核心代码文件详解

### 1. S2PrototypeBootstrap.cs
**路径**: `Assets/Scripts/Level/S2PrototypeBootstrap.cs`  
**功能**: 关卡生成器 - 程序化生成整个S2关卡  
**关键职责**:
- 创建相机和玩家
- 生成地面、背景、装饰物
- 放置柱子、岩石、树木等关卡元素
- 管理碰撞体和物理

**主要方法**:
- `Build()` - 主入口，生成整个关卡
- `CreateLevel()` - 创建地面和基础结构
- `AddReferenceSceneDressing()` - 放置所有关卡道具
- `AddColumn()` - 添加可站立的完整柱子（随机3种外形）
- `AddBrokenColumnObstacle()` - 添加残缺柱子障碍物（弧形碰撞体）
- `CreateSpriteSize()` - 创建指定大小的精灵
- `AddTopPlatform()` - 添加单向平台

**关键常量**:
```
ColumnTargetWidth = 3.85 (柱子宽度)
FullColumnHeight = 完整柱子高度
BrokenColumnHeight = 残缺柱子高度
GroundTopY = 地面Y坐标
```

**最近改动**:
- 添加了三种柱子随机选择
- 改进了残缺柱子碰撞体（两层堆叠）
- 添加了完整柱子的solid碰撞体
- 调整了房子位置

---

### 2. PrototypePlayerController.cs
**路径**: `Assets/Scripts/Player/PrototypePlayerController.cs`  
**功能**: 玩家控制和物理  
**关键职责**:
- 处理玩家输入（移动、跳跃）
- 管理玩家状态（空中、着陆、转身）
- 控制玩家动画
- 实现双跳机制

**主要方法**:
- `Update()` - 每帧更新玩家状态
- `IsGrounded()` - 检测玩家是否接触地面
- `GetHorizontalInput()` - 获取水平输入
- `WasJumpPressed()` - 检测跳跃按键

**支持的输入**:
- A/D 或 左右箭头 - 移动
- 空格 - 跳跃（可双跳）

---

### 3. PrototypeFrameAnimator.cs
**路径**: `Assets/Scripts/Player/PrototypeFrameAnimator.cs`  
**功能**: 帧动画系统  
**关键职责**:
- 管理5种动画状态（Idle、Run、Jump、Land、Turn）
- 控制帧播放速度
- 处理一次性动画（着陆、转身）

**动画状态**:
- `Idle` - 待机（6fps，首帧延迟0.4秒）
- `Run` - 跑步（16fps）
- `Jump` - 跳跃（12fps）
- `Land` - 着陆（12fps，一次性）
- `Turn` - 转身（18fps，一次性）

---

### 4. PrototypeCameraFollow.cs
**路径**: `Assets/Scripts/Level/PrototypeCameraFollow.cs`  
**功能**: 相机跟随系统  
**关键职责**:
- 平滑跟随玩家
- 限制相机边界
- 固定Y轴位置

**参数**:
- `smoothTime` = 0.18秒（跟随平滑度）
- `fixedY` = 相机固定Y坐标
- `minX/maxX` = 相机X轴边界

---

## 美术资源详解

### 玩家动画资源
| 动画 | 帧数 | 路径 | 说明 |
|------|------|------|------|
| Idle | 4 | `Animations/Player/Idle/Frames/` | 待机循环 |
| Run | 10 | `Animations/Player/Run*/Frames/` | 跑步循环 |
| Jump | 11 | `Animations/Player/Jump/Frames/` | 跳跃循环 |
| Land | 6 | `Animations/Player/Land/Frames/` | 着陆一次性 |
| Turn | 6 | `Animations/Player/Turn/Frames/` | 转身一次性 |
| LeafTransform | 20 | `Animations/Player/LeafTransform/Frames/` | 变身动画 |
| VineForm | 多个 | `Animations/Player/VineForm/Frames/` | 藤蔓形态 |

### 环境资源
| 资源 | 用途 | 说明 |
|------|------|------|
| roman_column_01.png | 完整柱子 | 可站立，变体1 |
| roman_column_02.png | 完整柱子 | 可站立，变体2 |
| roman_column_thin.png | 完整柱子 | 可站立，变体3（细） |
| roman_column_broken.png | 残缺柱子 | 障碍物，弧形碰撞体 |
| roman_columns_ground.png | 地面 | 瓷砖纹理 |
| roman_columns_grass.png | 装饰 | 草丛 |
| roman_columns_tree_leaves.png | 装饰 | 树叶 |
| roman_columns_tree_brunk.png | 装饰 | 树干 |
| roman_columns_house.png | 通关点 | 神殿/房子 |
| roman_columns_rolling_stone_animated.png | 道具 | 滚石 |

---

## 关卡设计

### S2_RomanColumns 关卡布局

**关卡流程** (从左到右):
1. **起点** (X = -12) - 玩家出生点
2. **树木区** (X = -4.2) - 装饰和第一个岩石
3. **第一段** (X = 13-20) - 岩石 → 完整柱子 → 残缺柱子
4. **中段** (X = 25-34) - 岩石 → 完整柱子 → 残缺柱子
5. **后段** (X = 44-49) - 完整柱子 → 残缺柱子
6. **终点** (X = 54.5) - 房子/神殿（通关）

### 关键数值

| 参数 | 值 | 说明 |
|------|-----|------|
| 地面Y坐标 | GroundTopY | 所有道具基准 |
| 柱子宽度 | 3.85世界单位 | ColumnTargetWidth |
| 完整柱子高度 | ~2.0世界单位 | FullColumnHeight |
| 残缺柱子高度 | ~1.72世界单位 | BrokenColumnHeight |
| 相机宽度 | 16:9比例 | CameraAspect |
| 相机高度 | 6.1世界单位 | ScreenHeight |

---

## 物理系统

### 碰撞体类型

**完整柱子**:
- 顶部: `PlatformEffector2D` (单向平台，可站立)
- 身体: `BoxCollider2D` (solid，不可穿过)

**残缺柱子**:
- 两层堆叠 `BoxCollider2D`:
  - 下层: 宽度75%，高度60%
  - 上层: 宽度50%，高度40%
  - 效果: 玩家踩到上层会滑落

**玩家**:
- `BoxCollider2D` (1.1 x 3.0世界单位)
- `Rigidbody2D` (gravityScale = 2.4)

---

## 随机系统

### 柱子随机选择

**方法**: `GetRandomColumnFileName(float x)`

**特性**:
- 确定性伪随机（基于X坐标）
- 同一位置每次生成相同外形
- 不同位置外形不同

**算法**:
```csharp
var idx = Mathf.FloorToInt(PseudoRandom01((int)(x * 100f) + 3) * 3);
return ColumnVariants[idx];
```

**三种变体**:
1. `roman_column_01.png` - 标准柱子
2. `roman_column_02.png` - 变体2
3. `roman_column_thin.png` - 细柱

---

## 输入系统

### 支持两种输入方式

**新输入系统** (InputSystem):
```csharp
#if ENABLE_INPUT_SYSTEM
- A/D 键 - 移动
- 空格键 - 跳跃
#endif
```

**旧输入系统** (Legacy):
```csharp
- Horizontal轴 - 移动
- Jump按键 - 跳跃
```

---

## 编译和构建

### 项目文件
- `Assembly-CSharp.csproj` - C#项目文件
- 使用 `dotnet build` 编译

### 编译命令
```bash
dotnet build Assembly-CSharp.csproj
```

### 编译结果
- 输出: `Temp/bin/Debug/Assembly-CSharp.dll`
- 无错误、无警告

---

## 开发工作流

### 编辑器菜单
- **GameJam > Build S2 Prototype Preview** - 重新生成关卡

### 运行时初始化
- `[RuntimeInitializeOnLoadMethod]` - Play时自动生成关卡
- 检查场景名称，仅在S2_RomanColumns场景生成

### 编辑器脚本
- 使用 `#if UNITY_EDITOR` 条件编译
- 支持编辑器菜单和资源加载

---

## 关键改动历史

### 最新改动 (当前版本)

**1. 柱子碰撞体改进**
- 完整柱子: 添加solid BoxCollider2D（不可穿过）
- 残缺柱子: 改为两层堆叠碰撞体（模拟弧形顶部）

**2. 柱子随机外形**
- 实现三种柱子变体随机选择
- 每个位置固定一种外形（确定性随机）

**3. 房子位置调整**
- 从 `GroundTopY` 改为 `GroundTopY - GroundedPropSink`
- 房子不再悬空，与其他道具对齐

---

## 注意事项

### 不要修改
- 玩家控制器逻辑
- 相机跟随参数
- 其他关卡元素（树、岩石等）

### 确保
- 三个柱子变体文件存在
- 文件名完全匹配（包括大小写）
- 没有多余空格或换行

### 测试时
- 多次Play/Stop验证随机性
- 检查碰撞体与视觉对齐
- 确认玩家物理行为正常

---

## 文件大小参考

| 类型 | 数量 | 说明 |
|------|------|------|
| C#脚本 | 4 | 核心代码 |
| 玩家动画帧 | 100+ | 各种动画 |
| 环境资源 | 15+ | 柱子、树、草等 |
| 场景文件 | 1 | S2_RomanColumns |

---

## 相关文档

- `CHANGES_DOCUMENTATION.md` - 详细改动说明
- `README.md` - 项目总体说明
- 本文件 - 项目结构总览
