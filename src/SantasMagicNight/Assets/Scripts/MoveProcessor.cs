using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class MoveProcessor : OnMessage<MoveToRequested>
{
    [SerializeField] private CurrentLevelMap map;
    bool isProcessing = false;
    
    protected override void Execute(MoveToRequested m)
    {        
        if (m.Piece.GetComponent<MovementEnabled>() == null)
            return;

        var movementProposals = map.MovementOptionRules
            .Where(r => m.Piece.GetComponent<MovementEnabled>().Types.Any(t => r.Type == t))
            .Where(x => x.IsPossible(m))
            .Select(x => new MovementProposed(x.Type, m.Piece, m.From, m.To)).ToList();

        foreach (var proposal in movementProposals)
        {
            if (map.MovementRestrictionRules.All(x => x.IsValid(proposal)))
            {
                Message.Publish(new PieceMoved(proposal.Piece, m.From, m.To));
                return;
            }
        }
    }

    public void ProcessLinkable(PieceMoved msg)
    {
        if (!isProcessing) StartCoroutine(ProcessLinkableCoroutine(msg));
    }

    IEnumerator ProcessLinkableCoroutine(PieceMoved msg)
    {
        Debug.Log("Process Linkable Started");
        isProcessing = true;
        RegisterAsLinkable[] linkableObjects = FindObjectsOfType<RegisterAsLinkable>();
        List<RegisterAsLinkable> linkList = new List<RegisterAsLinkable>();
        foreach (RegisterAsLinkable linkableObj in linkableObjects)
        {
            linkList.Add(linkableObj);
        }
        TilePoint origin = msg.To;
        bool isMovementInXAxis = msg.Delta.X == 1 && msg.Delta.Y == 0;
        for(int i = 0; i < linkList.Count; i++)
        {
            for (int j = 0; j < linkList.Count; j++)
            {
                if (!linkList[j].isActiveAndEnabled) continue;
                
                TilePoint destination = new TilePoint((int)linkList[j].transform.localPosition.x, (int)linkList[j].transform.localPosition.y);
                if (origin == destination) continue;

                if (isMovementInXAxis)
                {
                    TilePoint diff = origin - destination;
                    if (diff.Y != 0) continue;
                }
                else
                {
                    TilePoint diff = origin - destination;
                    if (diff.X != 0) continue;
                }

                if (destination.IsAdjacentTo(origin))
                {
                    Debug.Log("Looking at tile: " + destination + " and the origin is " + origin);
                    yield return new WaitForSeconds(0.5f);
                    Message.Publish(new PieceMoved(msg.Piece, origin, destination));

                    if (linkList[j].gameObject.activeInHierarchy)
                    {
                        Message.Publish(new ObjectDestroyed(linkList[j].gameObject, false));
                    }

                    origin = destination;
                    break;
                }
            }
        }
        isProcessing = false;
        Debug.Log("Process Linkable Ended");
    }
}
