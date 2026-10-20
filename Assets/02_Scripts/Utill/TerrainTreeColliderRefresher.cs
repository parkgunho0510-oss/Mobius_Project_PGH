using System.Collections;
using UnityEngine;

public class TerrainTreeColliderRefresher : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Terrain / Tree 초기화 대기
        yield return null;

        Terrain terrain = GetComponent<Terrain>();

        // 이 스크립트를 Terrain 오브젝트가 아닌 다른 곳에 붙였을 때 대비
        if (terrain == null)
            terrain = Terrain.activeTerrain;

        if (terrain == null || terrain.terrainData == null)
            yield break;

        // Tree / Detail Prototype 갱신
        terrain.terrainData.RefreshPrototypes();

        yield return new WaitForFixedUpdate();

        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider == null)
            yield break;

        // TerrainCollider 자체를 껐다 켜서 내부 Tree Collider 갱신 유도
        terrainCollider.enabled = false;

        yield return new WaitForFixedUpdate();

        terrainCollider.enabled = true;
    }
}