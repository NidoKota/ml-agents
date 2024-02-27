using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class UCAgent : Agent
{
    [SerializeField] private Rigidbody _poleRb;
    [SerializeField] private Transform _handCollider;
    [SerializeField] private float _speed;

    private Vector3 _defaultPolePos;
    private Animator _animator;
    private readonly int _animXKey = Animator.StringToHash("X");
    private readonly int _animYKey = Animator.StringToHash("Y");
    private readonly int _animPXKey = Animator.StringToHash("PX");
    private readonly int _animPYKey = Animator.StringToHash("PY");

    private Vector2 _animXYValue;
    private Vector2 _animXYValueVelocity;
    private Vector2 _animXYValueAccel;

    private int _sleepFlame;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _defaultPolePos = _poleRb.transform.position;
    }

    public override void OnEpisodeBegin()
    {
        ResetData();
        _sleepFlame = 0;
    }

    private void ResetData()
    {
        _poleRb.velocity = Vector3.zero;
        _poleRb.angularVelocity = new Vector3(GetRandomValue(1f), 0, GetRandomValue(1f));

        _poleRb.transform.position = _defaultPolePos;
        _poleRb.rotation = Quaternion.identity;

        _animXYValue = Vector2.zero;
        _animXYValueVelocity = Vector2.zero;
        _animXYValueAccel = Vector2.zero;

        _animator.SetFloat(_animXKey, 0);
        _animator.SetFloat(_animYKey, 0);
        _animator.SetFloat(_animPXKey, 0);
        _animator.SetFloat(_animPYKey, 0);
    }

    private float GetRandomValue(float clamp)
    {
        return (Random.value - 0.5f) * 2 * clamp;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_poleRb.transform.up);
        sensor.AddObservation(_handCollider.localPosition.x);
        sensor.AddObservation(_handCollider.localPosition.y);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxisRaw("Horizontal");
        actions[1] = Input.GetAxisRaw("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (_sleepFlame < 5)
        {
            ResetData();
            _sleepFlame++;
            return;
        }

        var continuousActionVector = new Vector2(actionBuffers.ContinuousActions[0], actionBuffers.ContinuousActions[1]);
        // var continuousActionVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (continuousActionVector.sqrMagnitude > 1) continuousActionVector = continuousActionVector.normalized;

        _animXYValue += continuousActionVector * _speed  * Time.fixedDeltaTime;

        float animXYValueMagnitude = _animXYValue.magnitude;
        _animXYValue = Mathf.Clamp01(animXYValueMagnitude) * _animXYValue.normalized;

        _animator.SetFloat(_animXKey, _animXYValue.x);
        _animator.SetFloat(_animYKey, _animXYValue.y);
        _animator.SetFloat(_animPXKey, _poleRb.transform.up.x);
        _animator.SetFloat(_animPYKey, _poleRb.transform.up.z);

        var poleTra = _poleRb.transform;

        float halfScale = poleTra.localScale.y / 2;
        Vector3 upPosition = poleTra.position + poleTra.up * halfScale;
        Vector3 defaultDownPosition = _defaultPolePos + Vector3.down * halfScale;

        if (upPosition.y > (defaultDownPosition.y + upPosition.y) / 2)
        {
            AddReward(0.01f);
        }
        else
        {
            EndEpisode();
        }
    }
}
