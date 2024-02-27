using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public static class AgentModeUtility
{
    public enum AgentMode
    {
        Heuristic,
        Inference,
        Training
    }

    public static AgentMode GetAgentMode(this BehaviorParameters policyFactory)
    {
        if (policyFactory.IsInHeuristicMode())
            return AgentMode.Heuristic;

        if (policyFactory.BehaviorType == BehaviorType.InferenceOnly || policyFactory.BehaviorType == BehaviorType.Default && !Academy.Instance.IsCommunicatorOn)
            return AgentMode.Inference;

        return AgentMode.Training;
    }
}
