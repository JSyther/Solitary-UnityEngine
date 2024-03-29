using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] string _parameter                 = "";
    [SerializeField] int    _bloodParticlesBurstAmount = 10;
    [SerializeField] float  _damageAmount              = 0.1f;

    // Private 
    AIStateMachine   _stateMachine        = null;
    Animator         _animator            = null;
    int              _paramaterHash       = -1;
    GameSceneManager _gameSceneManager    = null;

    void Start()
    {
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        if (_stateMachine!=null)
            _animator = _stateMachine.animator;

        // Generate parameter hash for more efficient paramter looksups from the animator.
        _paramaterHash = Animator.StringToHash ( _parameter);

        _gameSceneManager = GameSceneManager.instance;

    }


    void OnTriggerStay(Collider col)
    {
        // If we don't have animator return
        if(!_animator)
            return;


        // If this is the player and our parameter is set for damage 
        if ( col.gameObject.CompareTag("Player") && _animator.GetFloat(_paramaterHash) > 0.9f)
        {
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles)
            {
                ParticleSystem system = GameSceneManager.instance.bloodParticles;

                // Temporary Code
                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                var settings = system.main;
                settings.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit ( _bloodParticlesBurstAmount);
            }

            if(_gameSceneManager!= null)
            {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo (col.GetInstanceID());
                if(info!=null && info.characterManager!=null)
                {
                    info.characterManager.TakeDamage( _damageAmount );
                }
            }
        }
    }
}
