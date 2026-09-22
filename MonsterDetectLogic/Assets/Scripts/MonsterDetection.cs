using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDetection : MonoBehaviour
{
    [SerializeField] private float _DetectAngle;
    [SerializeField] private float _offsetYPosition;
    [SerializeField] private float _moveSpeed;
    
    private SphereCollider _collider;
    private Transform _transformInTrigger;
    private Vector3 _monsterRayPoint;
    private Vector3 _targetRayPoint;
    private Vector3 _rayDirection;
    private float _detectRange;
    private Transform _monsterPostion;
    private bool _remainPosition;
    private Vector3 _lastPostion;
    

    private void Awake() => CacheComponents();

    private void Start()
    {
        _detectRange = _collider.radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _transformInTrigger = other.transform;
            _remainPosition = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _lastPostion = other.transform.position;
            _remainPosition = true;
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
        if (_transformInTrigger == null && !_remainPosition) return; // 처음 시작하여 플레이어 정보가 없을때

        if (_remainPosition) // 플레이어 감지가 끊기고 마지막위치로 이동할때
        {
            MoveRemainPosition();

            if (_monsterPostion.position == _lastPostion)
            {
                _remainPosition = false;
            }
            return;
        }
        
        
        if(IsPlayerInDetectRange(_transformInTrigger) && IsRaycastReached(_transformInTrigger))
        {
            MoveMonster();
        }
        else
        {
            // 패트롤
        }
    }

    /// <summary>
    /// 플레이어 추적함수
    /// </summary>
    private void MoveMonster()
    {
        Vector3 dir = _transformInTrigger.position - _monsterPostion.position;
            
        _monsterPostion.Translate(dir.normalized * _moveSpeed * Time.deltaTime, Space.World);
        
        _monsterPostion.LookAt(_transformInTrigger);
    }

    /// <summary>
    /// 플레이어가 부채꼴 영역 밖으로 벗어나
    /// 플레이어 마지막 위치로 이동하는 함수
    /// </summary>
    private void MoveRemainPosition()
    {
        Vector3 dir = (_lastPostion -  _monsterPostion.position).normalized;
        _monsterPostion.position += dir * _moveSpeed * Time.deltaTime;
    }
    
    private bool IsPlayerInDetectRange(Transform TriggerTransform)
    {
        Vector3 vectorToTarget = (TriggerTransform.position - transform.position).normalized;

        float targetDot = Vector3.Dot(transform.forward, vectorToTarget); // 타겟과의 벡터내적값 연산

        float threshold = Mathf.Cos(_DetectAngle * 0.5f * Mathf.Deg2Rad); // 기준 벡터내적값
        
        
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
        
        Ray ray = new Ray(_monsterRayPoint, _rayDirection);
        RaycastHit hit; 
        
        if(Physics.Raycast(ray, out hit, _detectRange))
        {
            if (!hit.transform.CompareTag("Player")) return false;
            
            return true;
        }
        return false;
    }
    
    
    // 에디터 시각화
    // --------------------
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,_detectRange);
        
        // 부채꼴
        if(_transformInTrigger == null) return;
        if (!IsPlayerInDetectRange(_transformInTrigger)) return;
        
        Vector3 leftDir = Quaternion.Euler(0f,-_DetectAngle / 2 , 0f) * _monsterPostion.forward;
        Vector3 rightDir = Quaternion.Euler(0f,_DetectAngle / 2 , 0f) * _monsterPostion.forward;
        
        Gizmos.DrawRay(transform.position, leftDir * _detectRange);
        Gizmos.DrawRay(transform.position, rightDir * _detectRange);
        
        
        // 레이캐스트
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_monsterRayPoint, _rayDirection * _detectRange);
    }
}
