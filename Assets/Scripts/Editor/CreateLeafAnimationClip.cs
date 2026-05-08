using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Editor 工具：自动生成树叶动画 AnimationClip 和 AnimatorController
/// 从 Assets/Imported_S1/leaf 文件夹读取 Sprite 序列帧
/// </summary>
public sealed class CreateLeafAnimationClip
{
    private const string LEAF_SPRITES_PATH = "Assets/Imported_S1/leaf";
    private const string OUTPUT_ANIM_PATH = "Assets/Animations/Leaf";
    private const string ANIM_CLIP_NAME = "Leaf_Fall_Loop.anim";
    private const string CONTROLLER_NAME = "Leaf_Fall_Controller.controller";
    private const int DEFAULT_FPS = 12;

    [MenuItem("Tools/Leaf Sequence/Create Leaf Animation")]
    public static void CreateLeafAnimation()
    {
        // 查找所有 Sprite
        List<Sprite> sprites = FindLeafSprites();

        if (sprites.Count == 0)
        {
            Debug.LogError("CreateLeafAnimationClip: No sprites found in " + LEAF_SPRITES_PATH);
            return;
        }

        // 创建输出文件夹
        if (!Directory.Exists(OUTPUT_ANIM_PATH))
        {
            Directory.CreateDirectory(OUTPUT_ANIM_PATH);
            AssetDatabase.Refresh();
        }

        // 生成 AnimationClip
        string clipPath = Path.Combine(OUTPUT_ANIM_PATH, ANIM_CLIP_NAME);
        AnimationClip clip = CreateAnimationClip(sprites, clipPath);

        if (clip == null)
        {
            Debug.LogError("CreateLeafAnimationClip: Failed to create animation clip");
            return;
        }

        // 生成 AnimatorController
        string controllerPath = Path.Combine(OUTPUT_ANIM_PATH, CONTROLLER_NAME);
        CreateAnimatorController(clip, controllerPath);

        Debug.Log($"CreateLeafAnimationClip: Successfully created animation with {sprites.Count} frames");
        Debug.Log($"Animation Clip: {clipPath}");
        Debug.Log($"Animator Controller: {controllerPath}");
    }

    private static List<Sprite> FindLeafSprites()
    {
        List<Sprite> sprites = new List<Sprite>();

        // 查找所有 PNG 文件
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { LEAF_SPRITES_PATH });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        // 按文件名中的数字排序
        sprites.Sort((a, b) =>
        {
            int numA = ExtractNumberFromFilename(a.name);
            int numB = ExtractNumberFromFilename(b.name);
            return numA.CompareTo(numB);
        });

        return sprites;
    }

    private static int ExtractNumberFromFilename(string filename)
    {
        // 从文件名中提取数字
        string numberStr = "";
        foreach (char c in filename)
        {
            if (char.IsDigit(c))
                numberStr += c;
            else if (numberStr.Length > 0)
                break;
        }

        if (int.TryParse(numberStr, out int number))
            return number;

        return 0;
    }

    private static AnimationClip CreateAnimationClip(List<Sprite> sprites, string clipPath)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = DEFAULT_FPS;

        // 创建 EditorCurveBinding 用于 SpriteRenderer.sprite
        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve(
            "",
            typeof(SpriteRenderer),
            "m_Sprite"
        );

        // 创建关键帧
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / (float)DEFAULT_FPS,
                value = sprites[i]
            };
        }

        // 设置曲线
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        // 设置循环
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // 保存 AnimationClip
        AssetDatabase.CreateAsset(clip, clipPath);
        AssetDatabase.SaveAssets();

        return clip;
    }

    private static void CreateAnimatorController(AnimationClip clip, string controllerPath)
    {
        // 创建 AnimatorController
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 获取默认 Layer
        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;

        // 创建状态
        AnimatorState state = stateMachine.AddState("Leaf_Fall_Loop");
        state.motion = clip;

        // 设置为默认状态
        stateMachine.defaultState = state;

        // 保存
        AssetDatabase.SaveAssets();
    }
}
