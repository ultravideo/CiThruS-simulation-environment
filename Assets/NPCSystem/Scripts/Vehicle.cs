using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the behaviour of a vehicle
/// </summary>
public class Vehicle : MonoBehaviour
{

    private UniqueWaypoint[] path;

    private float targetDriveSpeed = 14f;
    private float driveSpeed;
    public float DriveSpeed { get { return driveSpeed; } }
    private float turnSpeed = 4f; //6f old

    private float brakingRate = 35f;
    private float accelRate = 10f;

    [SerializeField] private Transform intersectingCheckRayPosition;

    private Rigidbody rb;

    private bool drivingCompleted;
    public bool DrivingCompleted { get { return drivingCompleted; } }

    private float nextTime = 0.0f;

    // time t
    private float t = 0.0f;

    private uint currentIndex = 0;
    private float currentPositionFromNext = 0.0f;
    private float currentDistance = 0.0f;

    private float ttime;
    private float nextT;

    private float collisionDistance = 5f;

    private float extendedColDistance = 8f;

    private float currentColDist;

    private float previousHeight = 0f;

    [SerializeField] private VehicleType vehicleType;
    public VehicleType VehicleType { get { return vehicleType; } }

    //private float speedCheckDistance = 8f;

    // origin or, target ta, origin quaternion oq, target quaternion tq
    Vector3 origin_position, target_position;
    Quaternion origin_quaternion, target_quaternion;

    private RaycastHit hitLeft;
    private RaycastHit hitRight;
    private RaycastHit hitLeftSlanted;
    private RaycastHit hitRightSlanted;
    private RaycastHit hitCenter;
    private RaycastHit hitSpeed;
    private RaycastHit fastHit;

    private RaycastHit collisionHit;

    private RaycastHit[] sphereHit;

    private bool fastHitStatus;

    private Vector3 hitLeftDir;
    private Vector3 hitRightDir;
    private Vector3 hitLeftDirSlanted;
    private Vector3 hitRightDirSlanted;

    private Vector3 newPosition;

    private bool movementDone = false;

    private bool speedColLastState = false;

    private NPCMovementManager npcmovement;

    [SerializeField] private Transform raycastDefaultPosition;

    private float lastMovement;
    private const float killThreshold = 12f;

    private bool overrideCollisionDetection = false;

    private List<Vehicle> ignoreVehicles = new List<Vehicle>();

    private RaycastHit heightHit;
    int heightRaycastlayermask = (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12);
    int colCheckLayerMask = (1 << 9);

    public bool ignoreLongRoadAll = false;

    private RaycastHit[] pedestrianRaycastHits;
    private int pedestrianRaycastLayerMask = (1 << 11);

    public enum TurnDirection
    {
        left, right, forward
    };

    private enum Accel
    {
        idle,
        accelerate,
        deaccelerate
    };

    private Accel accelStatus = Accel.idle;

    private TurnDirection turnDirection;
    public TurnDirection TurnDir { get { return turnDirection; } }

    private float turnThreshold = 20f; //17f;
    Vector3 turnVector;

    private bool stoppedInTrafficLights = false;
    public bool StoppedInTrafficLights { get { return stoppedInTrafficLights; } }

    private Vehicle previousCarFromLeft;

    private List<UniqueWaypoint> path3 = new List<UniqueWaypoint>();

    private float collisionDisablePeriod;

    private bool didTrafficLightsStop = false;

    private float angle;
    private float nextAngle;
    public float TurnAngle { get { return nextAngle; } }

    private Ray sphereRay;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        drivingCompleted = true;
        driveSpeed = targetDriveSpeed;
        heightRaycastlayermask = ~heightRaycastlayermask;

        npcmovement = FindObjectOfType<NPCMovementManager>();

        currentColDist = collisionDistance;

