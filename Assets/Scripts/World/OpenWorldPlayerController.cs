using UnityEngine;

namespace SandsOfLegends.World
{
    /// <summary>
    /// Lightweight third-person controller for the isolated open-world preview.
    /// Supports a left-side touch joystick and keyboard axes for Editor testing.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class OpenWorldPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = 22f;
        [SerializeField] private Camera followCamera;

        private CharacterController character;
        private Vector3 verticalVelocity;
        private Vector2 joystick;
        private string locationName = "Kingdom Outskirts";
        private const float JoystickRadius = 76f;
        private static readonly Vector2 JoystickCenter = new Vector2(108f, 112f);

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            character.height = 2f;
            character.radius = .38f;
            character.center = new Vector3(0f, 1f, 0f);
            character.stepOffset = .35f;
            character.slopeLimit = 48f;
        }

        public void SetCamera(Camera camera)
        {
            followCamera = camera;
            if (followCamera != null)
            {
                OpenWorldFollowCamera rig = followCamera.GetComponent<OpenWorldFollowCamera>();
                if (rig == null) rig = followCamera.gameObject.AddComponent<OpenWorldFollowCamera>();
                rig.SetTarget(transform);
            }
        }

        private void Update()
        {
            Vector2 input = ReadMovement();
            Vector3 forward = followCamera != null ? followCamera.transform.forward : Vector3.forward;
            Vector3 right = followCamera != null ? followCamera.transform.right : Vector3.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();

            Vector3 move = forward * input.y + right * input.x;
            if (move.sqrMagnitude > 1f) move.Normalize();

            if (character.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
            verticalVelocity.y -= gravity * Time.deltaTime;
            character.Move((move * moveSpeed + verticalVelocity) * Time.deltaTime);

            if (move.sqrMagnitude > .001f)
            {
                Quaternion target = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        private Vector2 ReadMovement()
        {
            Vector2 result = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            joystick = Vector2.zero;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) continue;
                if (touch.position.x > Screen.width * .48f || touch.position.y > Screen.height * .78f) continue;
                Vector2 delta = touch.position - JoystickCenter;
                if (delta.magnitude > JoystickRadius * 1.9f) continue;
                joystick = Vector2.ClampMagnitude(delta / JoystickRadius, 1f);
                break;
            }
            if (joystick.sqrMagnitude > .01f) result = joystick;
            return Vector2.ClampMagnitude(result, 1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            OpenWorldLandmark landmark = other.GetComponent<OpenWorldLandmark>();
            if (landmark != null) locationName = landmark.DisplayName;
        }

        private void OnTriggerExit(Collider other)
        {
            OpenWorldLandmark landmark = other.GetComponent<OpenWorldLandmark>();
            if (landmark != null) locationName = "Kingdom Outskirts";
        }

        private void OnGUI()
        {
            GUI.color = new Color(.06f, .10f, .16f, .78f);
            GUI.Box(new Rect(18f, 18f, 270f, 66f), GUIContent.none);
            GUI.color = new Color(1f, .86f, .48f, 1f);
            GUI.Label(new Rect(32f, 27f, 245f, 24f), "LEGENDS REBORN  •  EXPLORATION");
            GUI.color = Color.white;
            GUI.Label(new Rect(32f, 52f, 245f, 22f), locationName);

            GUI.color = new Color(.08f, .13f, .19f, .45f);
            GUI.DrawTexture(new Rect(JoystickCenter.x - JoystickRadius, Screen.height - JoystickCenter.y - JoystickRadius,
                JoystickRadius * 2f, JoystickRadius * 2f), Texture2D.whiteTexture);
            GUI.color = new Color(.85f, .91f, 1f, .72f);
            Vector2 knob = JoystickCenter + joystick * JoystickRadius * .62f;
            GUI.DrawTexture(new Rect(knob.x - 22f, Screen.height - knob.y - 22f, 44f, 44f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(18f, Screen.height - 25f, 300f, 22f), "Touch left side to move  •  WASD in Editor");
        }
    }
}