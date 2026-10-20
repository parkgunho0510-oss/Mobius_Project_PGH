using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// [REFACTORING]
/// 
/// 1. 코루틴을 활용한 전체 생성 프로세스 관리
/// - 기존 Awake 에서 모든 함수를 실행하던 방식 대신, GenerateDungeonRoutine 코루틴을 통해 단계별로 생성 과정을 관리.
/// - 이를 통해 DFS (데이터 계산), 오브젝트 생성(Prefab), 네브메쉬 베이크 등의 무거운 작업들을 프레임별로 끊어서 실행하여,
///   유니티 엔진 부하 및 멈춤 현상 방지.
///   
/// 2. CheckNeighbors 함수 최적화
/// - 매번 List를 생성하지 않고 고정 배열 필드를 사용하여 성능 향상
/// - 기존 List<Vector2Int> neighbors 대신 Vector2Int[] _neighborsBuffer와 int neighborCount로 관리.
///   (new 키워드가 루프 속 반복되면 GC 과부하 발생하여 프레임 드랍 가능 -> 배열 하나만 선언 후 개수만 반환하여 재사용하도록 수정)
/// 
/// 3. Safety Loop 제거
/// - 기존 DFS에서 무한 루프 방지용으로 safetyLoop 변수를 사용했으나, 
///   코루틴으로 단계별로 실행되면서 자연스럽게 프레임마다 쉬어주기 때문에 안전장치로써의 역할 대체.
/// 
/// 4. System.Random 추가
/// - UnityEngine.Random은 메인 스레드 종속적이며 내부 로직이 무겁기에, 다수의 랜덤 호출 시 성능 저하 발생 가능.
/// - DFS 용으론 C# 표준인 System.Random을 사용하여 랜덤 인덱스 생성 (성능 우위)
/// </summary>

public enum RoomType { Start, Monster, Structure, Escape, CorridorH, CorridorV }
[System.Serializable]
public struct RoomPrefabData
{
    public RoomType type;
    public GameObject[] prefabs; // 해당 타입의 프리팹들 (구조물 방 등 다변화를 위해 배열 사용)
}

[System.Serializable]
public class Cell // 맵 전체 격자
{
    public bool visited = false; // 방문여분 하기

    // 문의 상태 [0]:상, [1]:하, [2]:우, [3]:좌
    // true면 벽이 뚫림, false면 막혀있음
    public bool[] status = new bool[4];

    // 방 병합
    public bool[] merged = new bool[4];

