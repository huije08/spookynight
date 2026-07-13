using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("기본 스탯")]
    public float damage = 20f;
    public float fireRate = 0.3f;
    public float range = 100f;

    [Header("레퍼런스")]
    public Camera cam;
    public LaserBeam laserBeam;

    // 증강으로 해금
    [HideInInspector] public bool hasMultiShot = false;
    [HideInInspector] public int bulletCount = 3;
    [HideInInspector] public float spreadAngle = 15f;

    [HideInInspector] public bool hasChargeShot = false;
    [HideInInspector] public float maxChargeTime = 2f;
    [HideInInspector] public float chargeMultiplier = 3f;

    [HideInInspector] public bool hasPiercing = false;

    private float nextFireTime = 0f;
    private float chargeTime = 0f;
    private bool isCharging = false;
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
        // 우클릭 = 차지샷 (증강 획득 시 활성화)
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
                // UIManager.Instance.UpdateChargeBar(chargeTime / maxChargeTime);
            }

            if (Input.GetButtonUp("Fire2") && isCharging)
            {
                FireChargedShot();
                isCharging = false;
                chargeTime = 0f;
                return;
            }
        }

        // 좌클릭 = 일반샷 / 멀티샷
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && !isCharging)
        {
            nextFireTime = Time.time + fireRate;

            if (hasMultiShot)
                FireMultiShot();
            else
                FireSingleShot();
        }
    }

    // ─── 단발 ────────────────────────────────────────────────
    void FireSingleShot()
    {
        FireRaycast(cam.transform.forward, damage);
    }

    // ─── 멀티샷 (증강 해금) ───────────────────────────────────
    void FireMultiShot()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float offset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f,
                                      (float)i / (bulletCount - 1));
            Vector3 direction = Quaternion.Euler(0f, offset, 0f) * cam.transform.forward;
            FireRaycast(direction, damage);
        }
    }

    // ─── 차지샷 (증강 해금) ───────────────────────────────────
    void FireChargedShot()
    {
        float ratio = chargeTime / maxChargeTime;
        float finalDamage = damage * (1f + ratio * (chargeMultiplier - 1f));

        if (ratio >= 0.8f)
            FirePiercingShot(finalDamage, cam.transform.forward);
        else
            FireRaycast(cam.transform.forward, finalDamage);
    }

    // ─── Raycast 공통 ────────────────────────────────────────
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

        laserBeam.Show(cam.transform.position, endPoint, false);
    }

    // ─── 관통 ────────────────────────────────────────────────
    void FirePiercingShot(float dmg, Vector3 direction)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            cam.transform.position, 0.4f, direction, range);

        Vector3 endPoint = cam.transform.position + direction * range;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(dmg);
                playerStats.OnDamageDealt(dmg);
                endPoint = hit.point;
            }
        }

        laserBeam.Show(cam.transform.position, endPoint, true);
    }
}
