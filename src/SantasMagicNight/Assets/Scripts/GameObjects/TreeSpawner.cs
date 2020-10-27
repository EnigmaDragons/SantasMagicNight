
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tree;
    [SerializeField] private int minTrees;
    [SerializeField] private int maxTrees;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float minAbsoluteDistance;
    [SerializeField] private bool useSpawner;

    private readonly List<GameObject> _trees = new List<GameObject>();
    private readonly int maxRetries = 5;

    private void Awake()
    {
        if (!useSpawner)
            return;
        
        foreach(Transform obj in transform)
            obj.gameObject.SetActive(false);
        
        var numTrees = Rng.Int(minTrees, maxTrees);
        var tryNum = 0;
        for (var i = 0; i < numTrees; i++)
        {
            var pos = GetNextRandomPosition();
            while (tryNum < maxRetries && _trees.Any(t => Vector3.Distance(pos, t.transform.position) < minAbsoluteDistance))
            {
                tryNum++;
                pos = GetNextRandomPosition();
            }
            
            _trees.Add(Instantiate(tree, pos, Quaternion.identity, transform));
        }
    }

    private Vector3 GetNextRandomPosition() 
        => new Vector3(Random.Range(minValue, maxValue), 0, Random.Range(minValue, maxValue)) + transform.position;
}
