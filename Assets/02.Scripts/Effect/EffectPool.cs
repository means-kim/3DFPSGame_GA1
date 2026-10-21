using System.Collections;
using UnityEngine;
using Lean.Pool;

public class EffectPool : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleEffect;

    private void Awake()
    {
        _particleEffect = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        StartCoroutine(DestroyEffect());
    }

    private IEnumerator DestroyEffect()
    {
        yield return null;

        while (_particleEffect.IsAlive(true))
        {
            yield return null;
        }

        LeanPool.Despawn(_particleEffect);
    }
}