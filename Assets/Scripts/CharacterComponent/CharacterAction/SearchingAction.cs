using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterComponent.CharacterAction
{
    [System.Serializable]
    public struct CastInfo
    {
        public bool Hit;                // ** 맞았는지 확인
        public Vector3 Point;           // ** 맞았다면 맞은 위치, 안맞았다면 Range 거리
        public float Distance;          // ** 도달 거리
        public float Angle;             // ** 각도
    }
    
    public class SearchingAction : MonoBehaviour
    {
        [Header("Circle")]
        [Range(0, 30)]
        [SerializeField] private float viewRange = 15f;               // 시야 범위
        [Range(0, 360)]
        [SerializeField] private float viewAngle = 90f;               // 시야 각도

        [Header("Target")]
        [SerializeField] private LayerMask targetMask;          // 탐색 대상
        [SerializeField] private LayerMask obstacleMask;        // 장애물 대상
        [SerializeField] private List<Transform> targetList;    // 탐색 결과 리스트
    
        [Header("Draw Line")]
        [Range(0.1f, 1f)]
        [SerializeField] private float angle = 1f;                   // 선이 표시될 각도. 작을 수록 선이 촘촘해진다.
        [SerializeField] private List<CastInfo> lineList;       // 표시된 선의 정보 리스트
        [SerializeField] private Vector3 offset;                // 위치 보정용 벡터. zero 로 해도 무관
        
        public Transform Target { get; private set; }
        
        private IEnumerator Start()
        {
            targetList = new List<Transform>();
            lineList = new List<CastInfo>();

            yield return null;
            
            // 함수 실행 순서를 바꾸면 적 탐지 선이 가려져서 지워짐
            StartCoroutine(DrawRayLine());
        }

        public bool CheckTarget()
        {
            targetList.Clear();

            // 원형 범위 내 대상을 검출한다.
            var results = new Collider[1];
            
            Physics.OverlapSphereNonAlloc(transform.position, viewRange, results, targetMask);

            if ( ReferenceEquals(results[0], null) )
                return false;
            
            foreach(var e in results)
            {
                // 검출한 대상의 방향을 구한다.
                var direction = (e.transform.position - transform.position).normalized;

                // 대상과의 각도가 설정한 각도 이내에 있는지 확인한다.
                // viewAngle 은 부채꼴 전체 각도이기 때문에, 0.5를 곱해준다.
                if (Vector3.Angle(transform.forward, direction) < (viewAngle * 0.5f))
                {
                    // 대상과 거리를 측정한다.
                    var distance = Vector3.Distance(transform.position, e.transform.position);

                    // 레이캐스트를 쏴서, 장애물이 있는지 검사한다.
                    if (Physics.Raycast(transform.position, direction, distance, obstacleMask)) 
                        continue;
                    
                    Debug.DrawLine(transform.position + offset, e.transform.position + offset, Color.red);
                    
                    targetList.Add(e.transform);
                }
            }
            
            Target = targetList.Count > 0 ? targetList[0] : null;
            
            return targetList.Count > 0;
        }

        private IEnumerator DrawRayLine()
        {
            while (true)
            {
                lineList.Clear();       // 이미 생성된 레이캐스트 정보는 삭제한다.

                // 선이 표시될 갯수. 시야각에서 선이 표시될 각도를 나눠서 구한다.
                var count = Mathf.RoundToInt(viewAngle / angle) + 1;
                // 가장 오른쪽 각도. 시야각과 플레이어의 방향을 기준으로 결정된다.
                var fAngle = -(viewAngle * 0.5f) + transform.eulerAngles.y;

                // 선이 표시될 갯수만큼 실행한다.
                for (var i = 0; i < count; ++i)
                {
                    // 해당 각도로 발사한 레이캐스트 정보를 가져온다.
                    var info = GetCastInfo(fAngle + (angle * i));
                    lineList.Add(info);

                    // 해당 레이캐스트 정보에 따라 선을 그린다.
                    Debug.DrawLine(transform.position + offset, info.Point, Color.green);
                }

                yield return null;
            }
        }

        private CastInfo GetCastInfo(float angle)
        {
            // 입력받은 각도에 따라 방향을 결정한다.
            var dir = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angle * Mathf.Deg2Rad));
            CastInfo Info;
            RaycastHit hit;

            // 장애물에 맞는지 테스트
            if (Physics.Raycast(transform.position + offset, dir, out hit, viewRange, obstacleMask))
            {

                Info.Hit = true;                // 맞았는지 앉맞았는지 확인
                Info.Angle = angle;            // 각도
                Info.Distance = hit.distance;   // 거리
                Info.Point = hit.point;         // 맞은 위치
            }

            // 장애물에 맞지 않았다면
            else
            {
                Info.Hit = false;               // 맞았는지 앉맞았는지 확인
                Info.Angle = angle;            // 각도
                Info.Distance = viewRange;      // 맞지 않았다면 최대 도달 거리인 Range
                // 맞지 않았다면 해당 방향으로 최대 거리인 Range의 위치
                Info.Point = transform.position + offset + dir * viewRange;
            }

            return Info;
        }
    }
    
}