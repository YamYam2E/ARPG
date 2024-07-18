using System;
using AIComponent.BTContainer;
using AIComponent.BTTask;
using CharacterComponent.CharacterAction;
using UnityEngine;
using UnityEngine.AI;
using Action = AIComponent.BTTask.Action;
using Random = UnityEngine.Random;

namespace AIComponent.Test
{
    public class TestObject : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private SearchingAction searchingAction;
        
        private Root _aiRoot;
        private BlackBoard _blackBoard = new();
        
        private bool _dirty;
        private float _totalTime;
        
        private void Start()
        {
            _totalTime = Random.Range(3f, 10f);
            Debug.LogError($"Make Total Time : {_totalTime}");
            
            _aiRoot = new Root(
                new Sequence(
                    new Condition(CheckTimer),
                    new Sequence(
                        new Action(SearchingPlayer),
                        new Action(FollowPlayer)
                    )
                )
            );

            _aiRoot.StartRoot();
        }
        
        // random timer check
        private TaskResult CheckTimer()
        {
            _blackBoard.ElapsedTime += 0.125f + Time.deltaTime;
            Debug.Log($"Elapsed Time : {_blackBoard.ElapsedTime}");
            if (_blackBoard.ElapsedTime >= _totalTime)
            {
                _blackBoard.ElapsedTime = 0;
                _totalTime = Random.Range(3f, 10f);
                Debug.LogError($"Make New Total Time : {_totalTime}");
                return TaskResult.Success;
            }

            return TaskResult.Failure;
        }
        
        
        // search player
        private TaskResult SearchingPlayer()
        {
            Debug.Log($"Search Player");
            if (!searchingAction.CheckTarget())
            {
                if( _blackBoard.LastPlayerPosition != Vector3.zero )
                    _dirty = true;
                
                return TaskResult.Failure;
            }
            
            return TaskResult.Success;
        }

        // follow player
        private TaskResult FollowPlayer()
        {
            // Follow player
            var player = searchingAction.Target;
            _blackBoard.LastPlayerPosition = player.position;
            _blackBoard.LastPlayerRotation = player.rotation;
            
            agent.destination = _blackBoard.LastPlayerPosition;
            
            return TaskResult.Success;
        }

        private void ResetLastPlayerData()
        {
            _blackBoard.LastPlayerPosition = Vector3.zero;
            _blackBoard.LastPlayerRotation = Quaternion.identity;
            _dirty = false;
        }

        private void Update()
        {
            if (agent.velocity == Vector3.zero && _dirty)
            {
                Debug.Log("Update Rotation");
                transform.rotation = _blackBoard.LastPlayerRotation;
                ResetLastPlayerData();
            }
        }
    }
}