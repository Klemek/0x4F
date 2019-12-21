using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace GameObjects
{
    public class FpsPlayerController : MonoBehaviour
    {
        private Camera _playerCamera;

        [Header("General")] public float gravityDownForce = 10f;
        public LayerMask groundCheckLayers;
        public float groundCheckDistance = 1f;

        public float PlayerRadius => _controller.radius;

        [Header("Movement")] public float maxSpeedOnGround = 13f;
        public float movementSharpnessOnGround = 15f;
        public float maxSpeedInAir = 25f;
        public float accelerationSpeedInAir = 25f;
        public float sprintSpeedModifier = 1.5f;

        [Header("Rotation")] public float rotationSpeed = 200f;
        public float maxVerticalRotation = 89f;


        [Header("Jump")] public float jumpForce = 9f;
        public bool isGrounded;

        [Header("Audio")] public AudioSource playerMovement;
        public float pitchRange = .1f;
        // ReSharper disable InconsistentNaming

        public float footstepSFXFrequency = 0.3f;
        public float footstepSFXFrequencyWhileSprinting = 0.4f;
        public AudioClip[] footstepSFX;
        public AudioClip[] jumpSFX;

        public AudioClip[] landSFX;
        // ReSharper restore InconsistentNaming

        public AudioSource playerFalling;
        public float minVelocityVolume = 5;
        public float maxVelocityVolume = 30;

        private GameManager _gameManager;
        private CharacterController _controller;
        private Vector3 _groundNormal;
        private Vector3 _characterVelocity;
        private float _lastTimeJumped;
        private float _cameraVerticalAngle;
        private float _footstepDistanceCounter;

        private const float JumpGroundingPreventionTime = 0.2f;
        private const float GroundCheckDistanceInAir = 0.1f;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _controller = GetComponent<CharacterController>();
            _playerCamera = GetComponentInChildren<Camera>();
        }

        private void FixedUpdate()
        {
            if (_gameManager.paused) return;

            var wasGrounded = isGrounded;
            GroundCheck();

            // landing
            if (isGrounded && !wasGrounded)
            {
                PlayRandomSFX(landSFX);
                _footstepDistanceCounter = 0;
            }

            HandleMovement();
        }

        private void LateUpdate()
        {
            if (_gameManager.paused)
            {
                playerFalling.Pause();
                return;
            }

            HandleCamera();
            if (!isGrounded && _controller.velocity.y < -minVelocityVolume)
            {
                if (!playerFalling.isPlaying)
                    playerFalling.Play();
                playerFalling.volume = Mathf.Min((minVelocityVolume - _controller.velocity.y) / maxVelocityVolume, 1f);
            }
            else if (playerFalling.isPlaying)
            {
                playerFalling.Stop();
            }
        }

        private void GroundCheck()
        {
            // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
            var chosenGroundCheckDistance =
                isGrounded ? (_controller.skinWidth + groundCheckDistance) : GroundCheckDistanceInAir;

            // reset values before the ground check
            isGrounded = false;
            _groundNormal = Vector3.up;

            // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
            if (!(Time.time >= _lastTimeJumped + JumpGroundingPreventionTime)) return;
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (!Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(_controller.height),
                _controller.radius, Vector3.down, out var hit, chosenGroundCheckDistance, groundCheckLayers,
                QueryTriggerInteraction.Ignore)) return;
            // storing the upward direction for the surface found
            _groundNormal = hit.normal;

            // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
            // and if the slope angle is lower than the character controller's limit
            if (!(Vector3.Dot(hit.normal, transform.up) > 0f) || !IsNormalUnderSlopeLimit(_groundNormal)) return;
            isGrounded = true;

            // handle snapping to the ground
            if (hit.distance > _controller.skinWidth)
            {
                _controller.Move(Vector3.down * hit.distance);
            }
        }

        private void HandleCamera()
        {
            // horizontal character rotation
            // rotate the transform with the input speed around its local Y axis
            transform.Rotate(new Vector3(0f, InputManager.GetCameraXAxis() * rotationSpeed, 0f),
                Space.Self);

            // vertical camera rotation
            // add vertical inputs to the camera's vertical angle
            _cameraVerticalAngle += InputManager.GetCameraYAxis() * rotationSpeed;
            // limit the camera's vertical angle to min/max
            _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, -maxVerticalRotation, maxVerticalRotation);
            // apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
            _playerCamera.transform.localEulerAngles = new Vector3(_cameraVerticalAngle, 0, 0);
        }

        private void HandleMovement()
        {
            // character movement handling

            var isSprinting = InputManager.GetKey("Sprint");
            var speedModifier = isSprinting ? sprintSpeedModifier : 1f;
            // converts move input to a world space vector based on our character's transform orientation
            var worldSpaceMoveInput = transform.TransformVector(InputManager.GetMoveInput());

            // handle grounded movement
            if (isGrounded)
            {
                // calculate the desired velocity from inputs, max speed, and current slope
                var targetVelocity = speedModifier * maxSpeedOnGround * worldSpaceMoveInput;
                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, _groundNormal) *
                                 targetVelocity.magnitude;
                // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                _characterVelocity = Vector3.Lerp(_characterVelocity, targetVelocity,
                    movementSharpnessOnGround * Time.deltaTime);

                // jumping
                if (isGrounded && InputManager.GetKey("Jump"))
                {
                    // start by canceling out the vertical component of our velocity
                    _characterVelocity = new Vector3(_characterVelocity.x, 0f, _characterVelocity.z);
                    // then, add the jumpSpeed value upwards
                    _characterVelocity += Vector3.up * jumpForce;
                    // play sound
                    PlayRandomSFX(jumpSFX);
                    // remember last time we jumped because we need to prevent snapping to ground for a short time
                    _lastTimeJumped = Time.time;
                    // Force grounding to false
                    isGrounded = false;
                    _groundNormal = Vector3.up;
                }
                else
                {
                    // footsteps sound
                    // ReSharper disable once InconsistentNaming
                    var chosenFootstepSFXFrequency =
                        (isSprinting ? footstepSFXFrequencyWhileSprinting : footstepSFXFrequency);
                    if (_footstepDistanceCounter >= 1f / chosenFootstepSFXFrequency)
                    {
                        _footstepDistanceCounter = 0f;
                        PlayRandomSFX(footstepSFX);
                    }

                    // keep track of distance traveled for footsteps sound
                    _footstepDistanceCounter += _characterVelocity.magnitude * Time.deltaTime;
                }
            }
            // handle air movement
            else
            {
                // add air acceleration
                _characterVelocity += Time.deltaTime * accelerationSpeedInAir * worldSpaceMoveInput;
                // limit air speed to a maximum, but only horizontally
                var verticalVelocity = _characterVelocity.y;
                var horizontalVelocity = Vector3.ProjectOnPlane(_characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                _characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
                // apply the gravity to the velocity
                _characterVelocity += Time.deltaTime * gravityDownForce * Vector3.down;
            }

            // apply the final calculated velocity value as a character movement
            var capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
            var capsuleTopBeforeMove = GetCapsuleTopHemisphere(_controller.height);
            _controller.Move(_characterVelocity * Time.deltaTime);
            if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, _controller.radius,
                _characterVelocity.normalized, out var hit, _characterVelocity.magnitude * Time.deltaTime, -1,
                QueryTriggerInteraction.Ignore))
            {
                _characterVelocity = Vector3.ProjectOnPlane(_characterVelocity, hit.normal);
            }
        }

        public void Teleport(Vector3 position)
        {
            _controller.enabled = false;
            _controller.transform.position = position;
            // ReSharper disable once Unity.InefficientPropertyAccess
            _controller.enabled = true;
        }

        // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
        private bool IsNormalUnderSlopeLimit(Vector3 normal)
        {
            return Vector3.Angle(transform.up, normal) <= _controller.slopeLimit;
        }

        // Gets the center point of the bottom hemisphere of the character controller capsule    
        private Vector3 GetCapsuleBottomHemisphere()
        {
            var t = transform;
            return t.position + (t.up * _controller.radius);
        }

        // Gets the center point of the top hemisphere of the character controller capsule    
        private Vector3 GetCapsuleTopHemisphere(float atHeight)
        {
            var t = transform;
            return t.position + (t.up * (atHeight - _controller.radius));
        }

        // Gets a reoriented direction that is tangent to a given slope
        private Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            var directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        // ReSharper disable once InconsistentNaming
        private void PlayRandomSFX(IReadOnlyList<AudioClip> list)
        {
            if (list.Count <= 0) return;
            GameUtils.PlayRandomSFX(playerMovement, pitchRange, list);
        }

        public void ResetHorizontalVelocity()
        {
            _characterVelocity = Vector3.Project(_characterVelocity, Vector3.up);
        }
    }
}