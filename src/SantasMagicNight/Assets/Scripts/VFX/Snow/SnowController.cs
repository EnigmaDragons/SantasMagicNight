using UnityEngine;

public class SnowController : OnMessage<EnableSnowRequested, DisableSnowRequested>
{
    [SerializeField] private GameObject snow;
    
    protected override void Execute(EnableSnowRequested msg)
    {
        Debug.Log("Enable Snow Requested");
        snow.SetActive(true);
    }

    protected override void Execute(DisableSnowRequested msg)
    {
        Debug.Log("Disable Snow Requested");
        snow.SetActive(false);
    }
}
