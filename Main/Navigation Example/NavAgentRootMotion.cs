using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// ----------------------------------------------------------
// CLASS    :   NavAgentNoRootMotion
// DESC     :   Behaviour to test Unit^s NavMeshAgent
// ----------------------------------------------------------
[RequireComponent(typeof(NavMeshAgent))] // when drag this script to any object it is automaticly add navmeshagent component if is it null
public class NavAgentRootMotion : MonoBehaviour
{
    // Inspector Assigned Variables 
    public AIWaypointNetwork WayPointNetwork = null;
    public int               CurrentIndex    = 0;
    public bool              hasPath         = false;
    public bool              PathPending     = false;
    public bool              PathStale       = false;
    public NavMeshPathStatus PathStatus      = NavMeshPathStatus.PathInvalid;
    public AnimationCurve    JumpCurve       = new AnimationCurve();
    public bool              MixedMode       = true;

    //Private Members
    private NavMeshAgent _navAgent = null;
    private Animator     _animator = null;
    private float        _smoothAngle = 0;

    //ForDebug Variables 
    public Text Debuglog1;
    public Text Debuglog2;
          
    // ----------------------------------------------------------
    // Name :   Start
    // Desc :   Cache NavMeshAgent and set initial
    //          destination.
    // ----------------------------------------------------------
    void Start()
    {
        //Cache NavMeshAgent reference
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
        /*_navAgent.updatePosition = false;*/
        _navAgent.updateRotation = false;

        // If not valid Waypoint Network has been assigned than return
        if (WayPointNetwork == null) return;

        SetNextDestination(false);
    }

    private void FixedUpdate()
    {
        //Debugging
        //Debuglog1.text = _navAgent.destination.ToString();
        //Debuglog2.text = _navAgent.desiredVelocity.ToString();
    }

    // ---------------------------------------------------------
    // Name	:	Update
    // Desc	:	Called each frame by Unity
    // ---------------------------------------------------------
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Time.timeScale = 0.01f; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Time.timeScale = 0.1f; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Time.timeScale = 1.0f; }

        // Copy NavMeshAgents state into inspector visible variables
        hasPath     = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale   = _navAgent.isPathStale;
        PathStatus  = _navAgent.pathStatus;

        Vector3 localDesiredVelocity = transform.InverseTransformVector(_navAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;
        _smoothAngle = Mathf.MoveTowardsAngle(_smoothAngle, angle, 80.0f * Time.deltaTime);

        float speed = localDesiredVelocity.z;

        _animator.SetFloat("Angle", _smoothAngle);
        _animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        if(_navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon )
        {
            if(!MixedMode || (MixedMode && Mathf.Abs(angle) < 80.0f && _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotoin")))
            {
            Quaternion lookRotation = Quaternion.LookRotation(_navAgent.desiredVelocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
            }
        }


    /*  if (_navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }*/

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

    

    void OnAnimatorMove()
    {
        if(MixedMode && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotoin"))
            transform.rotation = _animator.rootRotation;
            _navAgent.velocity = _animator.deltaPosition / Time.deltaTime;
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
