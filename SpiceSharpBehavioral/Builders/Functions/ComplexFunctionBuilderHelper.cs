﻿using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Helper methods for a <see cref="RealFunctionBuilder"/>.
    /// </summary>
    public static class ComplexFunctionBuilderHelper
    {
        private static readonly Random _rnd = new Random();
        private static readonly MethodInfo
            _sgn = ((Func<double, int>)Math.Sign).GetMethodInfo(),
            _round = ((Func<Complex, int, Complex>)Functions.Round).GetMethodInfo(),
            _pwl = ((Func<double, Point[], double>)Functions.Pwl).GetMethodInfo(),
            _pwlDerivative = ((Func<double, Point[], double>)Functions.PwlDerivative).GetMethodInfo();
        private static readonly ConstructorInfo _point = typeof(Point).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });

        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static readonly Dictionary<string, ApplyFunction<Complex>> Defaults = new Dictionary<string, ApplyFunction<Complex>>(StringComparer.OrdinalIgnoreCase)
        {
            { "abs", Abs },
            { "sgn", Sgn },
            { "sqrt", Sqrt },
            { "pow", Pow },
            { "pwr", Pwr },
            { "log", Log }, { "ln", Log },
            { "pwrs", Pwrs },
            { "log10", Log10 },
            { "exp", Exp },
            { "sin", Sin },
            { "cos", Cos },
            { "tan", Tan },
            { "sinh", Sinh },
            { "cosh", Cosh },
            { "tanh", Tanh },
            { "asin", Asin }, { "arcsin", Asin },
            { "acos", Acos }, { "arccos", Acos },
            { "atan", Atan }, { "arctan", Atan },
            { "atan2", Atan2 },
            { "hypot", Hypot },
            { "u", U }, { "du(0)", Zero },
            { "u2", U2 }, { "du2(0)", DU2 },
            { "uramp", URamp }, { "duramp(0)", DURampDerivative },
            { "ceil", Ceil },
            { "floor", Floor },
            { "nint", Nint },
            { "round", Round },
            { "square", Square },
            { "pwl", Pwl }, { "dpwl(0)", PwlDerivative },
            { "table", Pwl }, { "dtable(0)", PwlDerivative },
            { "tbl", Pwl }, { "dtbl(0)", PwlDerivative },
            { "min", Min },
            { "max", Max },
            { "rnd", Random }, { "rand", Random },
            { "if", If }
        };

        private static IReadOnlyList<Node> Check(this IReadOnlyList<Node> nodes, int expected)
        {
            if (nodes.Count != expected)
                throw new ArgumentMismatchException(expected, nodes.Count);
            return nodes;
        }

        /// <summary>
        /// Registers the default functions.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        public static void RegisterDefaultFunctions(this Dictionary<string, ApplyFunction<Complex>> definitions)
        {
            definitions.ThrowIfNull(nameof(definitions));
            foreach (var pair in Defaults)
                definitions.Add(pair.Key, pair.Value);
        }

        // No-argument functions
        private static void Random(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            ils.Call(() => new Complex(_rnd.NextDouble(), 0));
        }

        // One-argument functions
        private static void Zero(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Push(new Complex());
        private static void Abs(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Abs, arguments);
        private static void Sgn(IILState<Complex> ils, IReadOnlyList<Node> arguments) { ils.Call(null, _sgn, arguments.Check(1)); ils.Generator.Emit(OpCodes.Conv_R8); }
        private static void Sqrt(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Sqrt, arguments);
        private static void URamp(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Ramp, arguments);
        private static void DURampDerivative(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.RampDerivative, arguments);
        private static void U(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step, arguments);
        private static void U2(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step2, arguments);
        private static void DU2(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step2Derivative, arguments);
        private static void Sin(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Sin, arguments);
        private static void Cos(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Cos, arguments);
        private static void Tan(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Tan, arguments);
        private static void Asin(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Asin, arguments);
        private static void Acos(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Acos, arguments);
        private static void Atan(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Atan, arguments);
        private static void Sinh(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Sinh, arguments);
        private static void Cosh(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Cosh, arguments);
        private static void Tanh(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Tanh, arguments);
        private static void Ceil(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Ceiling, arguments);
        private static void Floor(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Floor, arguments);
        private static void Exp(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Exp, arguments);
        private static void Log(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Log, arguments);
        private static void Log10(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Log10, arguments);
        private static void Square(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            ils.Call(Functions.Square, arguments);
        }
        private static void Nint(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            ils.Push(arguments.Check(1)[0]); 
            ils.PushInt(0);
            ils.Generator.Emit(OpCodes.Call, _round);
        }

        // Two-argument functions
        private static void Pow(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Complex.Pow, arguments);
        private static void Pwr(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Power, arguments);
        private static void Pwrs(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Power2, arguments);
        private static void Min(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Min, arguments);
        private static void Max(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Max, arguments);
        private static void Round(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            ils.Push(arguments.Check(2)[0]);
            ils.Push(arguments[1]);
            ils.Generator.Emit(OpCodes.Conv_I4);
            ils.Generator.Emit(OpCodes.Call, _round);
        }
        private static void Atan2(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Atan2, arguments);
        private static void Hypot(IILState<Complex> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Hypot, arguments);

        // Three-argument functions
        private static void If(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            var ilsc = (IILComplexState)ils;
            arguments.Check(3);
            ilsc.PushReal(arguments[0]);
            ilsc.PushDouble(0.5);
            ilsc.PushCheck(OpCodes.Bgt_S, arguments[1], arguments[2]);
        }

        // N-argument functions
        private static void Pwl(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = ils.Generator;
            var ilsc = (IILComplexState)ils;

            // Create our array
            ilsc.PushReal(arguments[0]);
            ils.PushInt(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                ils.PushInt(i); // Set the index

                // Create the point
                ilsc.PushReal(arguments[i * 2 + 1]);
                ilsc.PushReal(arguments[i * 2 + 2]);
                il.Emit(OpCodes.Newobj, _point);

                // Store the element
                il.Emit(OpCodes.Stelem, typeof(Point));
            }
            il.Emit(OpCodes.Call, _pwl);
            ilsc.RealToComplex();
        }
        private static void PwlDerivative(IILState<Complex> ils, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = ils.Generator;
            var ilsc = (IILComplexState)ils;

            // Create our array
            ilsc.PushReal(arguments[0]);
            ils.PushInt(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                ils.PushInt(i); // Set the index

                // Create the point
                ilsc.PushReal(arguments[i * 2 + 1]);
                ilsc.PushReal(arguments[i * 2 + 2]);
                il.Emit(OpCodes.Newobj, _point);

                // Store the element
                il.Emit(OpCodes.Stelem, typeof(Point));
            }
            il.Emit(OpCodes.Call, _pwlDerivative);
            ilsc.RealToComplex();
        }
    }
}
