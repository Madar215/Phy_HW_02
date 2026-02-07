using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW_03 {
    /// <summary>
    /// Bow and arrow with manual physics (no Rigidbody, no Colliders, no Joints).
    /// String = LineRenderer; draw power + vertical angle; trajectory prediction; R = release; arrow flies and rotates; ground hit logs KE.
    /// Controls: LMB = increase draw, RMB = release string, W/S = angle, R = fire, N = nock again, Q = reload scene.
    /// </summary>
    public class BowAndArrowController : MonoBehaviour
    {
        [Header("Bow & string")]
        [Tooltip("Bow transform (forward = aim direction when angle = 0)")]
        public Transform bow;
        [Tooltip("String LineRenderer – always attached to left & right; curves deeper as you draw (real bow)")]
        public LineRenderer stringLine;
        [Tooltip("Left string attachment – string line starts here")]
        public Transform stringLeft;
        [Tooltip("Right string attachment – string line ends here")]
        public Transform stringRight;
        [Tooltip("How far the nock pulls at full draw (world units)")]
        public float maxDrawDistance = 0.5f;
        [Tooltip("String arc scale – higher = deeper curve at full draw (e.g. 2.5)")]
        public float stringArcScale = 2.5f;
        [Tooltip("String line curve resolution (points along the arc; 3 = minimal curve, 24+ = smooth)")]
        public int stringCurvePoints = 24;

        [Header("Draw & aim")]
        [Tooltip("Draw power 0–1 (how far string is pulled)")]
        [Range(0f, 1f)]
        public float drawPower = 0f;
        [Tooltip("Vertical aim angle in degrees (positive = up)")]
        public float verticalAngle = 0f;

        [Header("Arrow")]
        public Transform arrow;
        [Tooltip("Distance from arrow pivot to nock (bottom). Pivot stays ahead; nock is on the string.")]
        public float arrowNockOffset = 0.25f;
        [Tooltip("Arrow mass (kg) – heavier = less launch speed from same draw; affects trajectory and KE")]
        public float arrowMass = 0.1f;
        [Tooltip("Launch energy at full draw (Joules). Bow imparts this KE; speed = sqrt(2*energy/mass)")]
        public float launchEnergyAtFullDraw = 31f;

        [Header("Trajectory prediction")]
        public LineRenderer trajectoryLine;
        [Tooltip("Max points for trajectory (simulate until ground; full arc up then down)")]
        public int trajectoryMaxPoints = 600;
        [Tooltip("Fallback time step when deltaTime unavailable. Trajectory uses same step as flight (Time.deltaTime × timeScaleDuringFlight) so prediction matches impact.")]
        public float trajectoryTimeStepFallback = 0.016f;
        [Header("Demo: show prediction error")]
        [Tooltip("When ON: trajectory uses fixed step below instead of flight step → predicted impact lands CLOSER than actual (for teaching numerical integration error).")]
        public bool useFixedStepForTrajectoryDemo;
        [Tooltip("Fixed step used when demo is ON. Try 0.04–0.06 to see trajectory/impact land short; flight still uses real deltaTime so arrow lands farther.")]
        [Min(0.01f)]
        public float trajectoryFixedStepForDemo = 0.05f;

        [Header("Physics (manual)")]
        public Vector3 gravity = new Vector3(0f, -9.81f, 0f);
        [Tooltip("Air drag per second (0 = none). Linear drag: velocity decays; applied in trajectory and flight.")]
        [Min(0f)]
        public float airDrag = 0f;

        [Header("Ground")]
        [Tooltip("Ground Y – arrow stops when position.y <= this")]
        public float groundY = 0f;
        [Tooltip("Or use raycast on this layer (if > 0, overrides groundY when hit)")]
        public LayerMask groundLayer = 0;

        [Header("Time")]
        [Tooltip("Unity time scale during flight (1 = normal, 0.5 = half speed). Auto reset to 1 on impact.")]
        [Range(0.05f, 1f)]
        public float timeScaleDuringFlight = 0.5f;

        public bool IsFlying { get; private set; }
        bool arrowStuck;
        Vector3 arrowVelocity;
        public Vector3 CurrentVelocity => arrowVelocity;
        Quaternion bowBaseRotation;
        Vector3 currentNockPosition;
        bool lmbHeldPreviousFrame;
        float debugKE;
        float debugPE;
        float debugMomentumMagnitude;
        float debugSpeed;
        Vector3 debugVelocity;

        void Start()
        {
            Time.timeScale = 1f;
            if (bow == null) bow = transform;
            bowBaseRotation = bow != null ? bow.rotation : Quaternion.identity;
            if (trajectoryLine != null) trajectoryLine.positionCount = 0;
            IsFlying = false;
            arrowStuck = false;
            if (stringLeft != null && stringRight != null)
                currentNockPosition = (stringLeft.position + stringRight.position) * 0.5f;
            else
                currentNockPosition = bow != null ? bow.position : Vector3.zero;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            if (bow == null) return;

            if (Input.GetKeyDown(KeyCode.N))
                arrowStuck = false;

            if (IsFlying)
            {
                UpdateArrowFlight(Time.deltaTime);
                if (trajectoryLine != null) trajectoryLine.positionCount = 0;
                return;
            }

            UpdateDrawInput();
            ApplyBowTilt();
            UpdateStringLine();
            if (!arrowStuck)
            {
                UpdateArrowAtNock();
                UpdateTrajectoryPrediction();
            }
            else if (trajectoryLine != null)
                trajectoryLine.positionCount = 0;

            if (Input.GetKeyDown(KeyCode.R) && !arrowStuck)
                ReleaseArrow();
        }

        void UpdateDrawInput()
        {
            bool lmbHeld = Input.GetMouseButton(0);
            //f (lmbHeld && lmbHeldPreviousFrame)
            //    drawPower = Mathf.Clamp01(drawPower + Time.deltaTime * 2f);
            lmbHeldPreviousFrame = lmbHeld;

            //if (Input.GetMouseButton(1))
            //  drawPower = Mathf.Clamp01(drawPower - Time.deltaTime * 4f);

            float angleInput = Input.GetAxisRaw("Vertical");
            verticalAngle += angleInput * 90f * Time.deltaTime;
            verticalAngle = Mathf.Clamp(verticalAngle, -89f, 89f);
        }

        void ApplyBowTilt()
        {
            if (bow == null) return;
            Vector3 pitchAxis = bowBaseRotation * Vector3.right;
            bow.rotation = bowBaseRotation * Quaternion.AngleAxis(verticalAngle, pitchAxis);
        }

        void UpdateStringLine()
        {
            if (stringLine == null || stringLeft == null || stringRight == null) return;

            Vector3 left = stringLeft.position;
            Vector3 right = stringRight.position;
            Vector3 forward = GetBowForward();
            float pull = drawPower * maxDrawDistance * Mathf.Max(0.01f, stringArcScale);
            Vector3 nock = (left + right) * 0.5f + forward * pull;
            currentNockPosition = nock;

            int pointCount = (stringLine.positionCount >= 3) ? stringLine.positionCount : Mathf.Max(3, stringCurvePoints);
            stringLine.positionCount = pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                float t = (pointCount > 1) ? (i / (float)(pointCount - 1)) : 0f;
                Vector3 p = QuadraticBezier(left, nock, right, t);
                stringLine.SetPosition(i, p);
            }
        }

        static Vector3 QuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

        void UpdateArrowAtNock()
        {
            if (arrow == null) return;

            Vector3 nock = (stringLine != null && stringLeft != null && stringRight != null) ? currentNockPosition : GetNockPosition();
            Vector3 launchDir = GetLaunchDirection();

            arrow.rotation = Quaternion.LookRotation(launchDir);
            arrow.position = nock + arrow.forward * arrowNockOffset;
        }

        float GetEffectiveFlightStep()
        {
            float dt = Time.deltaTime > 0f ? Time.deltaTime * timeScaleDuringFlight : trajectoryTimeStepFallback;
            return Mathf.Clamp(dt, 0.002f, 0.03f);
        }

        /// <summary>Step used for trajectory line and predicted impact. When demo is ON, uses fixed larger step so prediction lands short (for teaching).</summary>
        public float GetTrajectoryStep()
        {
            if (useFixedStepForTrajectoryDemo)
                return Mathf.Clamp(trajectoryFixedStepForDemo, 0.01f, 0.1f);
            return GetEffectiveFlightStep();
        }

        void UpdateTrajectoryPrediction()
        {
            if (trajectoryLine == null) return;

            Vector3 pos = arrow != null ? arrow.position : GetNockPosition();
            Vector3 vel = GetLaunchVelocity();
            float step = GetTrajectoryStep();
            List<Vector3> points = ComputeTrajectory(pos, vel, trajectoryMaxPoints, step);
            trajectoryLine.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                trajectoryLine.SetPosition(i, points[i]);
        }

        public List<Vector3> ComputeTrajectory(Vector3 startPos, Vector3 startVel, int maxPoints, float dt)
        {
            var points = new List<Vector3> { startPos };
            Vector3 p = startPos;
            Vector3 v = startVel;
            for (int i = 0; i < maxPoints - 1; i++)
            {
                ApplyDrag(ref v, dt);
                v += gravity * dt;
                p += v * dt;
                points.Add(p);
                if (p.y <= groundY) break;
            }
            return points;
        }

        /// <summary>
        /// Simulate trajectory until ground; returns impact speed (m/s) and impact position at exact ground crossing (interpolated). Returns false if never hits ground.
        /// Uses same step as flight so prediction matches actual impact.
        /// </summary>
        public bool GetPredictedImpact(out Vector3 impactPos, out float impactSpeed)
        {
            impactPos = Vector3.zero;
            impactSpeed = 0f;
            Vector3 pos = arrow != null ? arrow.position : GetNockPosition();
            Vector3 vel = GetLaunchVelocity();
            float dt = GetTrajectoryStep();
            Vector3 p = pos;
            Vector3 v = vel;
            for (int i = 0; i < trajectoryMaxPoints - 1; i++)
            {
                Vector3 prevP = p;
                Vector3 prevV = v;
                ApplyDrag(ref v, dt);
                v += gravity * dt;
                p += v * dt;
                if (p.y <= groundY)
                {
                    float denom = prevP.y - p.y;
                    float fraction = (denom > 0.0001f) ? (prevP.y - groundY) / denom : 1f;
                    fraction = Mathf.Clamp01(fraction);
                    impactPos = Vector3.Lerp(prevP, p, fraction);
                    Vector3 impactVel = prevV + (v - prevV) * fraction;
                    impactSpeed = impactVel.magnitude;
                    return true;
                }
            }
            return false;
        }

        void ReleaseArrow()
        {
            if (arrow == null || IsFlying) return;

            arrowVelocity = GetLaunchVelocity();
            IsFlying = true;
            drawPower = 0f;
            Time.timeScale = timeScaleDuringFlight;
        }

        void UpdateArrowFlight(float dt)
        {
            if (arrow == null) return;

            Vector3 prevPos = arrow.position;
            Vector3 prevVel = arrowVelocity;

            ApplyDrag(ref arrowVelocity, dt);
            arrowVelocity += gravity * dt;
            arrow.position += arrowVelocity * dt;

            if (arrowVelocity.sqrMagnitude > 0.0001f)
                arrow.rotation = Quaternion.LookRotation(arrowVelocity.normalized);

            float speedSq = arrowVelocity.sqrMagnitude;
            float speed = Mathf.Sqrt(speedSq);
            debugKE = 0.5f * arrowMass * speedSq;
            float g = gravity.magnitude;
            debugPE = arrowMass * g * Mathf.Max(0f, arrow.position.y - groundY);
            debugMomentumMagnitude = arrowMass * speed;
            debugSpeed = speed;
            debugVelocity = arrowVelocity;

            CheckGroundImpact(dt, prevPos, prevVel);
        }

        void ApplyDrag(ref Vector3 velocity, float dt)
        {
            if (airDrag <= 0f) return;
            float scale = Mathf.Clamp01(1f - airDrag * dt);
            velocity *= scale;
        }

        void CheckGroundImpact(float dt, Vector3 prevPos, Vector3 prevVel)
        {
            float currentY = arrow.position.y;
            if (groundLayer != 0)
            {
                if (Physics.Raycast(arrow.position, Vector3.down, 0.5f, groundLayer))
                {
                    StopArrowAndLogKE(arrowVelocity, arrow.position);
                    return;
                }
            }
            if (currentY <= groundY)
            {
                float denom = prevPos.y - arrow.position.y;
                if (denom > 0.0001f)
                {
                    float fraction = (prevPos.y - groundY) / denom;
                    fraction = Mathf.Clamp01(fraction);
                    Vector3 impactPos = Vector3.Lerp(prevPos, arrow.position, fraction);
                    Vector3 impactVel = Vector3.Lerp(prevVel, arrowVelocity, fraction);
                    arrow.position = impactPos;
                    StopArrowAndLogKE(impactVel, impactPos);
                }
                else
                    StopArrowAndLogKE(arrowVelocity, arrow.position);
            }
        }

        /// <summary>
        /// Impact formulas: KE = ½ m v² (J); momentum p = m v, |p| = m v (kg·m/s). Uses velocity at exact ground crossing so prediction and actual match.
        /// </summary>
        void StopArrowAndLogKE(Vector3 impactVelocity, Vector3 impactPosition)
        {
            float speedSq = impactVelocity.sqrMagnitude;
            float speed = Mathf.Sqrt(speedSq);
            float ke = 0.5f * arrowMass * speedSq;
            Vector3 momentum = arrowMass * impactVelocity;
            float momentumMag = momentum.magnitude;
            Debug.Log($"[Arrow] Ground impact. velocity = ({impactVelocity.x:F2}, {impactVelocity.y:F2}, {impactVelocity.z:F2}) m/s, |v| = {speed:F2} m/s, KE = {ke:F2} J, |p| = {momentumMag:F2} kg·m/s (mass = {arrowMass} kg)");
            Time.timeScale = 1f;
            IsFlying = false;
            arrowStuck = true;
            arrowVelocity = Vector3.zero;
        }

        void OnGUI()
        {
            float x = 10f, y = 10f, lineH = 22f, w = 540f;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.yellow;
            style.fontStyle = FontStyle.Bold;

            if (IsFlying && arrow != null)
            {
                int lineCount = 5;
                float boxH = lineCount * lineH + 8f;
                GUI.Box(new Rect(x - 4f, y - 4f, w + 8f, boxH), "");
                GUI.Label(new Rect(x, y, w, lineH), $"v (velocity): ({debugVelocity.x:F2}, {debugVelocity.y:F2}, {debugVelocity.z:F2}) m/s  |v| = {debugSpeed:F2} m/s", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"KE (kinetic): {debugKE:F2} J  (½ m v²)", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"PE (potential): {debugPE:F2} J  (m g h)", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"|p| (momentum): {debugMomentumMagnitude:F2} kg·m/s  (m v)", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"mass = {arrowMass} kg", style);
            }
            else
            {
                float storedPE = drawPower * launchEnergyAtFullDraw;
                float mass = Mathf.Max(arrowMass, 0.001f);
                float launchSpeed = Mathf.Sqrt(2f * storedPE / mass);
                bool hitGround = GetPredictedImpact(out Vector3 impactPos, out float impactSpeed);
                float predictedDamageJ = hitGround ? (0.5f * arrowMass * impactSpeed * impactSpeed) : 0f;
                int lineCount = 6;
                float boxH = lineCount * lineH + 8f;
                GUI.Box(new Rect(x - 4f, y - 4f, w + 8f, boxH), "");
                GUI.Label(new Rect(x, y, w, lineH), $"draw power: {drawPower:F2}  (0–1)", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"PE (stored in bow): {storedPE:F2} J  (drawPower × launchEnergyAtFullDraw)", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"launch speed (if released now): {launchSpeed:F2} m/s  (√(2 PE / m))", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), $"mass = {arrowMass} kg", style); y += lineH;
                GUI.Label(new Rect(x, y, w, lineH), hitGround
                    ? $"damage at target (where trajectory hits ground): {predictedDamageJ:F2} J  (½ m v² at impact)"
                    : "damage at target: — (trajectory does not hit ground)", style); y += lineH;
                if (hitGround)
                    GUI.Label(new Rect(x, y, w, lineH), $"impact speed at ground: {impactSpeed:F2} m/s  |  impact position: ({impactPos.x:F1}, {impactPos.y:F1}, {impactPos.z:F1})", style);
            }
        }

        Vector3 GetBowForward()
        {
            if (bow == null) return Vector3.forward;
            return bow.forward;
        }

        Vector3 GetLaunchDirection()
        {
            return (-GetBowForward()).normalized;
        }

        public Vector3 GetLaunchVelocity()
        {
            float mass = Mathf.Max(arrowMass, 0.001f);
            float ke = drawPower * launchEnergyAtFullDraw;
            float speed = Mathf.Sqrt(2f * ke / mass);
            return GetLaunchDirection().normalized * speed;
        }

        public Vector3 GetNockPosition()
        {
            Vector3 left = stringLeft != null ? stringLeft.position : bow.position;
            Vector3 right = stringRight != null ? stringRight.position : bow.position;
            Vector3 mid = (left + right) * 0.5f;
            float pull = drawPower * maxDrawDistance * Mathf.Max(0.01f, stringArcScale);
            return mid + GetBowForward() * pull;
        }
    }
}
