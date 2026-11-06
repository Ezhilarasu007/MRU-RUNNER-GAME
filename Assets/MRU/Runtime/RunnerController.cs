using UnityEngine;

namespace MRU.InfiniteRunner
{
    public enum HazardKind { Jump, Slide, Block }

    public sealed class RunnerController : MonoBehaviour
    {
        public const float LaneWidth = 2.4f;

        private readonly Transform[] limbs = new Transform[4];
        private Transform torso;
        private Transform head;
        private Transform shieldBubble;
        private Transform magnetAura;
        private Transform hoverboard;
        private Transform jetpackTrails;

        private int lane = 1; // 0: Left, 1: Center, 2: Right
        private float jumpTime;
        private float slideTime;
        private float runTime;
        private float currentX;
        private float targetX;
        private float tiltAngle;
        private float squishScaleY = 1f;

        private Renderer torsoRenderer;
        private Renderer headRenderer;
        private Vector2 touchStart;
        private bool touchTracked;

        public bool IsJumping => jumpTime > 0f;
        public bool IsSliding => slideTime > 0f;
        public bool IsHoverboarding { get; private set; }
        public float X => transform.position.x;
        public int CurrentLane => lane;

        public static RunnerController Create()
        {
            GameObject root = new GameObject("MRU Cyber Runner");
            RunnerController runner = root.AddComponent<RunnerController>();
            runner.BuildCharacter();
            return runner;
        }

        private void BuildCharacter()
        {
            transform.position = Vector3.zero;

            // Torso (Main Body Hoodie)
            torso = MakePart("Hoodie Torso", PrimitiveType.Capsule, transform, new Vector3(0f, 1.05f, 0f),
                new Vector3(0.72f, 0.75f, 0.48f), new Color(0.85f, 0.10f, 0.12f));
            torsoRenderer = torso.GetComponent<Renderer>();

            // Head & Hair
            head = MakePart("Head", PrimitiveType.Sphere, transform, new Vector3(0f, 2.05f, 0f),
                new Vector3(0.45f, 0.48f, 0.45f), new Color(0.92f, 0.68f, 0.52f));
            headRenderer = head.GetComponent<Renderer>();

            MakePart("Cyber Visor", PrimitiveType.Cube, head, new Vector3(0f, 0.08f, 0.18f),
                new Vector3(0.42f, 0.14f, 0.15f), new Color(0.05f, 0.95f, 1f));

            MakePart("Hair Stylized", PrimitiveType.Sphere, transform, new Vector3(0f, 2.30f, -0.04f),
                new Vector3(0.46f, 0.25f, 0.46f), new Color(0.12f, 0.08f, 0.05f));

            // Limbs: [0] Left Leg, [1] Right Leg, [2] Left Arm, [3] Right Arm
            limbs[0] = MakePart("Left Leg", PrimitiveType.Capsule, transform, new Vector3(-0.22f, 0.45f, 0f),
                new Vector3(0.24f, 0.50f, 0.24f), new Color(0.08f, 0.18f, 0.52f));
            limbs[1] = MakePart("Right Leg", PrimitiveType.Capsule, transform, new Vector3(0.22f, 0.45f, 0f),
                new Vector3(0.24f, 0.50f, 0.24f), new Color(0.08f, 0.18f, 0.52f));
            limbs[2] = MakePart("Left Arm", PrimitiveType.Capsule, transform, new Vector3(-0.52f, 1.15f, 0f),
                new Vector3(0.20f, 0.54f, 0.22f), new Color(0.85f, 0.10f, 0.12f));
            limbs[3] = MakePart("Right Arm", PrimitiveType.Capsule, transform, new Vector3(0.52f, 1.15f, 0f),
                new Vector3(0.20f, 0.54f, 0.22f), new Color(0.85f, 0.10f, 0.12f));

            // Sneakers
            MakePart("Left Sneaker", PrimitiveType.Cube, transform, new Vector3(-0.22f, 0.09f, 0.12f),
                new Vector3(0.32f, 0.18f, 0.52f), new Color(0.95f, 0.95f, 0.98f));
            MakePart("Right Sneaker", PrimitiveType.Cube, transform, new Vector3(0.22f, 0.09f, 0.12f),
                new Vector3(0.32f, 0.18f, 0.52f), new Color(0.95f, 0.95f, 0.98f));

            // Cyber Hoverboard (Subway Surfers style unique boost vehicle)
            hoverboard = MakePart("Cyber Hoverboard", PrimitiveType.Cube, transform, new Vector3(0f, 0.08f, 0f),
                new Vector3(0.95f, 0.12f, 2.2f), new Color(0.05f, 0.95f, 0.95f));
            hoverboard.gameObject.SetActive(false);

            // Jetpack Thruster Trails
            jetpackTrails = MakePart("Jetpack Thrusters", PrimitiveType.Cylinder, transform, new Vector3(0f, 1.1f, -0.35f),
                new Vector3(0.4f, 0.6f, 0.25f), new Color(1.0f, 0.4f, 0.05f));
            jetpackTrails.gameObject.SetActive(false);

            // Shield Visual Effect
            GameObject shieldObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shieldObj.name = "Shield Forcefield";
            shieldObj.transform.SetParent(transform, false);
            shieldObj.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            shieldObj.transform.localScale = new Vector3(2.4f, 2.4f, 2.4f);
            Destroy(shieldObj.GetComponent<Collider>());
            Renderer shieldRend = shieldObj.GetComponent<Renderer>();
            Material shieldMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            shieldMat.color = new Color(0.1f, 0.9f, 1.0f, 0.45f);
            shieldRend.material = shieldMat;
            shieldBubble = shieldObj.transform;
            shieldBubble.gameObject.SetActive(false);

            // Magnet Visual Aura
            GameObject magObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            magObj.name = "Magnet Ring";
            magObj.transform.SetParent(transform, false);
            magObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            magObj.transform.localScale = new Vector3(2.8f, 0.05f, 2.8f);
            Destroy(magObj.GetComponent<Collider>());
            Renderer magRend = magObj.GetComponent<Renderer>();
            Material magMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            magMat.color = new Color(1.0f, 0.2f, 0.8f, 0.5f);
            magRend.material = magMat;
            magnetAura = magObj.transform;
            magnetAura.gameObject.SetActive(false);
        }

