using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerEnabler : AIStateMachineLink
{
    public bool         OnEnter = false;
    public bool         OnExit  = false;


    // OnStateEnter
    // Called prior to the first frame the animation assigned to this state.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine)
            _stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnEnter);
    }


    // OnStateExit
    // Called on the last frame of the animator prior to leaving the state.
    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
            _stateMachine.SetLayerActive(animator.GetLayerName (layerIndex), OnExit);
    }


}
