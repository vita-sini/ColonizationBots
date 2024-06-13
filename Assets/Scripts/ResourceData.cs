using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceData : MonoBehaviour
{
    private Dictionary<Resource, bool> _resourceStates = new Dictionary<Resource, bool>();

    public bool IsResourceBusy(Resource resource)
    {
        if (_resourceStates.ContainsKey(resource))
        {
            return _resourceStates[resource];
        }
        else
        {
            _resourceStates.Add(resource, false);
            return false;
        }
    }

    public void OccupyResource(Resource resource)
    {
        if (_resourceStates.ContainsKey(resource))
        {
            _resourceStates[resource] = true;
        }
        else
        {
            _resourceStates.Add(resource, true);
        }
    }

    public void ReleaseResource(Resource resource)
    {
        if (_resourceStates.ContainsKey(resource))
        {
            _resourceStates[resource] = false;
        }
    }
}
