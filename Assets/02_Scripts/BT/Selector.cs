using System.Collections.Generic;

/// <summary>
/// [REFACTORING]
///
/// 1. Success / Failure 시 노드 상태 정리 추가
/// - 기존 -> 노드 종료 후 내부 상태(코루틴, 애니메이션, 타이머 등)가 남을 가능성 존재
/// - 변경 -> Success / Failure 반환 직전에 currentNode.Abort() 호출
/// - 효과 -> 행동 종료 시 내부 상태를 안전하게 초기화하여 상태 꼬임 방지
///
/// 2. Abort() 전체 전파 방식으로 개선
/// - 기존 -> 현재 Running 중인 노드만 Abort 처리
/// - 문제 -> 이전에 실행되었던 노드들의 내부 상태가 남아있을 가능성 존재
/// - 변경 -> Selector 하위 모든 자식 노드에 Abort 전파
/// - 효과 -> 스턴, 그로기, 사망, 패턴 전환 시 상태 초기화 안정성 향상
///
/// 3. Selector 실행 안정성 강화
/// - 기존 -> 행동 전환 과정에서 이전 행동 상태가 일부 남을 가능성 존재
/// - 변경 -> 행동 종료 및 중단 시 상태 초기화를 명확하게 수행
/// - 효과 -> 우선순위 기반 행동 전환 안정성 향상
/// </summary>

public class Selector : BTNode
{
    private List<BTNode> _nodes;
    private int _currentIndex = 0; // 상태 기억을 위한 인덱스 추가

    public Selector(List<BTNode> nodes)
    {
        _nodes = nodes;
    }

    // 셀렉터는 성공 시 멈춤
    public override NodeState Evaluate()
    {
        while (_currentIndex < _nodes.Count)
        {
            BTNode currentNode = _nodes[_currentIndex];

            switch (currentNode.Evaluate())
            {
                case NodeState.Running:
                    // 진행 중인 행동이 있으면 트리를 붙잡아두고 인덱스 유지
                    return State = NodeState.Running;

                case NodeState.Success:

                    // 성공한 노드 상태 정리
                    currentNode.Abort();

                    // 하나라도 성공하면 셀렉터 전체 성공 및 인덱스 초기화
                    _currentIndex = 0;
                    return State = NodeState.Success;

                case NodeState.Failure:

                    // 실패한 노드 상태 정리
                    currentNode.Abort();

                    // 실패하면 다음 자식 노드 평가
                    _currentIndex++;
                    continue;
            }
        }

        // 다 실패하면 Failure 반환
        _currentIndex = 0;
        return State = NodeState.Failure;
    }

    public override void Abort()
    {
        base.Abort();

        // 모든 자식 노드 상태 정리
        foreach (var node in _nodes)
        {
            node.Abort();
        }
        /*if (_currentIndex < _nodes.Count)
        {
            _nodes[_currentIndex].Abort();
        }*/
        _currentIndex = 0;
    }
}