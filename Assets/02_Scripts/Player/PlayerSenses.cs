using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSenses : MonoBehaviour
{
    [SerializeField] float _detectRadius = 1f;
    [SerializeField] float _detectDelay = 0.1f;
    [SerializeField] LayerMask _unitLayerMask;
    [SerializeField] LayerMask _itemLayerMask;
    [SerializeField] LayerMask _interactableLayerMask;
    List<GameObject> _unitList = new();
    List<WorldItem> _worldItemList = new();
    List<GameObject> _interactableList = new();

    Collider[] _unitBuffer = new Collider[64];
    Collider[] _itemBuffer = new Collider[64];
    Collider[] _interactableBuffer = new Collider[5];

    WaitForSeconds _delay;
    
    void Awake()
    {
        _delay = new WaitForSeconds(_detectDelay);
        _unitList.Clear();
        StartCoroutine(Co_UpdateUnitList());
        StartCoroutine(Co_UpdateWorldItemList());
        StartCoroutine(Co_UpdateInteractableList());
    }
    public GameObject GetNearestUnit()
    {
        if (_unitList.Count == 0) return null;

        GameObject nearestUnit = _unitList[0];
        float minDist = Vector3.Distance(transform.position, nearestUnit.transform.position);

        for (int i = 1; i < _unitList.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, _unitList[i].transform.position);
            if (dist < minDist)
            {
                nearestUnit = _unitList[i];
                minDist = dist;
            }
        }

        return nearestUnit;
    }
    public WorldItem GetNearestWorldItem()
    {
        if (_worldItemList.Count == 0) return null;

        GameObject nearestUnit = _worldItemList[0].gameObject;
        float minDist = Vector3.Distance(transform.position, nearestUnit.transform.position);

        for (int i = 1; i < _worldItemList.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, _worldItemList[i].transform.position);
            if (dist < minDist)
            {
                nearestUnit = _worldItemList[i].gameObject;
                minDist = dist;
            }
        }

        return nearestUnit.GetComponent<WorldItem>();
    }

    public IInteractable GetNearestInteractable()
    {
        if (_interactableList.Count == 0) return null;

        GameObject nearestUnit = _interactableList[0];
        float minDist = Vector3.Distance(transform.position, nearestUnit.transform.position);

        for (int i = 1; i < _interactableList.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, _interactableList[i].transform.position);
            if (dist < minDist)
            {
                nearestUnit = _interactableList[i];
                minDist = dist;
            }
        }

        return nearestUnit.GetComponent<IInteractable>();
    }

    IEnumerator Co_UpdateUnitList()
    {
        while(true)
        {
            _unitList.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, _detectRadius, _unitBuffer, _unitLayerMask);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _unitList.Add(_unitBuffer[i].gameObject);
                }
            }
            yield return _delay;
        }
    }
    IEnumerator Co_UpdateWorldItemList()
    {
        while (true)
        {
            _worldItemList.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, _detectRadius, _itemBuffer, _itemLayerMask);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _worldItemList.Add(_itemBuffer[i].GetComponent<WorldItem>());
                }
            }
            yield return _delay;
        }
    }
    IEnumerator Co_UpdateInteractableList()
    {
        while (true)
        {
            _interactableList.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, _detectRadius, _interactableBuffer, _interactableLayerMask);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _interactableList.Add(_interactableBuffer[i].gameObject);
                }
            }
            yield return _delay;
        }
    }
}
