using UnityEngine;

public enum ShakeDirectionType
{
    NONE = -1,
    RADIAL,          // 방사형 (피격 방향 기반)
    VERTICAL,        // 수직만 (쿵 내려치는 느낌)
    HORIZONTAL,     // 수평만
    ALLAXIS,        // 전방향 랜덤 (광역기)
    NOISE,          // 잔잔한 노이즈 (패링, 착지)
    MAX
}
