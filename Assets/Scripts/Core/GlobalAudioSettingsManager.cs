using UnityEngine;
using UnityEngine.SceneManagement;

public enum AudioVolumeChannel
{
    Music,
    PlayerSfx
}

public sealed class GlobalAudioSettingsManager : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string PlayerSfxVolumeKey = "PlayerSfxVolume";

    public static GlobalAudioSettingsManager Instance { get; private set; }

    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float playerSfxVolume = 1f;

    public float MusicVolume => musicVolume;
    public float PlayerSfxVolume => playerSfxVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceBeforeSceneLoad()
    {
        EnsureInstance();
    }

    public static GlobalAudioSettingsManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GlobalAudioSettingsManager existing = Object.FindAnyObjectByType<GlobalAudioSettingsManager>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject managerObject = new GameObject("GlobalAudioSettingsManager");
        Instance = managerObject.AddComponent<GlobalAudioSettingsManager>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        playerSfxVolume = PlayerPrefs.GetFloat(PlayerSfxVolumeKey, 1f);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetPlayerSfxVolume(float value)
    {
        playerSfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PlayerSfxVolumeKey, playerSfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include);
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null)
            {
                continue;
            }

            if (IsMusicSource(source))
            {
                source.volume = musicVolume;
            }
        }

        ApplyPlayerSfxVolume();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumes();
    }

    private void ApplyPlayerSfxVolume()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (!IsPlayerObject(candidate))
            {
                continue;
            }

            AudioSource[] playerSources = candidate.GetComponentsInChildren<AudioSource>(true);
            for (int sourceIndex = 0; sourceIndex < playerSources.Length; sourceIndex++)
            {
                AudioSource source = playerSources[sourceIndex];
                if (source != null && !IsMusicSource(source))
                {
                    source.volume = playerSfxVolume;
                }
            }
        }
    }

    private static bool IsMusicSource(AudioSource source)
    {
        AudioVolumeTarget target = source.GetComponent<AudioVolumeTarget>();
        if (target != null)
        {
            return target.Channel == AudioVolumeChannel.Music;
        }

        string objectName = source.gameObject.name;
        return ContainsName(objectName, "BGM")
            || ContainsName(objectName, "Music")
            || ContainsName(objectName, "BackgroundMusic");
    }

    private static bool IsPlayerObject(GameObject candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        bool isTaggedPlayer = SafeCompareTag(candidate, "Player");
        string objectName = candidate.name;
        return isTaggedPlayer
            || ContainsName(objectName, "Player")
            || ContainsName(objectName, "Player Prototype");
    }

    private static bool SafeCompareTag(GameObject candidate, string tagName)
    {
        if (candidate == null || string.IsNullOrEmpty(tagName))
        {
            return false;
        }

        try
        {
            return candidate.CompareTag(tagName);
        }
        catch (UnityException)
        {
            return false;
        }
    }

    private static bool ContainsName(string source, string value)
    {
        return !string.IsNullOrEmpty(source)
            && source.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
