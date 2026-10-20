using UnityEngine;

public class VfxAutoDestroy : MonoBehaviour
{
    [SerializeField] EffectType _type;

    ParticleSystem _particle;

    public EffectType Type { get { return _type; } set { _type = value; } }

    void Awake()
    {
        _particle = GetComponent<ParticleSystem>();
    }

    
    void Update()
    {
        if (_particle != null)
        {
            if (_particle.isPlaying) return;
            else EffectEvents.RaiseEffectReturnedEvent(_type, gameObject);
        }
    }

    public void Play()
    {
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles)
        {
            p.Play();
        }
    }
}