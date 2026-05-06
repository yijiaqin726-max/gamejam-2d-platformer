# 美术资源完整清单

## 玩家动画资源

### Idle (待机)
**路径**: `Assets/Animations/Player/Idle/Frames/`  
**帧数**: 4  
**FPS**: 6  
**循环**: 是  
**首帧延迟**: 0.4秒

| 文件 | 说明 |
|------|------|
| player_idle_01.png | 待机帧1 |
| player_idle_02.png | 待机帧2 |
| player_idle_03.png | 待机帧3 |
| player_idle_04.png | 待机帧4 |

---

### Run (跑步)
**路径**: `Assets/Animations/Player/Run*/Frames/`  
**帧数**: 10 (左腿前进5帧 + 右腿前进5帧)  
**FPS**: 16  
**循环**: 是

#### RunLeftLegForward
| 文件 | 说明 |
|------|------|
| player_run_left_leg_forward_01.png | 左腿前进帧1 |
| player_run_left_leg_forward_02.png | 左腿前进帧2 |
| player_run_left_leg_forward_03.png | 左腿前进帧3 |
| player_run_left_leg_forward_04.png | 左腿前进帧4 |
| player_run_left_leg_forward_05.png | 左腿前进帧5 |

#### RunRightLegForward
| 文件 | 说明 |
|------|------|
| player_run_right_leg_forward_01.png | 右腿前进帧1 |
| player_run_right_leg_forward_02.png | 右腿前进帧2 |
| player_run_right_leg_forward_03.png | 右腿前进帧3 |
| player_run_right_leg_forward_04.png | 右腿前进帧4 |
| player_run_right_leg_forward_05.png | 右腿前进帧5 |

---

### Jump (跳跃)
**路径**: `Assets/Animations/Player/Jump/Frames/`  
**帧数**: 11  
**FPS**: 12  
**循环**: 是

| 文件 | 说明 |
|------|------|
| player_jump_00.png | 跳跃帧0 |
| player_jump_01.png | 跳跃帧1 |
| player_jump_02.png | 跳跃帧2 |
| player_jump_03.png | 跳跃帧3 |
| player_jump_04.png | 跳跃帧4 |
| player_jump_05.png | 跳跃帧5 |
| player_jump_06.png | 跳跃帧6 |
| player_jump_07.png | 跳跃帧7 |
| player_jump_08.png | 跳跃帧8 |
| player_jump_09.png | 跳跃帧9 |
| player_jump_10.png | 跳跃帧10 |

---

### Land (着陆)
**路径**: `Assets/Animations/Player/Land/Frames/`  
**帧数**: 6  
**FPS**: 12  
**循环**: 否（一次性）  
**触发**: 从空中落地

| 文件 | 说明 |
|------|------|
| player_land_01.png | 着陆帧1 |
| player_land_02.png | 着陆帧2 |
| player_land_03.png | 着陆帧3 |
| player_land_04.png | 着陆帧4 |
| player_land_05.png | 着陆帧5 |
| player_land_06.png | 着陆帧6 |

---

### Turn (转身)
**路径**: `Assets/Animations/Player/Turn/Frames/`  
**帧数**: 6  
**FPS**: 18  
**循环**: 否（一次性）  
**触发**: 地面上改变方向且有速度

| 文件 | 说明 |
|------|------|
| player_turn_01.png | 转身帧1 |
| player_turn_02.png | 转身帧2 |
| player_turn_03.png | 转身帧3 |
| player_turn_04.png | 转身帧4 |
| player_turn_05.png | 转身帧5 |
| player_turn_06.png | 转身帧6 |

---

### RunToStop (停止)
**路径**: `Assets/Animations/Player/RunToStop/Frames/`  
**帧数**: 5  
**说明**: 从跑步到停止的过渡动画

| 文件 | 说明 |
|------|------|
| player_run_to_stop_01.png | 停止帧1 |
| player_run_to_stop_02.png | 停止帧2 |
| player_run_to_stop_03.png | 停止帧3 |
| player_run_to_stop_04.png | 停止帧4 |
| player_run_to_stop_05.png | 停止帧5 |

---

### LeafTransform (变身)
**路径**: `Assets/Animations/Player/LeafTransform/Frames/`  
**帧数**: 20  
**说明**: 玩家变身为叶子的动画

| 文件 | 说明 |
|------|------|
| player_leaf_transform_01.png ~ 20.png | 变身帧1-20 |

---

### VineForm (藤蔓形态)
**路径**: `Assets/Animations/Player/VineForm/Frames/`  
**说明**: 玩家变身为藤蔓的多个形态