    public RoomType type = RoomType.Monster;
}

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Setting")]
    [SerializeField] private Vector2Int size = new Vector2Int(10, 10); // 던전의 가로세로 크기
    [SerializeField] private Vector2Int startPos = new Vector2Int(0, 0); // 시작 좌표


    [Header("Prefab Setting")]
    [SerializeField] private List<RoomPrefabData> _roomPrefabs;
    [SerializeField] private Vector2 _offset = new Vector2(3f, 3f); // 방의 실제 가로/세로 크기
    [SerializeField] private NavMeshSurface _navMeshSurface;
    [SerializeField] private MonsterSpawnManager _spawnManager;


    // 맵 전체 데이터를 담을 2차원 표 (X, Y 좌표로 직관적 접근 가능)
    [SerializeField] private Cell[,] board;
    private Vector2Int _exitRoom;

    // 최적화용 변수
    private readonly Vector2Int[] _neighborsBuffer = new Vector2Int[4];
    private System.Random _random = new System.Random();

    void Awake()
    {
        // 모든 로직을 코루틴으로 실행하여 프레임 저하 및 멈춤 방지
        StartCoroutine(GenerateDungeonRoutine());
    }

    IEnumerator GenerateDungeonRoutine()
    {
        // 초기화
        InitializeBoard();

        // DFS 탐색 (데이터 생성)
        yield return StartCoroutine(GenerateDFSRoutine());

        // 방 타입 설정 및 병합 로직
        RoomTypesSet();
        MergeRooms();

        // 프리팹 실체화 (가장 무거운 작업)
        yield return StartCoroutine(GenerateDungeonPrefabRoutine());

        // 네브메쉬
        BakeNavMesh();

        // 몬스터 스폰 (베이크 완료 후)
        if (_spawnManager != null)
        {
            _spawnManager.SpawnStageMonsters();
        }
    }

    void InitializeBoard()
    {
        board = new Cell[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                board[x, y] = new Cell();
            }
        }
    }

    IEnumerator GenerateDFSRoutine()
    {
        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        Vector2Int currentCell = startPos;
        int maxDepth = 0;
        int stepCount = 0;

        while (true)
        {
            board[currentCell.x, currentCell.y].visited = true;

            if (pathStack.Count > maxDepth)
            {
                maxDepth = pathStack.Count;
                _exitRoom = currentCell;
            }

            // 매번 List를 생성하지 않고 고정 배열 버퍼를 사용
            int neighborCount = CheckNeighborsNonAlloc(currentCell);

            if (neighborCount > 0)
            {
                pathStack.Push(currentCell);

                // UnityEngine.Random 대신 System.Random 사용 (성능 우위)
                int randomIndex = _random.Next(0, neighborCount);
                Vector2Int nextCell = _neighborsBuffer[randomIndex];

                KnockDownWall(currentCell, nextCell);
                currentCell = nextCell;
            }
            else if (pathStack.Count > 0)
            {
                currentCell = pathStack.Pop();
            }
            else
            {
                break; // 탐색 완료
            }

            // 100스텝마다 한 프레임씩 쉬어줌으로써 Safety Loop 역할 대체
            if (++stepCount % 100 == 0) yield return null;
        }
    }

    int CheckNeighborsNonAlloc(Vector2Int cell)
    {
        int count = 0;
        // 상
        if (cell.y + 1 < size.y && !board[cell.x, cell.y + 1].visited)
            _neighborsBuffer[count++] = new Vector2Int(cell.x, cell.y + 1);
        // 하
        if (cell.y - 1 >= 0 && !board[cell.x, cell.y - 1].visited)
            _neighborsBuffer[count++] = new Vector2Int(cell.x, cell.y - 1);
        // 우
        if (cell.x + 1 < size.x && !board[cell.x + 1, cell.y].visited)
            _neighborsBuffer[count++] = new Vector2Int(cell.x + 1, cell.y);
        // 좌
        if (cell.x - 1 >= 0 && !board[cell.x - 1, cell.y].visited)
            _neighborsBuffer[count++] = new Vector2Int(cell.x - 1, cell.y);

        return count;
    }

    void RoomTypesSet()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (!board[x, y].visited) continue;

                if (x == startPos.x && y == startPos.y)
                    board[x, y].type = RoomType.Start;
                else if (x == _exitRoom.x && y == _exitRoom.y)
                    board[x, y].type = RoomType.Escape;
                else
                    board[x, y].type = (Random.value > 0.3f) ? RoomType.Monster : RoomType.Structure;
            }
        }
    }

    void MergeRooms()
    {
        // 1. 2x2 병합
        for (int x = 0; x < size.x - 1; x++)
        {
            for (int y = 0; y < size.y - 1; y++)
            {
                if (board[x, y].visited && board[x + 1, y].visited &&
                    board[x, y + 1].visited && board[x + 1, y + 1].visited)
                {
                    if (IsProtectedRoom(x, y) || IsProtectedRoom(x + 1, y) ||
                        IsProtectedRoom(x, y + 1) || IsProtectedRoom(x + 1, y + 1)) continue;

                    if (Random.value < 0.05f)
                    {
                        board[x, y].merged[2] = true; board[x, y].merged[0] = true;
                        board[x + 1, y].merged[3] = true; board[x + 1, y].merged[0] = true;
                        board[x, y + 1].merged[2] = true; board[x, y + 1].merged[1] = true;
                        board[x + 1, y + 1].merged[3] = true; board[x + 1, y + 1].merged[1] = true;
                    }
                }
            }
        }

        // 2. 가로 통로 (CorridorH)
        for (int x = 0; x < size.x - 1; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (board[x, y].visited && board[x + 1, y].visited &&
                    board[x, y].status[2] && board[x + 1, y].status[3])
                {
                    if (IsProtectedRoom(x, y) || IsProtectedRoom(x + 1, y)) continue;

                    if (!board[x, y].status[0] && !board[x, y].status[1] &&
                        !board[x + 1, y].status[0] && !board[x + 1, y].status[1])
                    {
                        board[x, y].type = RoomType.CorridorH;
                        board[x + 1, y].type = RoomType.CorridorH;
                        board[x, y].merged[2] = true;
                        board[x + 1, y].merged[3] = true;
                    }
                }
            }
        }

        // 3. 세로 통로 (CorridorV)
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y - 1; y++)
            {
                if (board[x, y].visited && board[x, y + 1].visited &&
                    board[x, y].status[0] && board[x, y + 1].status[1])
                {
                    if (IsProtectedRoom(x, y) || IsProtectedRoom(x, y + 1)) continue;

                    if (!board[x, y].status[2] && !board[x, y].status[3] &&
                        !board[x, y + 1].status[2] && !board[x, y + 1].status[3])
                    {
                        board[x, y].type = RoomType.CorridorV;
                        board[x, y + 1].type = RoomType.CorridorV;
                        board[x, y].merged[0] = true;
                        board[x, y + 1].merged[1] = true;
                    }
                }
            }
        }
    }

    IEnumerator GenerateDungeonPrefabRoutine()
    {
        List<Transform> allSpawnPoints = new List<Transform>();
        int instantiateCount = 0;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (!board[x, y].visited) continue;

                GameObject prefab = GetRandomPrefabByType(board[x, y].type);
                if (prefab != null)
                {
                    Vector3 spawnPos = new Vector3(x * _offset.x, 0, y * _offset.y);
                    GameObject newRoom = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                    newRoom.name = $"Room_{x}_{y}";

                    if (newRoom.TryGetComponent<RoomSetting>(out var roomSetting))
                    {
                        roomSetting.UpdateRoom(board[x, y].status, board[x, y].merged);
                        allSpawnPoints.AddRange(roomSetting.GetSpawnPoints());
                    }
                }

                // 한 프레임에 너무 많은 배치가 일어나지 않도록 조절
                if (++instantiateCount % 10 == 0) yield return null;
            }
        }

        if (_spawnManager != null) _spawnManager.SetSpawnPoints(allSpawnPoints);
    }


    bool IsProtectedRoom(int x, int y) => board[x, y].type == RoomType.Start || board[x, y].type == RoomType.Escape;

    void KnockDownWall(Vector2Int current, Vector2Int next)
    {
        int dx = next.x - current.x;
        int dy = next.y - current.y;
        if (dy == 1) { board[current.x, current.y].status[0] = true; board[next.x, next.y].status[1] = true; }
        else if (dy == -1) { board[current.x, current.y].status[1] = true; board[next.x, next.y].status[0] = true; }
        else if (dx == 1) { board[current.x, current.y].status[2] = true; board[next.x, next.y].status[3] = true; }
        else if (dx == -1) { board[current.x, current.y].status[3] = true; board[next.x, next.y].status[2] = true; }
    }

    GameObject GetRandomPrefabByType(RoomType type)
    {
        var data = _roomPrefabs.Find(x => x.type == type);
        if (data.prefabs != null && data.prefabs.Length > 0)
            return data.prefabs[Random.Range(0, data.prefabs.Length)];
        return null;
    }

    void BakeNavMesh()
    {
        if (_navMeshSurface != null) _navMeshSurface.BuildNavMesh();
    }

    /*void Awake()
    {
        
        GenerateDFS();
        RoomTypesSet();
        MergeRooms();
        GenerateDungeonPrefab();
        BakeNavMesh();

        if (_spawnManager != null)
        {
            _spawnManager.SpawnStageMonsters();
        }
    }

    void GenerateDFS()
    {
        // 맵 전체 격자(Cell) 빈 공간으로 초기화
        board = new Cell[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                board[x, y] = new Cell();
            }
        }

        // DFS 탐색을 했던 위치를 위한 스택
        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        // 탐색기 현재 위치 설정
        Vector2Int currentCell = startPos;


        int maxDepth = 0;
        int safetyLoop = 0; // 유니티가 멈추는 것을 방지하기 위한 안전장치

        // 미로 파기 시작 (막다른 길에서 스택이 텅 빌 때까지 무한 반복)
        while (safetyLoop < 10000)
        {
            safetyLoop++;
            board[currentCell.x, currentCell.y].visited = true; // 현재 방 방문 여부 체크

            if (pathStack.Count > maxDepth)
            {
                maxDepth = pathStack.Count;
                _exitRoom = currentCell;
            }

            // 현재 방 주변 상하좌우 중 '안 가본' 이웃 방들의 좌표 가져오기
            List<Vector2Int> neighbors = CheckNeighbors(currentCell);

            // 갈 수 있는 이웃 방이 하나라도 있으면 전진
            if (neighbors.Count > 0)
            {
                // 나중에 돌아올 수 있게 현재 위치를 스택(주머니)에 킵해둠
                pathStack.Push(currentCell);

                // 갈 수 있는 이웃 중 무작위로 하나를 뽑기
                int randomIndex = Random.Range(0, neighbors.Count);
                Vector2Int nextCell = neighbors[randomIndex];

                // 현재 방과 다음 방 사이의 벽을 허물기 (양쪽 문의 status를 true)
                KnockDownWall(currentCell, nextCell);

                // 탐색 이동
                currentCell = nextCell;
            }
            // 주변에 길이 없고, 스택(주머니)에 돌아갈 과거의 방이 있을 때 (후퇴/백트래킹)
            else if (pathStack.Count > 0)
            {
                // 스택에서 가장 최근에 지나온 방을 꺼내서 거기로 되돌아감
                currentCell = pathStack.Pop();
            }
            // 모든곳을 탐색하면 탐색 종료
            else
            {
                Debug.Log("맵 생성 완료");
                break;
            }
        }

    }
    List<Vector2Int> CheckNeighbors(Vector2Int cell)
    {
        //'안 가본' 이웃 방들의 좌표 저장
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // 상 (Up): Y좌표가 1 증가. 배열의 최대 크기(size.y)를 넘지 않아야 함.
        if (cell.y + 1 < size.y && !board[cell.x, cell.y + 1].visited)
            neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

        // 하 (Down): Y좌표가 1 감소. 0보다 작아지면 안 됨.
        if (cell.y - 1 >= 0 && !board[cell.x, cell.y - 1].visited)
            neighbors.Add(new Vector2Int(cell.x, cell.y - 1));

        // 우 (Right): X좌표가 1 증가
        if (cell.x + 1 < size.x && !board[cell.x + 1, cell.y].visited)
            neighbors.Add(new Vector2Int(cell.x + 1, cell.y));

        // 좌 (Left): X좌표가 1 감소
        if (cell.x - 1 >= 0 && !board[cell.x - 1, cell.y].visited)
            neighbors.Add(new Vector2Int(cell.x - 1, cell.y));

        return neighbors; // 갈 수 있는 방향 목록을 반환
    }

    void RoomTypesSet()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (!board[x, y].visited) continue;

                if (x == startPos.x && y == startPos.y)
                {
                    board[x, y].type = RoomType.Start;
                }
                else if (x == _exitRoom.x && y == _exitRoom.y)
                {
                    board[x, y].type = RoomType.Escape;
                }
                else
                {
                    board[x, y].type = Random.value > 0.3f ? RoomType.Monster : RoomType.Structure;
                }
            }
        }
    }

    void KnockDownWall(Vector2Int current, Vector2Int next)
    {
        // 다음 방의 좌표에서 현재 방의 좌표를 빼면, 어느 방향으로 이동했는지 나옵니다.
        int dx = next.x - current.x;
        int dy = next.y - current.y;

        if (dy == 1) // 위로 이동
        {
            board[current.x, current.y].status[0] = true; // 내 방의 '위쪽' 문을 연다.
            board[next.x, next.y].status[1] = true;       // 다음 방의 '아래쪽' 문을 연다.
        }
        else if (dy == -1) // 아래로 이동
        {
            board[current.x, current.y].status[1] = true;
            board[next.x, next.y].status[0] = true;
        }
        else if (dx == 1) // 오른쪽으로 이동
        {
            board[current.x, current.y].status[2] = true;
            board[next.x, next.y].status[3] = true;
        }
        else if (dx == -1) // 왼쪽으로 이동
        {
            board[current.x, current.y].status[3] = true;
            board[next.x, next.y].status[2] = true;
        }
    }

    void GenerateDungeonPrefab()
    {
        List<Transform> allSpawnPoints = new List<Transform>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // 데이터 상으로 방문했던 방인지 확인 
                if (board[x, y].visited)
                {
                    RoomType currentType = board[x, y].type;
                    GameObject prefabToInstantiate = GetRandomPrefabByType(currentType);

                    if (prefabToInstantiate == null)
                    {
                        Debug.LogError($"{currentType} 타입의 프리팹이 누락");
                        continue;
                    }

                    // 유니티 3D 공간의 실제 좌표 계산 (배열 인덱스 * 방 크기)
                    Vector3 spawnPos = new Vector3(x * _offset.x, 0, y * _offset.y);

                    // 방 프리팹 생성
                    GameObject newRoom = Instantiate(prefabToInstantiate, spawnPos, Quaternion.identity, transform);
                    newRoom.name = $"Room_{x}_{y}";

                    // 생성된 방에게 '문/벽 뚫기 데이터' 전달
                    RoomSetting roomSetting = newRoom.GetComponent<RoomSetting>();
                    if (roomSetting != null)
                    {
                        // 배열에 저장해둔 이 방의 status(0:상, 1:하, 2:우, 3:좌)를 넘겨줌
                        roomSetting.UpdateRoom(board[x, y].status, board[x, y].merged);

                        allSpawnPoints.AddRange(roomSetting.GetSpawnPoints());
                    }
                }
            }
        }
        if (_spawnManager != null)
        {
            _spawnManager.SetSpawnPoints(allSpawnPoints);
        }
    }

    GameObject GetRandomPrefabByType(RoomType type)
    {
        var data = _roomPrefabs.Find(x => x.type == type);
        if (data.prefabs != null && data.prefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, data.prefabs.Length);
            return data.prefabs[randomIndex];
        }
        return null;
    }
    void MergeRooms()
    {
        // 2x2 거대 방 병합 (기존 유지)
        for (int x = 0; x < size.x - 1; x++)
        {
            for (int y = 0; y < size.y - 1; y++)
            {
                if (board[x, y].visited && board[x + 1, y].visited &&
                    board[x, y + 1].visited && board[x + 1, y + 1].visited)
                {
                    if (IsProtectedRoom(x, y) || IsProtectedRoom(x + 1, y) ||
                        IsProtectedRoom(x, y + 1) || IsProtectedRoom(x + 1, y + 1))
                        continue;

                    if (Random.value < 0.05f)
                    {
                        board[x, y].merged[2] = true; board[x, y].merged[0] = true;
                        board[x + 1, y].merged[3] = true; board[x + 1, y].merged[0] = true;
                        board[x, y + 1].merged[2] = true; board[x, y + 1].merged[1] = true;
                        board[x + 1, y + 1].merged[3] = true; board[x + 1, y + 1].merged[1] = true;
                    }
                }
            }
        }

        // 가로로 좁고 긴 통로 (CorridorH) 생성
        for (int x = 0; x < size.x - 1; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // 두 방이 가로로 연결되어 있는지 확인 (우측, 좌측 문 열림)
                if (board[x, y].visited && board[x + 1, y].visited &&
                    board[x, y].status[2] && board[x + 1, y].status[3])
                {
                    if (IsProtectedRoom(x, y) || IsProtectedRoom(x + 1, y)) continue;

                    // 가로 일자길인지 검증 (위아래가 다 막혀있어야 함)
                    if (!board[x, y].status[0] && !board[x, y].status[1] &&
                        !board[x + 1, y].status[0] && !board[x + 1, y].status[1])
                    {
                        if (Random.value < 1f) // 확률 적용
                        {
                            // 두 방을 모두 '가로 통로' 프리팹으로 교체
                            board[x, y].type = RoomType.CorridorH;
                            board[x + 1, y].type = RoomType.CorridorH;

                            // 두 좁은 통로 사이에 문/벽 오브젝트가 거추장스럽게 생기지 않도록 내부 병합 처리
                            board[x, y].merged[2] = true;
                            board[x + 1, y].merged[3] = true;
                        }
                    }
                }
            }
        }

        // 세로로 좁고 긴 통로 (CorridorV) 생성
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y - 1; y++)
            {
                // 두 방이 세로로 연결되어 있는지 확인 (상단, 하단 문 열림)
                if (board[x, y].visited && board[x, y + 1].visited &&
                    board[x, y].status[0] && board[x, y + 1].status[1])
                {
                    if (IsProtectedRoom(x, y) || IsProtectedRoom(x, y + 1)) continue;

                    // 세로 일자길인지 검증 (좌우가 다 막혀있어야 함)
                    if (!board[x, y].status[2] && !board[x, y].status[3] &&
                        !board[x, y + 1].status[2] && !board[x, y + 1].status[3])
                    {
                        if (Random.value < 1f)
                        {
                            // 두 방을 모두 '세로 통로' 프리팹으로 교체
                            board[x, y].type = RoomType.CorridorV;
                            board[x, y + 1].type = RoomType.CorridorV;

                            board[x, y].merged[0] = true;
                            board[x, y + 1].merged[1] = true;
                        }
                    }
                }
            }
        }
    }

    // Start 방과 Escape 방 보호 메서드
    bool IsProtectedRoom(int x, int y)
    {
        return board[x, y].type == RoomType.Start || board[x, y].type == RoomType.Escape;
    }


    void BakeNavMesh()
    {
        if (_navMeshSurface != null)
        {
            _navMeshSurface.BuildNavMesh();
            Debug.Log("던전 NavMesh 베이크 완료");
        }
    }*/
}
