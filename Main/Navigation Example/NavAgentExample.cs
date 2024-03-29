using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ----------------------------------------------------------
// CLASS    :   NavAgentExample
// DESC     :   Behaviour to test Unit^s NavMeshAgent
// ----------------------------------------------------------
[RequireComponent(typeof(NavMeshAgent))] // when drag this script to any object it is automaticly add navmeshagent component if is it null
public class NavAgentExample : MonoBehaviour
{
    // Inspector Assigned Variables 
    public AIWaypointNetwork WayPointNetwork = null;
    public int               CurrentIndex    = 0;
    public bool              hasPath         = false;
    public bool              PathPending     = false;
    public bool              PathStale       = false;
    public NavMeshPathStatus PathStatus      = NavMeshPathStatus.PathInvalid;
    public AnimationCurve    JumpCurve       = new AnimationCurve();

    //Private Members
    private NavMeshAgent _navAgent = null;
    // ----------------------------------------------------------
    // Name :   Start
    // Desc :   Cache NavMeshAgent and set initial
    //          destination.
    // ----------------------------------------------------------
    void Start()
    {
        //Cache navmeshagent reference
        _navAgent = GetComponent<NavMeshAgent>();

        // If not valid Waypoint Network has been assigned than return
        if (WayPointNetwork == null) return;

        SetNextDestination(false);
    }


    // ----------------------------------------------------------
    // Name : SetNextDestination
    // Desc : Optionally increments the current waypoints
    //        index and then sets the next destination
    //		  for the agent to head towards.
    // -----------------------------------------------------

    void SetNextDestination (bool increment)
    {
        // If no network return
        if (!WayPointNetwork) return;

        // Calculate how much the current waypoint index needs to be incremented
        int       incStep               = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        // Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
        int nextWaypoint        = (CurrentIndex + incStep >= WayPointNetwork.Waypoints.Count)?0:CurrentIndex+incStep;
        nextWaypointTransform   = WayPointNetwork.Waypoints[nextWaypoint];

        // Assuming we have a valid waypoint transform
        if (nextWaypointTransform != null)
        {
            // Update the current waypoint index, assign its position as the NavMeshAgents
            // Destination and then return
            CurrentIndex            = nextWaypoint;
            _navAgent.destination   = nextWaypointTransform.position;
            return;
        } 
        // We did not find a valid waypoint in the list for this iteration
        CurrentIndex++;
    }

    // ---------------------------------------------------------
    // Name	:	Update
    // Desc	:	Called each frame by Unity
    // ---------------------------------------------------------
    void Update()
    {
        // Copy NavMeshAgents state into inspector visible variables
        hasPath     = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale   = _navAgent.isPathStale;
        PathStatus  = _navAgent.pathStatus;


        if (_navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }

        // If we don't have a path and one isn't pending then set the next
        // waypoint as the target, otherwise if path is stale regenerate path
        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid /*||PathStatus==NavMeshPathStatus.PathPartial*/)
        {
            SetNextDestination(true);
        }
        else

        if (_navAgent.isPathStale)
            SetNextDestination(false);
    }

    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data        = _navAgent.currentOffMeshLinkData;
        Vector3         startPos    = _navAgent.transform.position;
        Vector3         endPos      = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float           time        = 0.0f;

        while( time <= duration)
        {
            float t = time/duration;

            _navAgent.transform.position = Vector3.Lerp(startPos, endPos,t) + (JumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }
        _navAgent.CompleteOffMeshLink();
    }
}
