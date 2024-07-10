using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharacterComponent.CharacterAction
{
    public class RollingAction : MonoBehaviour
    {
        private CharacterController _ownerCharacterController;

        private const float RollingSpeedChangeRate = 10f;
        
        private float _rollingSpeed;

        public bool IsRolling { get; private set; }

        public void Initialize(CharacterController characterController)
        {
            _ownerCharacterController = characterController;
        }
        
        /// <summary>
        /// [Animation Event]
        /// </summary>
        private void OnRollingStart()
        {
            Clear();
            IsRolling = true;
        }

        /// <summary>
        /// [Animation Event]
        /// </summary>
        private void OnRollingFinish()
        {
            IsRolling = false;
        }

        private void Clear()
        {
            _rollingSpeed = 0f;
        }

        private void OnAnimatorMove()
        {
            if( IsRolling )
                DoRolling();
        }

        private void DoRolling()
        {
            var targetSpeed = 8f;
            var moveVector = transform.forward;

            if (moveVector == Vector3.zero)
                targetSpeed = 0f;

            const float speedOffset = 0.1f;

            var velocity = _ownerCharacterController.velocity;
            var currentHorizontalSpeed =
                new Vector3(velocity.x, 0.0f, velocity.z).magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _rollingSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * RollingSpeedChangeRate);

                // round speed to 3 decimal places
                _rollingSpeed = Mathf.Round(_rollingSpeed * 1000f) / 1000f;
            }
            else
            {
                _rollingSpeed = targetSpeed;
            }

            // normalise input direction
            var targetDirection = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f) * Vector3.forward;

            // move the player
            _ownerCharacterController.Move(targetDirection.normalized * (_rollingSpeed * Time.deltaTime));
        }
    }
}