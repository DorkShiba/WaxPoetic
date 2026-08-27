using UnityEngine;

namespace Systems
{
    public class SoundManager
    {
        public void Play(AudioClip clip)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        }
    }
}
