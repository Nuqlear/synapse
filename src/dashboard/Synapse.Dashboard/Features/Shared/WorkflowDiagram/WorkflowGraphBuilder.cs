﻿/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.JSInterop;
using Neuroglia.Blazor.Dagre.Models;
using ServerlessWorkflow.Sdk;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    public class WorkflowGraphBuilder
    //: IWorkflowGraphViewModelBuilder
    {

        protected readonly IJSRuntime jSRuntime;

        public WorkflowGraphBuilder(IJSRuntime jSRuntime)
        {
            this.jSRuntime = jSRuntime;
        }

        /// <inheritdoc/>
        public async Task<IGraphViewModel> BuildGraph(WorkflowDefinition definition)
        {
            var isEmpty = definition.States == null || !definition.States.Any();
            var graph = new GraphViewModel();
            //graph.RegisterBehavior(new DragAndDropNodeBehavior(graph, this.jSRuntime));
            var startNode = this.BuildStartNode(!isEmpty);
            var endNode = this.BuildEndNode();
            await graph.AddElementAsync(startNode);
            if (!isEmpty) { 
                var startState = definition.GetStartState();
                await this.BuildStateNodes(definition, graph, startState, endNode, startNode);
            }
            else
            {
                await this.BuildEdgeBetween(graph, startNode, endNode);
            }
            await graph.AddElementAsync(endNode);
            return await Task.FromResult(graph);
        }

        protected NodeViewModel BuildStartNode(bool hasSuccessor = false)
        {
            return new StartNodeViewModel(hasSuccessor);
        }

        protected async Task<StateNodeViewModel> BuildStateNodes(WorkflowDefinition definition, GraphViewModel graph, StateDefinition state, NodeViewModel endNode, NodeViewModel previousNode)
        {
            var stateNodeGroup = graph.AllClusters.Values.OfType<StateNodeViewModel>().FirstOrDefault(cluster => cluster.State.Name == state.Name);
            NodeViewModel? firstNode, lastNode = null;
            if (stateNodeGroup != null)
            {
                return stateNodeGroup;
            }
            else
            {
                stateNodeGroup = new StateNodeViewModel(state, state == definition.GetStartState());
                await graph.AddElementAsync(stateNodeGroup);
                switch (state)
                {
                    case CallbackStateDefinition callbackState:
                        {
                            var actionNodes = await this.BuildActionNodes(graph, callbackState.Action!);
                            lastNode = this.BuildConsumeEventNode(callbackState.Event!);
                            foreach (var actionNode in actionNodes)
                            {
                                await stateNodeGroup.AddChildAsync(actionNode);
                            }
                            await this.BuildEdgeBetween(graph, previousNode, actionNodes.First());
                            await this.BuildEdgeBetween(graph, actionNodes.Last(), lastNode);
                            await stateNodeGroup.AddChildAsync(lastNode);
                            break;
                        }
                    case EventStateDefinition eventState:
                        {
                            firstNode = this.BuildParellelNode();
                            lastNode = this.BuildParellelNode();
                            await stateNodeGroup.AddChildAsync(firstNode);
                            await this.BuildEdgeBetween(graph, previousNode, firstNode);
                            foreach (var trigger in eventState.Triggers)
                            {
                                var gatewayIn = this.BuildGatewayNode(eventState.Exclusive ? GatewayNodeType.Xor : GatewayNodeType.And);
                                var gatewayOut = this.BuildGatewayNode(eventState.Exclusive ? GatewayNodeType.Xor : GatewayNodeType.And);
                                await stateNodeGroup.AddChildAsync(gatewayIn);
                                await stateNodeGroup.AddChildAsync(gatewayOut);
                                await this.BuildEdgeBetween(graph, firstNode, gatewayIn);
                                foreach (var eventName in trigger.Events)
                                {
                                    var eventNode = this.BuildConsumeEventNode(eventName);
                                    await stateNodeGroup.AddChildAsync(eventNode);
                                    await this.BuildEdgeBetween(graph, gatewayIn, eventNode);
                                    await this.BuildEdgeBetween(graph, eventNode, gatewayOut);
                                }
                                foreach (var action in trigger.Actions)
                                {
                                    var actionsNodes = await this.BuildActionNodes(graph, action);
                                    foreach (var actionNode in actionsNodes)
                                    {
                                        await stateNodeGroup.AddChildAsync(actionNode);
                                    }
                                    await this.BuildEdgeBetween(graph, gatewayOut, actionsNodes.First());
                                    await this.BuildEdgeBetween(graph, actionsNodes.Last(), lastNode);
                                }
                            }
                            await stateNodeGroup.AddChildAsync(lastNode);
                            break;
                        }
                    case ForEachStateDefinition foreachState:
                        {
                            firstNode = this.BuildForEachNode(foreachState);
                            lastNode = this.BuildForEachNode(foreachState);
                            await stateNodeGroup.AddChildAsync(firstNode);
                            await this.BuildEdgeBetween(graph, previousNode, firstNode);
                            foreach (var action in foreachState.Actions)
                            {
                                var actionNodes = await this.BuildActionNodes(graph, action);
                                foreach (var actionNode in actionNodes)
                                {
                                    await stateNodeGroup.AddChildAsync(actionNode);
                                }
                                await this.BuildEdgeBetween(graph, firstNode, actionNodes.First());
                                await this.BuildEdgeBetween(graph, actionNodes.Last(), lastNode);
                            }
                            await stateNodeGroup.AddChildAsync(lastNode);
                            break;
                        }
                    case InjectStateDefinition injectState:
                        {
                            lastNode = this.BuildInjectNode(injectState);
                            await stateNodeGroup.AddChildAsync(lastNode);
                            await this.BuildEdgeBetween(graph, previousNode, lastNode);
                            break;
                        }
                    case OperationStateDefinition operationState:
                        {
                            switch (operationState.ActionMode)
                            {
                                case ActionExecutionMode.Parallel:
                                    firstNode = this.BuildParellelNode();
                                    lastNode = this.BuildParellelNode();
                                    await stateNodeGroup.AddChildAsync(firstNode);
                                    await this.BuildEdgeBetween(graph, previousNode, firstNode);
                                    foreach (var action in operationState.Actions)
                                    {
                                        var actionNodes = await this.BuildActionNodes(graph, action);
                                        foreach (var actionNode in actionNodes)
                                        {
                                            await stateNodeGroup.AddChildAsync(actionNode);
                                        }
                                        await this.BuildEdgeBetween(graph, firstNode, actionNodes.First());
                                        await this.BuildEdgeBetween(graph, actionNodes.Last(), lastNode);
                                    }
                                    await stateNodeGroup.AddChildAsync(lastNode);
                                    break;
                                case ActionExecutionMode.Sequential:
                                    lastNode = previousNode;
                                    foreach (var action in operationState.Actions)
                                    {
                                        var actionNodes = await this.BuildActionNodes(graph, action);
                                        foreach (var actionNode in actionNodes)
                                        {
                                            await stateNodeGroup.AddChildAsync(actionNode);
                                        }
                                        await this.BuildEdgeBetween(graph, lastNode, actionNodes.First());
                                        lastNode = actionNodes.Last();
                                    }
                                    break;
                                default:
                                    throw new Exception($"The specified action execution mode '{operationState.ActionMode}' is not supported");
                            }
                            break;
                        }
                    case ParallelStateDefinition parallelState:
                        {
                            firstNode = parallelState.CompletionType == ParallelCompletionType.AllOf ? this.BuildParellelNode() : this.BuildGatewayNode(GatewayNodeType.N);
                            lastNode = parallelState.CompletionType == ParallelCompletionType.AllOf ? this.BuildParellelNode() : this.BuildGatewayNode(GatewayNodeType.N);
                            await stateNodeGroup.AddChildAsync(firstNode);
                            await this.BuildEdgeBetween(graph, previousNode, firstNode);
                            foreach (var branch in parallelState.Branches)
                            {
                                foreach (var action in branch.Actions)
                                {
                                    var actionNodes = await this.BuildActionNodes(graph, action);
                                    foreach (var actionNode in actionNodes)
                                    {
                                        await stateNodeGroup.AddChildAsync(actionNode);
                                    }
                                    await this.BuildEdgeBetween(graph, firstNode, actionNodes.First());
                                    await this.BuildEdgeBetween(graph, actionNodes.Last(), lastNode);
                                }
                            }
                            await stateNodeGroup.AddChildAsync(lastNode);
                            break;
                        }
                    case SleepStateDefinition sleepState:
                        {
                            lastNode = this.BuildSleepNode(sleepState);
                            await stateNodeGroup.AddChildAsync(lastNode);
                            await this.BuildEdgeBetween(graph, previousNode, lastNode);
                            break;
                        }
                    case SwitchStateDefinition switchState:
                        {
                            firstNode = this.BuildGatewayNode(GatewayNodeType.Xor);
                            await stateNodeGroup.AddChildAsync(firstNode);
                            await this.BuildEdgeBetween(graph, previousNode, firstNode);
                            switch (switchState.SwitchType)
                            {
                                case SwitchStateType.Data:
                                    {
                                        foreach (var condition in switchState.DataConditions)
                                        {
                                            var caseNode = this.BuildDataConditionNode(condition.Name!); // todo: should be a labeled edge, not a node?
                                            await stateNodeGroup.AddChildAsync(caseNode);
                                            await this.BuildEdgeBetween(graph, firstNode, caseNode);
                                            switch (condition.Type)
                                            {
                                                case ConditionType.End:
                                                    await this.BuildEdgeBetween(graph, caseNode, endNode);
                                                    break;
                                                case ConditionType.Transition:
                                                    var nextStateName = condition.Transition == null ? condition.TransitionToStateName : condition.Transition.NextState;
                                                    var nextState = definition.GetState(nextStateName!);
                                                    if (nextState == null)
                                                        throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                                    var nextStateNode = await this.BuildStateNodes(definition, graph, nextState, endNode, caseNode);
                                                    lastNode = nextStateNode.Children.Values.OfType<NodeViewModel>().Last();
                                                    break;
                                                default:
                                                    throw new Exception($"The specified condition type '${condition.Type}' is not supported");
                                            }
                                        }
                                        var defaultCaseNode = this.BuildDataConditionNode("default");
                                        await stateNodeGroup.AddChildAsync(defaultCaseNode);
                                        await this.BuildEdgeBetween(graph, firstNode, defaultCaseNode);
                                        if (switchState.DefaultCondition.IsEnd
                                            || switchState.DefaultCondition.End != null)
                                        {
                                            lastNode = defaultCaseNode;
                                            if (!state.IsEnd && state.End == null)
                                            {
                                                await this.BuildEdgeBetween(graph, lastNode, endNode);
                                            }
                                        }
                                        else if (!string.IsNullOrWhiteSpace(switchState.DefaultCondition.TransitionToStateName)
                                            || switchState.DefaultCondition.Transition != null)
                                        {
                                            var nextStateName = switchState.DefaultCondition.Transition == null ? switchState.DefaultCondition.TransitionToStateName : switchState.DefaultCondition.Transition.NextState;
                                            var nextState = definition.GetState(nextStateName!);
                                            if (nextState == null)
                                                throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                            var nextStateNode = await this.BuildStateNodes(definition, graph, nextState, endNode, defaultCaseNode);
                                            lastNode = nextStateNode.Children.Values.OfType<NodeViewModel>().Last();
                                        }
                                    }
                                    break;
                                case SwitchStateType.Event:
                                    {
                                        foreach (var condition in switchState.EventConditions)
                                        {
                                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(condition));
                                            var caseNode = this.BuildDataConditionNode(condition.Name ?? condition.Event); // todo: should be a labeled edge, not a node?
                                            await stateNodeGroup.AddChildAsync(caseNode);
                                            await this.BuildEdgeBetween(graph, firstNode, caseNode);
                                            switch (condition.Type)
                                            {
                                                case ConditionType.End:
                                                    await this.BuildEdgeBetween(graph, caseNode, endNode);
                                                    break;
                                                case ConditionType.Transition:
                                                    var nextStateName = condition.Transition == null ? condition.TransitionToStateName : condition.Transition.NextState;
                                                    var nextState = definition.GetState(nextStateName!);
                                                    if (nextState == null)
                                                        throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                                    var nextStateNode = await this.BuildStateNodes(definition, graph, nextState, endNode, caseNode);
                                                    lastNode = nextStateNode.Children.Values.OfType<NodeViewModel>().Last();
                                                    break;
                                                default:
                                                    throw new Exception($"The specified condition type '${condition.Type}' is not supported");
                                            }
                                        }
                                        var defaultCaseNode = this.BuildDataConditionNode("default");
                                        await stateNodeGroup.AddChildAsync(defaultCaseNode);
                                        await this.BuildEdgeBetween(graph, firstNode, defaultCaseNode);
                                        if (switchState.DefaultCondition.IsEnd
                                            || switchState.DefaultCondition.End != null)
                                        {
                                            lastNode = defaultCaseNode;
                                            if (!state.IsEnd && state.End == null)
                                            {
                                                await this.BuildEdgeBetween(graph, lastNode, endNode);
                                            }
                                        }
                                        else if (!string.IsNullOrWhiteSpace(switchState.DefaultCondition.TransitionToStateName)
                                            || switchState.DefaultCondition.Transition != null)
                                        {
                                            var nextStateName = switchState.DefaultCondition.Transition == null ? switchState.DefaultCondition.TransitionToStateName : switchState.DefaultCondition.Transition.NextState;
                                            var nextState = definition.GetState(nextStateName!);
                                            if (nextState == null)
                                                throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}");
                                            var nextStateNode = await this.BuildStateNodes(definition, graph, nextState, endNode, defaultCaseNode);
                                            lastNode = nextStateNode.Children.Values.OfType<NodeViewModel>().Last();
                                        }
                                    }
                                    break;
                                default:
                                    throw new Exception($"The specified switch state type '{switchState.Type}' is not supported");
                            }
                            break;
                        }
                    default:
                        throw new Exception($"The specified state type '{state.Type}' is not supported");
                }
            }
            if (lastNode == null)
            {
                throw new Exception($"Unable to define a last node for state '{state.Name}'. Every switch case should provide a last node.");
            }
            if (state.IsEnd
                || state.End != null)
            {
                //Console.WriteLine($"State '{state.Name}' ends");
                await this.BuildEdgeBetween(graph, lastNode, endNode);
                return stateNodeGroup;
            }
            if (!string.IsNullOrWhiteSpace(state.TransitionToStateName)
                || state.Transition != null)
            {
                var nextStateName = state.Transition == null ? state.TransitionToStateName! : state.Transition!.NextState;
                //Console.WriteLine($"State '{state.Name}' transitions to '{nextStateName}'");
                var nextState = definition.GetState(nextStateName);
                if (nextState == null)
                    throw new Exception($"Failed to find a state with name '{nextStateName}' in definition '{definition.GetUniqueIdentifier()}'");
                var nextStateNode = await this.BuildStateNodes(definition, graph, nextState, endNode, lastNode);
                await this.BuildEdgeBetween(graph, lastNode, nextStateNode.Children.Values.OfType<NodeViewModel>().First());
                return stateNodeGroup;
            }
            //Console.WriteLine($"No transition for state '{state.Name}'");
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(state));
            return stateNodeGroup;
        }

        protected async Task<List<NodeViewModel>> BuildActionNodes(GraphViewModel graph, ActionDefinition action)
        {
            switch (action.Type)
            {
                case ActionType.Function:
                    return new() { this.BuildFunctionNode(action, action.Function!) };
                case ActionType.Subflow:
                    return new() { this.BuildSubflowNode(action.Subflow!) };
                case ActionType.Trigger:
                    var triggerEventNode = this.BuildProduceEventNode(action.Event!.ProduceEvent);
                    var resultEventNode = this.BuildConsumeEventNode(action.Event!.ResultEvent);
                    await this.BuildEdgeBetween(graph, triggerEventNode, resultEventNode);
                    return new() { triggerEventNode, resultEventNode };
                default:
                    throw new NotSupportedException($"The specified action type '{action.Type}' is not supported");
            }
        }

        protected FunctionRefNodeViewModel BuildFunctionNode(ActionDefinition action, FunctionReference function)
        {
            return new(action, function);
        }

        protected SubflowRefNodeViewModel BuildSubflowNode(SubflowReference subflowRef)
        {
            return new(subflowRef);
        }

        protected EventNodeViewModel BuildProduceEventNode(string refName)
        {
            return new(EventKind.Produced, refName);
        }

        protected EventNodeViewModel BuildConsumeEventNode(string refName)
        {
            return new(EventKind.Consumed, refName);
        }

        protected GatewayNodeViewModel BuildGatewayNode(GatewayNodeType gatewayNodeType)
        {
            return new(gatewayNodeType);
        }

        protected JunctionNodeViewModel BuildJunctionNode()
        {
            return new();
        }

        protected InjectNodeViewModel BuildInjectNode(InjectStateDefinition injectState)
        {
            return new(Newtonsoft.Json.JsonConvert.SerializeObject(injectState.Data, Newtonsoft.Json.Formatting.Indented));
        }

        protected ForEachNodeViewModel BuildForEachNode(ForEachStateDefinition forEachState)
        {
            return new();
        }

        protected ParallelNodeViewModel BuildParellelNode()
        {
            return new();
        }

        protected SleepNodeViewModel BuildSleepNode(SleepStateDefinition sleepState)
        {
            return new(sleepState.Duration);
        }

        protected DataCaseNodeViewModel BuildDataConditionNode(string caseDefinitionName)
        {
            return new(caseDefinitionName);
        }

        protected NodeViewModel BuildEndNode()
        {
            return new EndNodeViewModel();
        }


        /// <summary>
        /// Builds an edge between two nodes
        /// </summary>
        /// <param name="graph">The graph instance hosting the edge</param>
        /// <param name="source">The edge's source node</param>
        /// <param name="target">The edge's target node</param>
        /// <returns></returns>
        protected async Task BuildEdgeBetween(GraphViewModel graph, NodeViewModel source, NodeViewModel target)
        {
            await graph.AddElementAsync(new EdgeViewModel(source.Id, target.Id));
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}
