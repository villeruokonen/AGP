using System;
using UnityEngine;

public class MovementSound : MonoBehaviour
{
    [Serializable]
    public struct KeywordSoundPair
    {
        public string keywords;
        public AudioClip sound;

        public bool HasTextureKeyword(string textureName)
        {
            string[] keywords = this.keywords.Split(';');
            foreach (var kw in keywords)
            {
                if (textureName.Contains(kw, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    [SerializeField] private AudioClip _jumpClip;
    [SerializeField] private AudioClip _landingClip;
    [SerializeField] private AudioClip _defaultFootstep;

    [SerializeField] private bool _playFootSteps = true;
    [SerializeField] private bool _playJumpSound = true;
    [SerializeField] private bool _playLandingSound = true;

    [SerializeField] private float _groundRaycastLength;

    [Tooltip("When the falling velocity is this or larger, the fall sound volume is max.")]
    [SerializeField] private float _fallSoundMaxVolumeVelocity = -10f;

    [Tooltip("Material name keywords to match with sounds, " +
        "for separate sounds based on the material under the player.")]
    [SerializeField] private KeywordSoundPair[] _keywordSoundPairs;


    // Min time between landing sounds
    private const float LAND_SOUND_COOLDOWN = 0.5f;
    private float _landSoundCooldownTimer;

    private void Update()
    {
        IncrementTimers();
    }

    private void IncrementTimers()
    {
        _landSoundCooldownTimer += Time.deltaTime;
    }

    public void PlayJumpSound()
    {
        if (!_playJumpSound)
            return;

        PlaySound(_jumpClip);
    }

    public void PlayLandingSound(float velocity)
    {
        if (!_playLandingSound)
            return;

        if (_landSoundCooldownTimer < LAND_SOUND_COOLDOWN)
            return;
        
        _landSoundCooldownTimer = 0;

        float volume = Mathf.InverseLerp(0, _fallSoundMaxVolumeVelocity, velocity);

        PlaySound(_landingClip, volume);
    }

    public void PlayFootStep()
    {
        if (!_playFootSteps)
            return;

        PlaySound(GetFootstepClip());
    }

    void PlaySound(AudioClip clip, float volume = 0.8f, float pitch = 1)
    {
        if (clip == null)
            return;

        GameObject audio = new(clip.name);
        audio.transform.position = transform.position;
        AudioSource source = audio.AddComponent<AudioSource>();
        source.spatialBlend = 1.0f;
        source.dopplerLevel = 0;
        source.clip = clip;
        source.volume = volume;
        source.Play();

        Destroy(audio, clip.length + 0.15f);
    }

    // Used solely for playing different footstep sounds depending on the texture
    // under the player's feet
    int GetHitSubmesh(RaycastHit hitInfo)
    {
        var meshCollider = hitInfo.collider as MeshCollider;
        if (meshCollider == null)
            return 0;

        if (meshCollider.sharedMesh.subMeshCount > 1)
        {
            int submeshStartIndex = 0;
            for (int i = 0; i < meshCollider.sharedMesh.subMeshCount; i++)
            {
                int numSubmeshTris = meshCollider.sharedMesh.GetTriangles(i).Length / 3;
                if (hitInfo.triangleIndex < submeshStartIndex + numSubmeshTris)
                    return i;
                submeshStartIndex += numSubmeshTris;
            }
            return -1;
        }
        return 0;
    }

    // Get correct footstep sound based on what we're standing on
    AudioClip GetFootstepClip()
    {
        AudioClip defaultClip = _defaultFootstep;

        var ray = new Ray(transform.position, Vector3.down);
        var mask = ~(1 << LayerMask.NameToLayer("Player"));

        // If there's nothing below us, don't return any sound
        if (!Physics.Raycast(ray, out var hitInfo, _groundRaycastLength, mask, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        var rend = hitInfo.transform.GetComponent<Renderer>();
        if (rend == null || rend.material.mainTexture == null) { return null; }

        var texName = rend.material.mainTexture.name;

        if (hitInfo.collider is MeshCollider)
        {
            int submesh = GetHitSubmesh(hitInfo);

            texName = rend.materials[submesh].mainTexture.name;
        }

        foreach (var pair in _keywordSoundPairs)
        {
            if (pair.HasTextureKeyword(texName))
            {
                return pair.sound;
            }
        }

        return defaultClip;
    }

}
