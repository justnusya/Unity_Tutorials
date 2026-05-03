using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Tanks.Complete
{
    [DefaultExecutionOrder(-10)]
    public class TankMovement : MonoBehaviour
    {
        [Tooltip("The player number.")]
        public int m_PlayerNumber = 1;
        [Tooltip("The speed in unity unit/second the tank move at")]
        public float m_Speed = 12f;
        [Tooltip("The speed in deg/s that tank will rotate at")]
        public float m_TurnSpeed = 180f;
        public bool m_IsDirectControl;
        public AudioSource m_MovementAudio;
        public AudioClip m_EngineIdling;
        public AudioClip m_EngineDriving;
        public float m_PitchRange = 0.2f;
        public bool m_IsComputerControlled = false;
        [HideInInspector]
        public TankInputUser m_InputUser;
        
        public Rigidbody Rigidbody => m_Rigidbody;
        public int ControlIndex { get; set; } = -1;
        
        private string m_MovementAxisName;
        private string m_TurnAxisName;
        private Rigidbody m_Rigidbody;
        private float m_MovementInputValue;
        private float m_TurnInputValue;
        private Vector3 m_ExplosionForceValue;
        private float m_OriginalPitch;
        private ParticleSystem[] m_particleSystems;
        
        private InputAction m_MoveAction;
        private InputAction m_TurnAction;
        private InputAction m_JumpAction;

        private Vector3 m_RequestedDirection;

        [Header("Jump Settings")]
        public float m_JumpForce = 15f; 
        public LayerMask m_GroundLayer;
        private bool m_IsGrounded;
        private bool m_JumpRequested;

        private void Awake ()
        {
            m_Rigidbody = GetComponent<Rigidbody> ();
            
            // FIX: Ensure triggers are always detected and the tank stays upright
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Prevent the tank from tipping over, which can cause it to miss PowerUp triggers on the ground
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            m_InputUser = GetComponent<TankInputUser>();
            if (m_InputUser == null)
                m_InputUser = gameObject.AddComponent<TankInputUser>();
        }

        private void OnEnable ()
        {
            m_Rigidbody.isKinematic = false;
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;
            m_ExplosionForceValue = Vector3.zero;
            m_particleSystems = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < m_particleSystems.Length; ++i) m_particleSystems[i].Play();
        }

        private void OnDisable ()
        {
            m_Rigidbody.isKinematic = true;
            for(int i = 0; i < m_particleSystems.Length; ++i) m_particleSystems[i].Stop();
        }

        private void Start ()
        {
            if (m_IsComputerControlled)
            {
                var ai = GetComponent<TankAI>();
                if (ai == null) gameObject.AddComponent<TankAI>();
            }

            if (ControlIndex == -1 && !m_IsComputerControlled) ControlIndex = m_PlayerNumber;
            
            var mobileControl = FindAnyObjectByType<MobileUIControl>();
            if (mobileControl != null && ControlIndex == 1)
            {
                m_InputUser.SetNewInputUser(InputUser.PerformPairingWithDevice(mobileControl.Device));
                m_InputUser.ActivateScheme("Gamepad");
            }
            else
            {
                m_InputUser.ActivateScheme(ControlIndex == 1 ? "KeyboardLeft" : "KeyboardRight");
            }

            m_MovementAxisName = "Vertical";
            m_TurnAxisName = "Horizontal";
            
            m_MoveAction = m_InputUser.ActionAsset.FindAction(m_MovementAxisName);
            m_TurnAction = m_InputUser.ActionAsset.FindAction(m_TurnAxisName);
            m_JumpAction = m_InputUser.ActionAsset.FindAction("Jump");
            
            m_MoveAction.Enable();
            m_TurnAction.Enable();
            m_JumpAction.Enable();
            
            if(m_MovementAudio) m_OriginalPitch = m_MovementAudio.pitch;
        }

        private void Update ()
        {
            // Ground check raycast
            m_IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f, m_GroundLayer);

            if (!m_IsComputerControlled)
            {
                m_MovementInputValue = m_MoveAction.ReadValue<float>();
                m_TurnInputValue = m_TurnAction.ReadValue<float>();

                if (m_JumpAction != null && m_JumpAction.WasPressedThisFrame() && m_IsGrounded)
                {
                    m_JumpRequested = true;
                }
            }
            
            if(m_MovementAudio) EngineAudio ();
        }

        private void FixedUpdate ()
        {
            m_Rigidbody.angularVelocity = Vector3.zero;

            // FIX: Force the Rigidbody to stay awake. 
            // When PowerUps are destroyed, the Rigidbody might sleep, causing it to ignore future triggers.
            if (m_Rigidbody.IsSleeping()) m_Rigidbody.WakeUp();

            if (m_InputUser.InputUser.controlScheme.Value.name == "Gamepad" ||  m_IsDirectControl)
            {
                var camForward = Camera.main.transform.forward;
                camForward.y = 0;
                if (camForward.sqrMagnitude < 0.0001f) { camForward = Camera.main.transform.up; camForward.y = 0; }
                camForward.Normalize();
                var camRight = Vector3.Cross(Vector3.up, camForward);
                m_RequestedDirection = (camForward * m_MovementInputValue + camRight * m_TurnInputValue);
                m_RequestedDirection.Normalize();
            }
            
            Move ();
            Turn ();

            if (m_JumpRequested)
            {
                m_Rigidbody.AddForce(Vector3.up * m_JumpForce, ForceMode.VelocityChange);
                m_JumpRequested = false;
            }
        }

        private void Move ()
        {
            float speedInput = 0.0f;
            if (m_InputUser.InputUser.controlScheme.Value.name == "Gamepad" || m_IsDirectControl)
            {
                speedInput = m_RequestedDirection.magnitude;
                speedInput *= 1.0f - Mathf.Clamp01((Vector3.Angle(m_RequestedDirection, transform.forward) - 90) / 90.0f);
            }
            else
            {
                speedInput = m_MovementInputValue;
            }
            
            Vector3 movement = transform.forward * speedInput * m_Speed;
            
            // Horizontal movement calculation
            Vector3 horizontalMove = (movement + m_ExplosionForceValue) * Time.fixedDeltaTime;
            
            // Vertical movement calculation (preserving gravity/jump velocity)
            Vector3 verticalMove = Vector3.up * m_Rigidbody.linearVelocity.y * Time.fixedDeltaTime;
            
            // MovePosition is used to ensure the physics engine sweeps the volume for triggers
            m_Rigidbody.MovePosition(m_Rigidbody.position + horizontalMove + verticalMove);

            m_ExplosionForceValue = Vector3.Lerp(m_ExplosionForceValue, Vector3.zero, Time.fixedDeltaTime * 3f);
        }

        private void Turn ()
        {
            Quaternion turnRotation;
            if (m_InputUser.InputUser.controlScheme.Value.name == "Gamepad" || m_IsDirectControl)
            {
                float angleTowardTarget = Vector3.SignedAngle(m_RequestedDirection, transform.forward, transform.up);
                var rotatingAngle = Mathf.Sign(angleTowardTarget) * Mathf.Min(Mathf.Abs(angleTowardTarget), m_TurnSpeed * Time.deltaTime);
                turnRotation = Quaternion.AngleAxis(-rotatingAngle, Vector3.up);
            }
            else
            {
                float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
                turnRotation = Quaternion.Euler (0f, turn, 0f);
            }
            m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
        }

        private void EngineAudio ()
        {
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f)
        {
            Vector3 explosionDir = (transform.position - explosionPosition);
            float explosionDistance = explosionDir.magnitude;
            explosionDir.y += upwardsModifier;
            explosionDir.Normalize();
            float attenuation = 1f - Mathf.Clamp01(explosionDistance / explosionRadius);
            m_ExplosionForceValue = explosionDir * (explosionForce * attenuation);
        }
    }
}