        private static Transform MakePart(string name, PrimitiveType shape, Transform parent, Vector3 position,
            Vector3 scale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(shape);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            Renderer rend = part.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            rend.material = mat;
            return part.transform;
        }

        public void ReadInput()
        {
            // Keyboard Controls
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveLane(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveLane(1);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) Jump();
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Slide();

            // Mobile Touch Swipe Controls
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchStart = touch.position;
                    touchTracked = true;
                }
                else if (touch.phase == TouchPhase.Ended && touchTracked)
                {
                    touchTracked = false;
                    Vector2 delta = touch.position - touchStart;
                    if (delta.magnitude >= 40f)
                    {
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        {
                            MoveLane(delta.x > 0 ? 1 : -1);
                        }
                        else
                        {
                            if (delta.y > 0) Jump();
                            else Slide();
                        }
                    }
                }
            }
        }

        public void MoveLane(int direction)
        {
            int nextLane = Mathf.Clamp(lane + direction, 0, 2);
            if (nextLane != lane)
            {
                lane = nextLane;
                targetX = (lane - 1) * LaneWidth;
                tiltAngle = direction * -18f; // Lean into turn
            }
        }

        public void Jump()
        {
            if (jumpTime <= 0f && slideTime <= 0f)
            {
                jumpTime = 0.72f;
                squishScaleY = 1.25f; // Jump stretch
            }
        }

        public void Slide()
        {
            if (slideTime <= 0f && jumpTime <= 0f)
            {
                slideTime = 0.68f;
                squishScaleY = 0.55f; // Slide squish
            }
        }

        public void SetHoverboard(bool active)
        {
            IsHoverboarding = active;
            if (hoverboard != null) hoverboard.gameObject.SetActive(active);
            if (jetpackTrails != null) jetpackTrails.gameObject.SetActive(active);
        }

        public void SetShieldVisual(bool active)
        {
            if (shieldBubble != null) shieldBubble.gameObject.SetActive(active);
        }

        public void SetMagnetVisual(bool active)
        {
            if (magnetAura != null) magnetAura.gameObject.SetActive(active);
        }

        public void ApplySkin(int index)
        {
            Color[] colors =
            {
                new Color(0.85f, 0.10f, 0.12f), // Red MRU Classic
                new Color(0.08f, 0.45f, 0.95f), // Blue Comet
                new Color(0.12f, 0.70f, 0.32f), // Jungle Scout
                new Color(0.55f, 0.18f, 0.88f), // Night Spark
                new Color(0.98f, 0.52f, 0.08f), // Desert Dash
                new Color(0.05f, 0.92f, 0.92f)  // Neon Diver
            };

            Color color = colors[Mathf.Clamp(index, 0, colors.Length - 1)];
            if (torsoRenderer != null && torsoRenderer.material != null)
            {
                torsoRenderer.material.color = color;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Lane interpolation
            targetX = (lane - 1) * LaneWidth;
            currentX = Mathf.MoveTowards(currentX, targetX, dt * 17f);
            tiltAngle = Mathf.MoveTowards(tiltAngle, 0f, dt * 65f);

            // Timers
            jumpTime = Mathf.Max(0f, jumpTime - dt);
            slideTime = Mathf.Max(0f, slideTime - dt);

            // Squish/Stretch recovery
            squishScaleY = Mathf.MoveTowards(squishScaleY, 1f, dt * 3.5f);

            // Jump Parabola or Hoverboard elevation
            float jumpHeight = 0f;
            if (IsHoverboarding)
            {
                jumpHeight = 1.2f + Mathf.Sin(Time.time * 6f) * 0.15f; // Floating hover elevation
            }
            else if (jumpTime > 0f)
            {
                float norm = 1f - (jumpTime / 0.72f);
                jumpHeight = Mathf.Sin(norm * Mathf.PI) * 2.2f;
            }

            // Position & Bank Rotation
            Vector3 pos = transform.position;
            pos.x = currentX;
            pos.y = jumpHeight;
            transform.position = pos;
            transform.rotation = Quaternion.Euler(0f, 0f, tiltAngle);

            // Torso & Limb Animations
            if (IsSliding && !IsHoverboarding)
            {
                torso.localScale = new Vector3(0.85f, 0.38f * squishScaleY, 0.75f);
                torso.localPosition = new Vector3(0f, 0.65f, 0f);
                if (head != null) head.localPosition = new Vector3(0f, 1.25f, 0.2f);
            }
            else
            {
                torso.localScale = new Vector3(0.72f, 0.75f * squishScaleY, 0.48f);
                torso.localPosition = new Vector3(0f, 1.05f, 0f);
                if (head != null) head.localPosition = new Vector3(0f, 2.05f, 0f);
            }

            // Run Cycle Limb Oscillations
            runTime += dt * 15f;
            float stride = IsHoverboarding ? 5f : Mathf.Sin(runTime) * 35f;
            limbs[0].localRotation = Quaternion.Euler(stride, 0f, 0f);
            limbs[1].localRotation = Quaternion.Euler(-stride, 0f, 0f);
            limbs[2].localRotation = Quaternion.Euler(-stride * 1.1f, 0f, 0f);
            limbs[3].localRotation = Quaternion.Euler(stride * 1.1f, 0f, 0f);

            // Magnet Aura Pulse
            if (magnetAura != null && magnetAura.gameObject.activeSelf)
            {
                magnetAura.Rotate(Vector3.up, 200f * dt);
            }
        }

        public bool Hits(HazardKind kind, Vector3 hazardPosition)
        {
            if (IsHoverboarding) return false; // Hoverboard provides complete obstacle immunity!
            if (Mathf.Abs(transform.position.x - hazardPosition.x) > 0.85f) return false;
            if (Mathf.Abs(hazardPosition.z) > 0.85f) return false;

            if (kind == HazardKind.Jump) return !IsJumping;
            if (kind == HazardKind.Slide) return !IsSliding;
            return true; // HazardKind.Block hit always
        }
    }
}
