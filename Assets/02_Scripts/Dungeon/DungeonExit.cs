using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonExit : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float _coolTime = 2.0f;

    [SerializeField] private float inputLockDelay = 0.2f;

    [Header("Effect Settings")]
    [SerializeField] private GameObject portalEffectObject;
    [SerializeField] private string dissolvePropertyName = "_Dissolve";

    private bool isTransitioning = false;
    private int dissolvePropertyID;

    private void Awake()
    {
        dissolvePropertyID = Shader.PropertyToID(dissolvePropertyName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTransitioning && other.CompareTag("Player"))
        {
            StartCoroutine(PortalSequence(other.gameObject));
        }
    }

    IEnumerator PortalSequence(GameObject player)
    {
        isTransitioning = true;



        // 포탈 파티클 이펙트 재생
        if (portalEffectObject != null)
        {
            portalEffectObject.SetActive(true); // 혹시 꺼져있을 경우를 대비해 켜줌

            // 자식에 있는 모든 파티클 시스템을 찾아 한 번에 재생 (가장 확실한 유니티 내장 방식)
            ParticleSystem[] particles = portalEffectObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                particle.Play();
            }
        }
        yield return new WaitForSeconds(inputLockDelay);

        // 플레이어 조작 잠금
        if (player.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.IsInputLocked = true;
        }
        // 플레이어 몸통(MeshRenderer) 가져오기
        Renderer[] playerRenderers = player.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        float elapsedTime = 0f;

        // 시간에 따른 Dissolve 수치 변경 (0 -> 1)
        while (elapsedTime < _coolTime)
        {
            elapsedTime += Time.deltaTime;
            // 0에서 1로 부드럽게 증가하는 비율 계산
            float t = elapsedTime / _coolTime;
            float dissolveValue = Mathf.Lerp(0f, 1f, t);

            foreach (var renderer in playerRenderers)
            {
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat(dissolvePropertyID, dissolveValue);
                renderer.SetPropertyBlock(propBlock);
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 연출이 완전히 끝나면 씬 이동
        DungeonSceneManager.Instance.ReturnToScene();
    }
}