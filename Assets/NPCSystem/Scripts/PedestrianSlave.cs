using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// used to differentiate between pedestrian movement stages
/// </summary>
public enum WalkStates
{
    walk = 2,
    run = 3,
    idle = 0
};

public class PedestrianSlave : MonoBehaviour
{
    [SerializeField] private float feetHeightOffset = 0f;

    private Vector3[] currentPath;
    private Vector3 pathLastPosition;
    private PedestrianHiveMind pedestrianHiveMind;

    // these are used to determine whether the pedestrian has been stuck somewhere and cannnot move
    private float nextTime = 0f;
    private float STATIONARY_THRESHOLD = 10f;

    // these are for updating next target position
    private float walkSpeed = 1.5f;
    public float currentWalkSpeed;
    private float runSpeed = 4f;
    private float turnSpeed = 3f;

    //private float nextWalkTime = 0f;
    private Vector3 nextPosition = new Vector3();
    private Quaternion nextRotation = new Quaternion();
    private uint pathIndex = 0;
    float y_height = 0f;

    private int heightCastLayerMask = (1 << 9) | (1 << 10) | (1 << 11);
    private RaycastHit heightHit;

    private bool stopped = false;
    public bool Stopped { get { return stopped; } }

    [SerializeField] private Animator animator;

    private RaycastHit[] pedestrianSpeedHits;
    private int pedestrianSpeedLayerMask = (1 << 11);
    Ray speedRay;

    public bool IsInIntersection = false;
    public bool standingInRedLight = false;

    private WalkStates currentWalkState = WalkStates.idle;

    private void Start()
    {
        walkSpeed = walkSpeed + Random.Range(-0.25f, 1.75f);
        runSpeed = runSpeed + Random.Range(-0.25f, 2.5f);

        currentWalkSpeed = walkSpeed;

        if (!animator)
            animator = GetComponent<Animator>();
        animator.SetInteger("walkState", (int)WalkStates.walk);

        heightCastLayerMask = ~heightCastLayerMask;

        pedestrianHiveMind = FindObjectOfType<PedestrianHiveMind>();
        SetNewPath(pedestrianHiveMind.RequestNewPath(transform.position));
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, pathLastPosition) < 1.5f || pathIndex >= currentPath.Length)
            SetNewPath(pedestrianHiveMind.RequestNewPath(transform.position));

        //if (Time.time > nextWalkTime && currentPath.Length > 0)
        if (currentPath.Length > 0 && Vector3.Distance(transform.position, nextPosition) < 1f)
        {
            if (pathIndex >= currentPath.Length)
                SetNewPath(pedestrianHiveMind.RequestNewPath(transform.position));

            nextPosition = currentPath[pathIndex];
            Vector3 deltaDir = currentPath[pathIndex] - currentPath[pathIndex - (pathIndex > 0 ? 1 : 0)];
            nextRotation = (pathIndex + 1 < currentPath.Length) ? 
                Quaternion.LookRotation(deltaDir != Vector3.zero ? deltaDir : currentPath[pathIndex], Vector3.up) : nextRotation;

            //nextWalkTime = Time.time + (Vector3.Distance(transform.position, nextPosition) / currentWalkSpeed);
            pathIndex++;

            StartCoroutine(Turn(nextRotation, transform.rotation));
        }

        if (!stopped)
            Move(nextPosition, nextRotation);

        if (standingInRedLight)
            animator.SetInteger("walkState", (int)WalkStates.idle);
    }

    public void StopMoving(bool value)
    {
        standingInRedLight = value;

        if (value == stopped)
            return;
        
        if (value)
            animator.SetInteger("walkState", (int)WalkStates.idle);
        else
            animator.SetInteger("walkState", (int)WalkStates.walk);

        stopped = value;
    }

    public void RunFromRed(bool value)
    {
        if (value)
        {
            currentWalkSpeed = runSpeed;
            animator.SetInteger("walkState", (int)WalkStates.run);
        }
        else
        {
            currentWalkSpeed = walkSpeed;
            animator.SetInteger("walkState", (int)WalkStates.walk);
        }
    }

    /// <summary>
    /// Moves the NPC towards a specified target
    /// </summary>
    /// <param name="target">Vector3</param>
    private void Move(Vector3 targetP, Quaternion targetQ)
    {
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Vector3.down, out heightHit, 2f, heightCastLayerMask);
        targetP = new Vector3(targetP.x, heightHit.point.y + feetHeightOffset, targetP.z);
        
        transform.position = Vector3.MoveTowards(transform.position, targetP, Time.deltaTime * currentWalkSpeed);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQ, Time.deltaTime * turnSpeed);
    }

    public void SetNewPath(Vector3[] newPath)
    {
        currentPath = newPath;
        pathIndex = 0;

        if (currentPath.Length == 0 || currentPath.Length == 1)
            return;

        nextPosition = currentPath[0];
        pathLastPosition = currentPath[currentPath.Length - 1];
        transform.rotation = Quaternion.LookRotation(currentPath[1] - currentPath[0], Vector3.up);
    }

    IEnumerator Turn(Quaternion dirQ, Quaternion originQ)
    {
        float slider = 0.0f;

        while (slider < 1.0f)
        {
            slider += Time.deltaTime * turnSpeed;

            transform.rotation = Quaternion.Slerp(originQ, dirQ, slider);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
