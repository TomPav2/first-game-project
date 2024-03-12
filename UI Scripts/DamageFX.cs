using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DamageFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScriptableRendererFeature _fullScreenDamage;
    [SerializeField] private Material _material;

    private int _flashIntensity = Shader.PropertyToID("_FlashIntensity");
    private int _flashPower = Shader.PropertyToID("_FlashPower");

    private float _intensity;

    Coroutine effectRoutine;

    void Start()
    {
        _fullScreenDamage.SetActive(false);
    }

    public void playDamageEffect(byte amount)
    {
        if (effectRoutine != null) StopCoroutine(effectRoutine);
        effectRoutine = StartCoroutine(takeDamageEffect(amount));
    }

    private IEnumerator takeDamageEffect(byte amount)
    {
        _intensity = (amount * 0.1f) + 0.1f;
        _fullScreenDamage.SetActive(true);
        _material.SetFloat(_flashIntensity, _intensity);

        yield return new WaitForSeconds(0.1f);

        while (_intensity > 0)
        {
            _intensity -= 0.02f + (0.01f * amount);

            if (_intensity <= 0) _intensity = 0;
            _material.SetFloat(_flashIntensity, _intensity);
            yield return new WaitForSeconds(0.1f);
        }

        _fullScreenDamage.SetActive(false);

        yield break;
    }
}