        if (ConfigManager.AllCarsIgnoreLongRoad)
            ignoreLongRoadAll = true;

    }

    private void Update()
    {
        if (accelStatus == Accel.accelerate)
        {
            driveSpeed += Time.deltaTime * accelRate;

            if (driveSpeed >= targetDriveSpeed)
            {
                driveSpeed = targetDriveSpeed;
                accelStatus = Accel.idle;
            }
        }
        else if (accelStatus == Accel.deaccelerate)
        {
            driveSpeed -= Time.deltaTime * brakingRate;

            if (driveSpeed <= 0f)
            {
                driveSpeed = 0f;
                accelStatus = Accel.idle;
            }
        }


        if (!overrideCollisionDetection && !didTrafficLightsStop)
            RuleCollision();

        Drive();

        // kill itself if gets stuck
        if (Time.time > lastMovement + killThreshold)
        {
            //npcmovement.KillVehicle(this);
            overrideCollisionDetection = true;
            collisionDisablePeriod = Time.time;
            driveSpeed = targetDriveSpeed;
        }
        else if (Time.time > collisionDisablePeriod + 2f && overrideCollisionDetection)
            overrideCollisionDetection = false;

        if (driveSpeed > 0f)
            currentColDist = collisionDistance;
        else
            currentColDist = extendedColDistance;

    }

    private void Drive()
    {
        if (Time.time >= nextTime && driveSpeed > 0.0f && movementDone) // Time.time >= nextTime && 
        {
            if (path3.Count != 3)
                npcmovement.KillVehicle(this);

            path3 = ExtensionMethods.ShiftPathChainByOne(path3, ignoreLongRoadAll);

            if (path3[0] == null || path3[1] == null || path3[2] == null)
            {
                npcmovement.KillVehicle(this);
                return;
            }

            origin_position = path3[0].Position;
            target_position = path3[1].Position;
            target_quaternion = Quaternion.LookRotation(target_position - origin_position, Vector3.up);

            turnVector = path3[2].Position - origin_position;

            angle = Vector3.SignedAngle(transform.forward, turnVector, Vector3.up);
            nextAngle = Vector3.SignedAngle(transform.forward, path3[1].Position - origin_position, Vector3.up);
            // right
            if (angle > turnThreshold)
                turnDirection = TurnDirection.right;
            // left
            else if (angle < -turnThreshold)
                turnDirection = TurnDirection.left;
            // forward
            else
                turnDirection = TurnDirection.forward;

            currentDistance = Vector3.Distance(transform.position, target_position);
            t = currentDistance / targetDriveSpeed;
            nextTime = Time.time + t;

            movementDone = false;

            StartCoroutine(Turn(target_quaternion, transform.rotation));

        }

        if (target_position == Vector3.zero)
            target_position = path3[0].Position;

        if (origin_position != null && target_position != null)
        {
            Vector3 heightOriginPos = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z);

            if (Physics.Raycast(heightOriginPos, Vector3.down, out heightHit, 15f, heightRaycastlayermask))
                transform.position = new Vector3(transform.position.x, heightHit.point.y, transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(target_position.x, transform.position.y, target_position.z),
                Time.deltaTime * driveSpeed);

            if (driveSpeed > 0f)
                lastMovement = Time.time;

            if (Mathf.Abs(transform.position.x - target_position.x) < 0.2f && Mathf.Abs(transform.position.z - target_position.z) < 0.2f)
                movementDone = true;
        }
    }

    private bool CheckForPedestrians()
    {
        sphereRay.origin = raycastDefaultPosition.position;
        sphereRay.direction = raycastDefaultPosition.forward;

        pedestrianRaycastHits = Physics.SphereCastAll(sphereRay, 2f, 1f, pedestrianRaycastLayerMask);

        foreach (RaycastHit h in pedestrianRaycastHits)
        {
            PedestrianSlave p = h.transform.GetComponent<PedestrianSlave>();
            if (p != null)
            {
                if (p.IsInIntersection && !p.standingInRedLight)
                    return true;
            }
        }
        return false;
    }
    
    private void RuleCollision()
    {
        hitLeftDir = Quaternion.Euler(Vector3.up * -32f) * raycastDefaultPosition.forward;
        hitRightDir = Quaternion.Euler(Vector3.up * 32f) * raycastDefaultPosition.forward;

        hitLeftDirSlanted = Quaternion.Euler(Vector3.up * -15f) * raycastDefaultPosition.forward;
        hitRightDirSlanted = Quaternion.Euler(Vector3.up * 15f) * raycastDefaultPosition.forward;

        // we are turning right, ignore everyone else but still check for speed
        if (turnDirection == TurnDirection.right)
        {
            bool raycastCenter =
                Physics.Raycast(raycastDefaultPosition.position, raycastDefaultPosition.forward, out hitCenter, currentColDist * 0.45f, colCheckLayerMask);
            bool raycastRight =
                Physics.Raycast(raycastDefaultPosition.position, hitRightDir, out hitRight, currentColDist * 0.3f, colCheckLayerMask);

            Vehicle c = null, r = null;
            if (raycastCenter) c = hitCenter.transform.GetComponent<Vehicle>();
            if (raycastRight) r = hitRight.transform.GetComponent<Vehicle>();

            if (r != null)
            {
                if (r.StoppedInTrafficLights)
                {
                    driveSpeed = 0f;
                    accelStatus = Accel.idle;
                    stoppedInTrafficLights = true;
                }
                else
                {
                    stoppedInTrafficLights = false;
                    //driveSpeed = targetDriveSpeed;
                    accelStatus = Accel.accelerate;
                }
            }
            else
            {
                stoppedInTrafficLights = false;
                //driveSpeed = targetDriveSpeed;
                accelStatus = Accel.accelerate;
            }

            if (c != null)
            {
                if (c.DriveSpeed == 0f && !c.StoppedInTrafficLights)
                {
                    driveSpeed = 0f;
                    accelStatus = Accel.idle;
                }
                else
                    accelStatus = Accel.accelerate;
                //driveSpeed = targetDriveSpeed;
            }
            // ;;;
            else
                accelStatus = Accel.accelerate;

            if (c == null && r == null)
                accelStatus = Accel.accelerate;

        }
        // we are turning left, avoid everyone else
        else if (turnDirection == TurnDirection.left)
        {
            Vehicle c = null, l = null, r = null, rs = null, ls = null;

            bool raycastCenter =
                Physics.Raycast(raycastDefaultPosition.position, raycastDefaultPosition.forward, out hitCenter, currentColDist * 0.65f, colCheckLayerMask);
            bool raycastLeft =
                Physics.Raycast(raycastDefaultPosition.position, hitLeftDir, out hitLeft, currentColDist * 0.45f, colCheckLayerMask);
            bool raycastRight =
                Physics.Raycast(raycastDefaultPosition.position, hitRightDir, out hitRight, currentColDist * 0.45f, colCheckLayerMask);
            bool raycastLeftSlanted =
                Physics.Raycast(raycastDefaultPosition.position, hitLeftDirSlanted, out hitLeftSlanted, currentColDist * 0.5f, colCheckLayerMask);
            bool raycastRightSlanted =
                Physics.Raycast(raycastDefaultPosition.position, hitRightDirSlanted, out hitRightSlanted, currentColDist * 0.5f, colCheckLayerMask);

            if (raycastCenter) c = hitCenter.transform.GetComponent<Vehicle>();
            if (raycastLeft) l = hitLeft.transform.GetComponent<Vehicle>();
            if (raycastRight) r = hitRight.transform.GetComponent<Vehicle>();
            if (raycastLeftSlanted) ls = hitLeftSlanted.transform.GetComponent<Vehicle>();
            if (raycastRightSlanted) rs = hitRightSlanted.transform.GetComponent<Vehicle>();

            if ((c != null && !c.StoppedInTrafficLights) || l != null || (r != null && !r.StoppedInTrafficLights) || (rs != null && !rs.StoppedInTrafficLights) || ls != null)
            {
                driveSpeed = 0f;
                accelStatus = Accel.idle;
            }
            else
                accelStatus = Accel.accelerate;
            //driveSpeed = targetDriveSpeed;

            if (c == null && l == null && r == null && rs == null && ls == null)
                accelStatus = Accel.accelerate;
        }
        // we are going forward, avoid only from right (and check forward for stopped vehicles)
        else if (turnDirection == TurnDirection.forward)
        {
            Vehicle c = null, r = null, rs = null;

            bool raycastCenter =
                Physics.Raycast(raycastDefaultPosition.position, raycastDefaultPosition.forward, out hitCenter, currentColDist * 0.25f, colCheckLayerMask);
            bool raycastRight =
                Physics.Raycast(raycastDefaultPosition.position, hitRightDir, out hitRight, currentColDist * 0.1f, colCheckLayerMask);
            bool raycastRightSlanted =
                Physics.Raycast(raycastDefaultPosition.position, hitRightDirSlanted, out hitRightSlanted, currentColDist * 0.15f, colCheckLayerMask);

            if (raycastCenter) c = hitCenter.transform.GetComponent<Vehicle>();
            if (raycastRight) r = hitRight.transform.GetComponent<Vehicle>();
            if (raycastRightSlanted) rs = hitRightSlanted.transform.GetComponent<Vehicle>();

            if ((r != null && !r.StoppedInTrafficLights) || (c != null && !c.StoppedInTrafficLights) || (rs != null && !rs.StoppedInTrafficLights))
                driveSpeed = 0f;
            else
                accelStatus = Accel.accelerate;
            //driveSpeed = targetDriveSpeed;

            if (c != null && c.StoppedInTrafficLights)
            {
                driveSpeed = 0f;
                accelStatus = Accel.idle;
                stoppedInTrafficLights = true;
            }
            else if (c != null && !c.StoppedInTrafficLights)
            {
                driveSpeed = 0f;
                accelStatus = Accel.idle;
                stoppedInTrafficLights = false;
            }
            // ;;;
            else
                accelStatus = Accel.accelerate;

            if (c == null && r == null && rs == null)
                accelStatus = Accel.accelerate;
        }

        // stop if intersecting
        if (Physics.Raycast(intersectingCheckRayPosition.position, intersectingCheckRayPosition.forward, out collisionHit, 2f, colCheckLayerMask))
        {
            Vehicle v = collisionHit.transform.GetComponent<Vehicle>();
            if (v && v != this)
            {
                driveSpeed = 0f;
                accelStatus = Accel.idle;
            }
        }

        if (CheckForPedestrians())
        {
            driveSpeed = 0f;
            accelStatus = Accel.idle;
        }
    }
    
    public void IsInIntersection(bool isInIntersec)
    {
        if (isInIntersec)
        {
            //driveSpeed = 0f;
            accelStatus = Accel.deaccelerate;
            didTrafficLightsStop = true;
            stoppedInTrafficLights = true;
            //overrideCollisionDetection = true;
        }
        else
        {
            //driveSpeed = targetDriveSpeed;
            accelStatus = Accel.accelerate;
            didTrafficLightsStop = false;
            stoppedInTrafficLights = false;
            //overrideCollisionDetection = false;
        }
    }
    
    /// <summary>
    /// handles smooth turning 
    /// </summary>
    /// <param name="dirQ"></param>
    /// <param name="originQ"></param>
    /// <returns></returns>
    IEnumerator Turn(Quaternion dirQ, Quaternion originQ)
    {
        float slider = 0.0f;

        while (slider < 1.0f)
        {
            slider += Time.deltaTime * turnSpeed * (driveSpeed / targetDriveSpeed);

            transform.rotation = Quaternion.Slerp(originQ, dirQ, slider);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
    

    /// <summary>
    /// Overrides collisiondetection. Mainly used when checking for intersections and traffic lights
    /// </summary>
    /// <param name="value">bool</param>
    public void OverrideCollisionDetection(bool value, List<Vehicle> _ignoreVehicles = null)
    {
        if (_ignoreVehicles != null)
            ignoreVehicles = _ignoreVehicles;

        overrideCollisionDetection = value;

    }

    public void InitializeSpawnData(UniqueWaypoint first, UniqueWaypoint second, UniqueWaypoint third)
    {
        path3 = new List<UniqueWaypoint>();
        path3.Add(first);
        path3.Add(second);
        path3.Add(third);
    }
}
