using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대거의 생성/등록/해제를 전담합니다.
/// Dagger가 스스로 등록하지 않고, DaggerManager가 생성 시 직접 등록합니다.
/// </summary>
public class DaggerManager : MonoBehaviour
{
    public GameObject daggerPrefab;

    private readonly List<Transform> _daggers = new List<Transform>();

    /// <summary>대거를 월드에 생성하고 목록에 등록합니다.</summary>
    public GameObject SpawnDagger(Vector2 position)
    {
        GameObject obj = Instantiate(daggerPrefab, position, Quaternion.identity);
        _daggers.Add(obj.transform);
        return obj;
    }

    /// <summary>대거를 제거하고 목록에서 해제합니다.</summary>
    public void RemoveDagger(GameObject dagger)
    {
        if (dagger == null) return;
        _daggers.Remove(dagger.transform);
        Destroy(dagger);
    }

    /// <summary>가장 가까운 대거를 반환합니다. 없으면 null.</summary>
    public Transform GetNearestDagger()
    {
        Transform nearest = null;
        float     minDist = Mathf.Infinity;

        foreach (Transform d in _daggers)
        {
            if (d == null) continue;

            float dist = Vector2.Distance(transform.position, d.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = d;
            }
        }

        return nearest;
    }
}