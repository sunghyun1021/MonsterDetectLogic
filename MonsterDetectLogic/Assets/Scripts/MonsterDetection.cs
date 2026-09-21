using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDetection : MonoBehaviour
{
    [SerializeField] private float _DetectAngle;
    [SerializeField] private float _offsetYPosition;
    
    private SphereCollider _collider;
    private Transform _transformInTrigger;
    private Vector3 _monsterRayPoint;
    private Vector3 _targetRayPoint;
    private Vector3 _rayDirection;
    private float _detectRange => _collider.radius;
    private Transform _monsterPostion;
    

    private void Awake() => CacheComponents();
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _transformInTrigger = other.transform;
            Debug.Log($"OnTriggerEnter{other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _transformInTrigger = null;
        }
    }

    private void Update()
    {
        DetectingPlayer();
    }
    
    private void CacheComponents()
    {
        _collider = GetComponent<SphereCollider>();
        _monsterPostion = transform.parent;
    }


    private void DetectingPlayer()
    {
        if (_transformInTrigger == null)
        {
            Debug.Log("나가심");
            return;
        }

        if (IsPlayerInDetectRange(_transformInTrigger) && IsRaycastReached(_transformInTrigger))
        {
            MoveMonster();
        }
    }

    private void MoveMonster()
    {
        Vector3 dir = _transformInTrigger.position - _monsterPostion.position;
            
        _monsterPostion.Translate(dir.normalized * 5f * Time.deltaTime);
        
        transform.LookAt(_transformInTrigger);
    }
    
    
    private bool IsPlayerInDetectRange(Transform TriggerTransform)
    {
        Vector3 vectorToTarget = (TriggerTransform.position - transform.position).normalized;

        float targetDot = Vector3.Dot(transform.forward, vectorToTarget); // 타겟과의 벡터내적값 연산

        float threshold = Mathf.Cos(_DetectAngle * 0.5f * Mathf.Deg2Rad); // 기준 벡터내적값
        
        Debug.Log($"targetDot : {targetDot}");
        Debug.Log($"threshold : {threshold}");
        Debug.Log($"IsPlayerInDetectRange : {targetDot >= threshold}");
        return (targetDot >= threshold);
    }

    private bool IsRaycastReached(Transform TriggerTransform)
    {
        _monsterRayPoint = new Vector3(
            transform.position.x,
            transform.position.y + _offsetYPosition,
            transform.position.z
        );

        _targetRayPoint = new Vector3(
            TriggerTransform.position.x,
            TriggerTransform.position.y + _offsetYPosition,
            TriggerTransform.position.z
        );

        _rayDirection = (_targetRayPoint - _monsterRayPoint).normalized;
        
        Ray ray = new Ray(transform.position, _rayDirection);
        RaycastHit hit; 
        
        if(Physics.Raycast(ray, out hit, _detectRange))
        {
            //TODO: 플레이어 인식하도록 수정해야 함
            return true;
        }

        return false;
    }
    
    
    // 에디터 시각화
    // --------------------
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,6f);
        
        // 부채꼴
        if(_transformInTrigger == null) return;
        if (!IsPlayerInDetectRange(_transformInTrigger)) return;
        
        Vector3 leftBoundary = DirFromAngle(-_DetectAngle * 0.5f);
        Vector3 rightBoundary = DirFromAngle(_DetectAngle * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * _detectRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * _detectRange);

        
        // 레이캐스트
        if(!IsRaycastReached(_transformInTrigger)) return;
        
        Debug.Log("기즈모 그림?");
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_monsterRayPoint, _rayDirection * 6f);
    }
    
    private Vector3 DirFromAngle(float angleOffsetDegrees)
    {
        float angle = transform.eulerAngles.y + angleOffsetDegrees;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
