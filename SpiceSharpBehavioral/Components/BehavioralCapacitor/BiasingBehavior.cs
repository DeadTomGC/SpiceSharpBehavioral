﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCapacitor"/>
    /// </summary>
    public class BiasingBehavior : Behavior
    {
        /// <summary>
        /// The variables.
        /// </summary>
        protected readonly OnePort<double> Variables;

        /// <summary>
        /// Gets the variables that are associated with each variable node.
        /// </summary>
        protected Dictionary<VariableNode, IVariable<double>> DerivativeVariables { get; }

        /// <summary>
        /// The function that computes the value.
        /// </summary>
        protected readonly Node Function;

        /// <summary>
        /// The functions that compute the derivatives.
        /// </summary>
        protected readonly Dictionary<VariableNode, Node> Derivatives;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public BiasingBehavior(BehavioralBindingContext context)
            : base(context)
        {
            // Make sure that we have access to the voltage over the behavior
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            Variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            // Create the derivatives, while also giving access to the voltage across the capacitor
            var replacer = new NodeReplacer
            {
                Map = new Dictionary<VariableNode, Node>(new VariableNodeComparer(null, null, bp.VariableComparer))
                {
                    { Node.Variable("x"), Node.Voltage(context.Nodes[0]) - Node.Voltage(context.Nodes[1]) }
                }
            };
            Function = replacer.Build(bp.Function);
            Derivatives = context.CreateDerivatives(Function);
            DerivativeVariables = new Dictionary<VariableNode, IVariable<double>>(Derivatives.Comparer);
            foreach (var key in Derivatives.Keys)
                DerivativeVariables.Add(key, context.MapNode(state, key));
        }
    }
}
