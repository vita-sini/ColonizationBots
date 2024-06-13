using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private BaseBot _baseBotPrefab;

    private float _moveSpeed = 15f;
    private float _pickupRange = 0.5f;
    private float _carryDistance = 0.5f;
    private float _buildDistance = 1f;

    private BaseBot _baseBot;
    private Resource _carriedResource;
    private Flag _targetFlag;

    public bool isBusy { get; private set; } = false;

    public void SetDestination(Resource resource)
    {
        if (resource == null)
            return;

        _carriedResource = resource;
        isBusy = true;
        StartCoroutine(MoveToResource());
    }

    public void SetDestination(Flag flag)
    {
        if (flag == null)
            return;

        _targetFlag = flag;
        isBusy = true;
        StartCoroutine(MoveToFlag());
    }

    public void DetachUnit()
    {
        _baseBot.RemoveBot(this);
        _baseBot = null;
        isBusy = false;
    }

    public void SetBaseBot(BaseBot baseBot)
    {
        _baseBot = baseBot;
    }

    private void CreateNewBase()
    {
        _baseBot.RemoveFlag();
        Vector3 newBasePosition = new Vector3(_targetFlag.transform.position.x, 1.01f, _targetFlag.transform.position.z);
        BaseBot newBase = Instantiate(_baseBotPrefab, newBasePosition, Quaternion.identity);
        newBase.SetFlag(_targetFlag);
        newBase.SetUnitCreated();
        newBase.SetFlagPlaced();
        DetachUnit();

        _baseBot = newBase;
        newBase.AddBot(this);

        _targetFlag = null;
    }

    private void PickupResource()
    {
        if (_carriedResource == null)
            return;

        _carriedResource.transform.SetParent(transform);
        _carriedResource.transform.localPosition = Vector3.forward * _carryDistance;
        _carriedResource.transform.localRotation = Quaternion.identity;

        StartCoroutine(MoveToBaseBot());
    }

    private void DropResource()
    {
        if (_carriedResource == null || _baseBot == null)
            return;

        _carriedResource.transform.SetParent(null);
        _baseBot.TakeResource(_carriedResource);
        _carriedResource.Release();
        _carriedResource = null;

        isBusy = false;
    }

    private IEnumerator MoveToResource()
    {
        while (true)
        {
            float distance = Vector3.Distance(transform.position, _carriedResource.transform.position);
            var moveDirection = (_carriedResource.transform.position - transform.position).normalized;

            if (distance > _pickupRange)
            {
                transform.position += moveDirection * _moveSpeed * Time.deltaTime;
            }
            else
            {
                PickupResource();
                break;
            }

            yield return null;
        }
    }

    private IEnumerator MoveToBaseBot()
    {
        if (_carriedResource == null || _baseBot == null)
            yield break;

        while (Vector3.Distance(transform.position, _baseBot.transform.position) > _carryDistance)
        {
            var moveDirection = (_baseBot.transform.position - transform.position).normalized;
            transform.position += moveDirection * _moveSpeed * Time.deltaTime;
            yield return null;
        }

        DropResource();
    }

    private IEnumerator MoveToFlag()
    {
        while (Vector3.Distance(transform.position, _targetFlag.transform.position) > _buildDistance)
        {
            var moveDirection = (_targetFlag.transform.position - transform.position).normalized;
            transform.position += moveDirection * _moveSpeed * Time.deltaTime;
            yield return null;
        }

        CreateNewBase();
    }
}