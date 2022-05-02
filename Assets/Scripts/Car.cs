using UnityEngine;
using PathCreation;
using System.Collections;
using System.Linq;
using System;

public class Car : MonoBehaviour
{
    public float speed = 10f;
    float currentSpeed;
    PathCreator[] lanes;

    float distance;
    int currentLane;
    public float maxChange = 1.5f;

    public float brakeSpeed = 1.2f, brakeDeceleration = 3f, brakeAcceleration = 0.6f;
    protected bool isBraking, canBob = true;

    protected bool isStopping;

    bool changingRotation;
    Quaternion changeLaneRot;

    protected bool canChangeLane = true;

    protected Rigidbody rb;

    public event Action<int, string> OnLaneChanged;
    public event Action<PathCreator> OnDirectionChanged;

    public PathCreator startingLane;

    protected virtual void Start()
    {
        lanes = FindObjectsOfType<PathCreator>();
        currentLane = Array.IndexOf(lanes, startingLane);
        OnLaneChanged?.Invoke(0, startingLane.name);
        currentSpeed = speed;
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        HandleMovement();

        HandleBobbing();
    }

    void HandleBobbing()
    {
        if (isStopping)
        {
            return;
        }

        float bobMultiplier = 0.05f;

        if (canBob)
        {
            // driving
            transform.rotation *= Quaternion.Euler(new Vector3(0, 0, Mathf.Sin(distance) * bobMultiplier));
        } else
        {
            // braking
            transform.rotation *= Quaternion.Euler(new Vector3(0, 0, Mathf.Sin(distance) * bobMultiplier + 0.3f));
        }
    }

    void HandleMovement()
    {
        if (isBraking)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, brakeSpeed, brakeDeceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, speed, brakeAcceleration * Time.deltaTime);
        }

        if (isStopping) return;

        distance += currentSpeed * Time.deltaTime;

        Vector3 nextPos = lanes[currentLane].path.GetPointAtDistance(distance) + new Vector3(0, 0.5f, 0);
        transform.position = Vector3.Lerp(transform.position, nextPos, 10f * Time.deltaTime);

        rb.MovePosition(transform.position);

        if (!changingRotation)
        {
            Quaternion nextRot = lanes[currentLane].path.GetRotationAtDistance(distance) * Quaternion.Euler(new Vector3(270, -90, -180));
            float rotChange = Quaternion.Angle(transform.rotation, nextRot);
            if (rotChange >= 80f)
            {
                StartCoroutine(HandleDirectionChange(nextRot, rotChange));
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, nextRot, 6f * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, changeLaneRot, 4f * Time.deltaTime);
        }
    }

    protected PathCreator[] GetPossibleLanes(int amount)
    {
        return lanes
            .Where((lane) => Array.IndexOf(lanes, lane) != currentLane)
            .Where((lane) =>
            {
                var pos = lane.path.GetClosestPointOnPath(transform.position);
                var localPos = transform.InverseTransformPoint(pos);

                if (amount < 0) return localPos.z < 0;
                return localPos.z > 0;
            })
            .OrderBy((lane) =>
            {
                var pos = lane.path.GetClosestPointOnPath(transform.position);
                var distance = Vector3.Distance(pos, transform.position);
                return distance;
            })
            .ToArray();
    }

    protected void ChangeLane(int amount)
    {
        var possible = GetPossibleLanes(amount);
        if (possible.Length == 0) return;

        int newLane = Array.IndexOf(lanes, possible[0]);

        if (newLane < 0 || newLane >= lanes.Length) return;

        Vector3 nextPoint = lanes[newLane].path.GetClosestPointOnPath(transform.position);
        float change = Vector3.Distance(transform.position, nextPoint);

        if (change > maxChange) return;

        currentLane = newLane;
        distance = lanes[currentLane].path.GetClosestDistanceAlongPath(transform.position);
        changeLaneRot = transform.rotation * Quaternion.Euler(new Vector3(40f * amount, 40f * amount, 0));

        StartCoroutine("HandleChangeLane", change * 0.2f);
        OnLaneChanged?.Invoke(amount, lanes[currentLane].name);
    }

    IEnumerator HandleBrake()
    {
        canBob = false;
        yield return new WaitForSeconds(0.5f);
        canBob = true;
        yield return null;
    }

    IEnumerator HandleChangeLane(float changeTime)
    {
        changingRotation = true;
        yield return new WaitForSeconds(changeTime);
        changingRotation = false;
        yield return null;
    }

    IEnumerator HandleDirectionChange(Quaternion nextRot, float angle)
    {
        OnDirectionChanged?.Invoke(lanes[currentLane]);

        var nextPos = lanes[currentLane].path.GetPointAtDistance(distance, EndOfPathInstruction.Loop);
        nextPos = transform.InverseTransformPoint(nextPos);

        float amount = angle * 0.01f;
        if (nextPos.z < 0) amount *= -1f;

        changingRotation = true;
        changeLaneRot = nextRot * Quaternion.Euler(new Vector3(40f * amount, 0, 10f));
        yield return new WaitForSeconds(0.3f);
        changingRotation = false;
        yield return null;
    }

    public string GetCurrentLaneName()
    {
        return lanes[currentLane].name;
    }

    public float GetDistance()
    {
        return distance;
    }
}
