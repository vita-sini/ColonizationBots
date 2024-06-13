using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Scanner), typeof(SpawnBot))]
public class BaseBot : MonoBehaviour
{
    private float _resourceCollectionDelay = 0.1f;
    private float _spawnRadius = 3f;
    private int _resourcesForNewBase = 5;
    private int _resourcesForNewBot = 3;
    private int _resourceCount = 0;
    private int _startCountBots = 3;

    private bool _isFlagPlaced = false;
    private bool _isCreatedUnit = false;

    private List<Unit> _bots = new List<Unit>();
    private Dictionary<Resource, bool> _resourceStates = new Dictionary<Resource, bool>();

    private SpawnBot _createBot;
    private Scanner _scanner;
    private Flag _flag;
    private ResourceData _resourceData;

    public event UnityAction<int> ResourcesChanged;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();
        _createBot = GetComponent<SpawnBot>();
        _resourceData = FindObjectOfType<ResourceData>();
    }

    private void Start()
    {
        if (!_isCreatedUnit)
        {
            CreateBot(_startCountBots);
        }

        StartCoroutine(CollectResourcesRoutine());
    }

    public void SetFlagPlaced()
    {
        _isFlagPlaced = false;
    }

    public void AddBot(Unit bot)
    {
        if (bot != null && !_bots.Contains(bot))
        {
            _bots.Add(bot);
        }
    }

    public void RemoveBot(Unit bot)
    {
        if (bot != null)
        {
            _bots.Remove(bot);
        }
    }

    public void SetUnitCreated()
    {
        _isCreatedUnit = true;
    }

    public void SetFlag(Flag flag)
    {
        _flag = flag;
        _isFlagPlaced = true;
    }

    public void TakeResource(Resource resource)
    {

        if (_isFlagPlaced)
        {
            if (_resourceCount >= _resourcesForNewBase)
            {
                SpawnNewBase();
            }
        }
        else
        {
            int countNewBot = _resourceCount / _resourcesForNewBot;

            CeateNewBot(countNewBot);
        }

        _resourceStates[resource] = false;
        _resourceCount++;
        ResourcesChanged?.Invoke(_resourceCount);

        _resourceData.ReleaseResource(resource);
    }

    public void RemoveFlag()
    {
        _isFlagPlaced = false;
        Destroy(_flag.gameObject);
        _flag = null;
    }

    private void SpawnNewBase()
    {
        foreach (Unit bot in _bots)
        {
            if (!bot.isBusy)
            {
                bot.SetDestination(_flag);
                _resourceCount -= _resourcesForNewBase;
                break;
            }
        }
    }

    private void CeateNewBot(int countNewBot)
    {
        if (_resourceCount >= _resourcesForNewBot)
        {
            for (int i = 0; i < countNewBot; i++)
            {
                _resourceCount -= _resourcesForNewBot;
                CreateBot(1);
            }
        }
    }

    private void CreateBot(int startCount)
    {
        for (int i = 0; i < startCount; i++)
        {
            float randomX = Random.Range(-_spawnRadius, _spawnRadius);
            float randomZ = Random.Range(-_spawnRadius, _spawnRadius);
            Vector3 randomPosition = transform.position + new Vector3(randomX, 0, randomZ);
            Unit bot = _createBot.Spawn(randomPosition);
            bot.SetBaseBot(this);
            _bots.Add(bot);
        }
    }

    private IEnumerator CollectResourcesRoutine()
    {
        var waitSeconds = new WaitForSeconds(_resourceCollectionDelay);

        while (true)
        {
            yield return waitSeconds;
            CollectResource();
        }
    }

    private void CollectResource()
    {
        List<Resource> availableResources = _scanner.GetAllResources()
            .Where(resource => !_resourceData.IsResourceBusy(resource))
            .ToList();

        if (availableResources.Count > 0)
        {
            Resource resource = availableResources.First();

            foreach (Unit bot in _bots)
            {
                if (!bot.isBusy)
                {
                    bot.SetDestination(resource);
                    _resourceData.OccupyResource(resource);
                    break;
                }
            }
        }
    }
}
