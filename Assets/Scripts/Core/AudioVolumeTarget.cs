using UnityEngine;

public sealed class AudioVolumeTarget : MonoBehaviour
{
    [SerializeField] private AudioVolumeChannel channel = AudioVolumeChannel.Music;

    public AudioVolumeChannel Channel => channel;
}
