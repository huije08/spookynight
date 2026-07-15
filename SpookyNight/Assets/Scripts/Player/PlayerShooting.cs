using UnityEngine;
using System.Collections;
 
public class PlayerShooting : MonoBehaviour
{
    [Header("기본 스탯")]
    public float damage = 20f;
    public float fireRate = 0.3f;
    public float range = 100f;

    [Header("레퍼런스")]
    public Camera cam;
    public Transform gunPoint;

    // 증강으로 해금
    [HideInInspector] public bool hasMultiShot = false;
    [HideInInspector] public int bulletCount = 3;
    [HideInInspector] public float burstInterval = 0.08f;

    [HideInInspector] public bool hasChargeShot = false;
    [HideInInspector] public float maxChargeTime = 2f;
    [HideInInspector] public float chargeMultiplier = 3f;

    [HideInInspector] public bool hasPiercing = false;
    [HideInInspector] public float piercingRadius = 0.4f;

    private float nextFireTime = 0f;
    private float chargeTime = 0f;
    private bool isCharging = false;
    private bool isBursting = false;
    private PlayerStats playerStats;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        HandleShooting();
    }

    void HandleShooting()
    {
        // 우클릭 = 차지샷 (증강 해금 시 활성화)
        if (hasChargeShot)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                isCharging = true;
                chargeTime = 0f;
            }

            if (Input.GetButton("Fire2") && isCharging)
            {
                chargeTime += Time.deltaTime;
                chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            }

            if (Input.GetButtonUp("Fire2") && isCharging)
            {
                FireChargedShot();
                isCharging = false;
                chargeTime = 0f;
                return;
            }
        }

        // 좌클릭 = 단발 or 점사
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && !isCharging && !isBursting)
        {
            nextFireTime = Time.time + fireRate;

            if (hasMultiShot)
                StartCoroutine(BurstFire());
            else
                FireSingleShot();
        }
    }

    // 단발
    void FireSingleShot()
    {
        FireRaycast(cam.transform.forward, damage);
    }

    // 점사 (bulletCount발을 burstInterval 간격으로)
    IEnumerator BurstFire()
    {
        isBursting = true;

        for (int i = 0; i < bulletCount; i++)
        {
            FireRaycast(cam.transform.forward, damage);
            yield return new WaitForSeconds(burstInterval);
        }

        isBursting = false;
    }

    // 차지샷
    void FireChargedShot()
    {
        float ratio = chargeTime / maxChargeTime;
        float finalDamage = damage * (1f + ratio * (chargeMultiplier - 1f));

        if (ratio >= 0.8f)
            FirePiercingShot(finalDamage, cam.transform.forward);
        else
            FireRaycast(cam.transform.forward, finalDamage);
    }

    // Raycast 공통
    void FireRaycast(Vector3 direction, float dmg)
    {
        if (hasPiercing)
        {
            FirePiercingShot(dmg, direction);
            return;
        }

        Ray ray = new Ray(cam.transform.position, direction);
        Vector3 endPoint = cam.transform.position + direction * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            endPoint = hit.point;
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(dmg);
                playerStats.OnDamageDealt(dmg);
            }
        }
    }

    // 관통
    void FirePiercingShot(float dmg, Vector3 direction)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            cam.transform.position, piercingRadius, direction, range);

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(dmg);
                playerStats.OnDamageDealt(dmg);
            }
        }
    }
}