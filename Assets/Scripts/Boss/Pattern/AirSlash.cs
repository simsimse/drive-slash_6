using System.Collections;
using UnityEngine;

public class AirSlash : MonoBehaviour, IBossPattern
{
    [Header("탄환 설정")]
    public int repeatCount = 2;
    public float repeatInterval = 0.5f;
    

    public float rotationPerWave = 20f;
    public GameObject airSlashPrefab;
    public Transform bossCenter;
    public float angleOffset = 20f;

    [Header("패턴 설정")]
    public int bulletCount = 8;
    public float spawnRadiusX = 2.5f;
    public float spawnRadiusY = 4f;
    public float bulletSpeed = 8f;
    public int damage = 2;
    public Vector2 spawnCenterOffset = new Vector2(1.2f, 0f);

    public float PatternDuration => 1f;


    public bool CanExecute()
    {
        return true;
    }
    public void Execute()
    {
        StartCoroutine(AirSlashRoutine());
    }

    IEnumerator AirSlashRoutine()
{
    float angleStep = 360f / bulletCount;
    float rotationPerWave = angleStep / 2f; // 빈 공간으로 쏘기

    for (int wave = 0; wave < repeatCount; wave++)
    {
        float currentRotation = wave * rotationPerWave;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = angleStep * i + angleOffset + currentRotation;

            SpawnBullet(angle);
        }

        yield return new WaitForSeconds(repeatInterval);
    }
}

    void SpawnBullet(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ).normalized;


        Vector3 center = bossCenter.position + (Vector3)spawnCenterOffset;


        Vector3 spawnPos = center + new Vector3(
            Mathf.Cos(rad) * spawnRadiusX,
            Mathf.Sin(rad) * spawnRadiusY,
            0f
        );

        GameObject bullet = Instantiate(
            airSlashPrefab,
            spawnPos,
            Quaternion.identity
        );

        AirSlashBullet slash = bullet.GetComponent<AirSlashBullet>();

        if (slash != null)
        {
            slash.SetDirection(dir, bulletSpeed, damage);
        }

        // 탄환이 날아가는 방향으로 회전
        float zRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, zRot);
    }
}