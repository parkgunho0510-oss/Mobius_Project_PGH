using System.Collections.Generic;

/// <summary>
/// [REFACTORING]
///
/// 1. Running 상태 기억 방식 유지
/// - 기존 -> 매 프레임 처음부터 자식 노드 재탐색 가능성 존재
/// - 변경 -> _currentIndex를 유지하여 Running 상태의 노드부터 평가 재개
/// - 효과 -> 불필요한 재평가 감소 및 행동 흐름 안정화
///
/// 2. Success / Failure 시 노드 상태 정리 추가
/// - 기존 -> 노드 종료 후 내부 상태(코루틴, 애니메이션, 타이머 등)가 남을 가능성 존재
/// - 변경 -> Success / Failure 반환 직전에 currentNode.Abort() 호출
/// - 효과 -> 행동 종료 시 내부 상태를 안전하게 초기화하여 상태 꼬임 방지
///
/// 3. Abort() 전체 전파 방식으로 개선
/// - 기존 -> 현재 Running 중인 노드만 Abort 처리
/// - 문제 -> 이전에 실행되었던 노드들의 내부 상태가 남아있을 가능성 존재
/// - 변경 -> Composite(Sequence / Selector) 하위 모든 자식 노드에 Abort 전파
/// - 효과 -> 스턴, 그로기, 사망, 패턴 강제 전환 시 상태 꼬임 및 잔여 행동 방지
///
/// 4. Composite 노드 안정성 강화
/// - 기존 -> 특정 상황에서 애니메이션, 이동 상태, 코루틴 등이 잔존할 가능성 존재
/// - 변경 -> 모든 종료 흐름에서 상태 초기화를 명확히 수행
/// - 효과 -> 보스 AI의 예외 상황 대응 안정성 향상
/// </summary>

public class Sequence : BTNode
{
    private List<BTNode> _nodes;
    private int _currentIndex = 0;

    public Sequence(List<BTNode> nodes)
    {
        _nodes = nodes;
    }

    // 시퀀스는 실패 시 멈춤
    public override NodeState Evaluate()
    {
        // while을 사용하여 현재 실행 중인 인덱스부터 평가를 재개
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

                    // 성공했으면 다음 리스트에 있는 행동으로 넘어감
                    _currentIndex++;
                    continue;

                case NodeState.Failure:

                    // 실패한 노드 상태 정리
                    currentNode.Abort();

                    // 하나라도 실패하면 시퀀스 전체 실패 및 인덱스 초기화
                    _currentIndex = 0;
                    return State = NodeState.Failure;
            }
        }

        // 리스트의 모든 행동을 실패 없이 끝마쳤다면 최종 성공
        _currentIndex = 0;
        return State = NodeState.Success;
    }

    public override void Abort()
    {
        base.Abort();

        // 모든 자식 노드 상태 정리
        foreach (var node in _nodes)
        {
            node.Abort();
        }
        /*// 전체 노드가 아닌 현재 실행 중이던 노드만 정확히 중단
        if (_currentIndex < _nodes.Count)
        {
            _nodes[_currentIndex].Abort();
        }*/
        _currentIndex = 0;
    }
}