#### 形态分类
- c_0101 ~ c_0101: 形态1变体1
- c_0102 ~ c_0102: 形态1变体2
- c_0201 ~ c_0201: 形态2变体1
- c_0202 ~ c_0202: 形态2变体2
- c_0301 ~ c_0301: 形态3变体1
- c_0302 ~ c_0302: 形态3变体2
- c_0401 ~ c_0401: 形态4变体1
- c_0402 ~ c_0402: 形态4变体2
- c_0402b: 形态4变体2b
- c_0501 ~ c_0503: 形态5变体1-3

**每个形态**: 24帧动画

---

## 环境资源 (RomanColumns主题)

### 柱子资源
**路径**: `Assets/Art/Environment/RomanColumns/`

#### 完整柱子（可站立）
| 文件 | 用途 | 说明 |
|------|------|------|
| roman_column_01.png | 完整柱子变体1 | 标准柱子，随机选择 |
| roman_column_02.png | 完整柱子变体2 | 变体2，随机选择 |
| roman_column_thin.png | 完整柱子变体3 | 细柱，随机选择 |

**特性**:
- 顶部有单向平台（可站立）
- 身体有solid碰撞体（不可穿过）
- 每个位置随机选择一种外形

#### 残缺柱子（障碍物）
| 文件 | 用途 | 说明 |
|------|------|------|
| roman_column_broken.png | 残缺柱子 | 唯一外形，弧形碰撞体 |

**特性**:
- 两层堆叠碰撞体
- 上层窄，玩家踩到会滑落
- 不可站立

---

### 地面和装饰
| 文件 | 用途 | 说明 |
|------|------|------|
| roman_columns_ground.png | 地面 | 瓷砖纹理，平铺 |
| roman_columns_grass.png | 草丛 | 装饰，多个位置 |
| roman_columns_tree_leaves.png | 树叶 | 树冠，装饰 |
| roman_columns_tree_brunk.png | 树干 | 树干，装饰 |

---

### 关键道具
| 文件 | 用途 | 说明 |
|------|------|------|
| roman_columns_house.png | 房子/神殿 | 通关点，关卡终点 |
| roman_columns_rolling_stone_animated.png | 滚石 | 可站立的岩石 |

---

### 其他资源
| 文件 | 用途 | 说明 |
|------|------|------|
| roman_columns_arch.png | 拱门 | 未使用 |
| roman_columns_pit.png | 坑 | 未使用 |
| roman_columns_pit_floor.png | 坑底 | 未使用 |
| roman_columns_short_railing.png | 栏杆 | 未使用 |
| roman_columns_solid_blue_background.png | 背景 | 纯色背景 |
| roman_columns_preview.png | 预览 | 关卡预览图 |

---

## 资源规格

### 玩家精灵
- **像素/单位**: 128 PPU
- **缩放**: 0.5倍
- **世界宽度**: 1.1 × 0.5 = 0.55单位
- **世界高度**: 3.0 × 0.5 = 1.5单位

### 柱子精灵
- **目标宽度**: 3.85世界单位
- **完整柱子高度**: ~2.0世界单位
- **残缺柱子高度**: ~1.72世界单位
- **缩放**: 根据目标尺寸自动计算

### 岩石精灵
- **高度**: 与树叶底部对齐
- **宽度**: 自动计算

---

## 资源加载方式

### 编辑器模式
```csharp
var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
if (sprite != null) return sprite;

var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
if (texture != null)
{
    texture.filterMode = FilterMode.Bilinear;
    return Sprite.Create(texture, 
        new Rect(0f, 0f, texture.width, texture.height), 
        new Vector2(0.5f, 0.5f), 
        pixelsPerUnit);
}
```

### 运行时模式
- 返回null（需要预加载或使用Resources文件夹）

---

## 资源使用统计

| 类型 | 数量 | 说明 |
|------|------|------|
| 玩家动画帧 | 100+ | 多个动画状态 |
| 环境资源 | 15+ | 柱子、地面、装饰 |
| 完整柱子变体 | 3 | 随机选择 |
| 残缺柱子 | 1 | 固定外形 |
| 总资源文件 | 150+ | 包括所有帧 |

---

## 资源优化建议

1. **动画帧**: 考虑使用Sprite Atlas减少Draw Call
2. **纹理**: 使用Bilinear过滤保证质量
3. **内存**: 关卡加载时动态加载资源
4. **性能**: 预加载常用资源到Resources文件夹
