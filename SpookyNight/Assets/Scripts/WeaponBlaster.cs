using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlaster : MonoBehaviour
{
    
    [SerializeField] private WeaponSettings weaponSettings;
    [SerializeField] private GameObject muzzleFlashEffect;

    [SerializeField] private AudioClip audioClipTakeOutWeapon;
    [SerializeField] private AudioClip audioClipFire;


    private float lastAttackTime = 0;

    private AudioSource audioSource;
    private PlayerAnimatorController animator;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<PlayerAnimatorController>();
    }

    private void OnEnable()
    {
        PlaySound(audioClipTakeOutWeapon);
        muzzleFlashEffect.SetActive(false);
    }

    public void StartWeaponAction(int type = 0)
    {
        if (weaponSettings.isAutomaticAttack == true)
        {
            StartCoroutine("OnAttackLoop");
        }
        else
        {
            OnAttack();
        }
    }

    public void StopWeaponAction(int type=0)
    {
        if (type == 0)
        {
            StopCoroutine("OnAttackLoop");
        }
    }

    private IEnumerator OnAttackLoop()
    {
        while (true)
        {
            OnAttack();

            yield return null;
        }
    }
    public void OnAttack()
    {
        if (Time.time - lastAttackTime > weaponSettings.attackRate)
        {
            lastAttackTime = Time.time;

            StartCoroutine("OnMuzzleFlashEffect");
            PlaySound(audioClipFire);
        }
    }

    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive (true);
        yield return new WaitForSeconds(weaponSettings.attackRate*0.3f);
        muzzleFlashEffect.SetActive (false);
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

}
