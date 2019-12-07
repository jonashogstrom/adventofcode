using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Policy;

namespace AdventofCode.AoC_2019
{
    public class IntCodeComputer
    {
        private List<int> _input;
        public int[] Memory;
        public int Pointer;
        public bool Terminated { get; set; }

        private readonly Dictionary<int, IOperation> _ops = new Dictionary<int, IOperation>();
        private List<int> _output =new List<int>();
        private bool _interrupt;
        private IntCodeComputer _target;
        public List<int> Output => _output;

        public IntCodeComputer(List<int> input, string program, int noun, int verb)
        {
            _input = input;
            RegisterOp(new GenericIntFunc("Add", 1, (i, j) => i + j));
            RegisterOp(new GenericIntFunc("Mul", 2, (i, j) => i * j));
            RegisterOp(new GenericIntFunc("LessThan", 7, (i, j) => i < j ? 1 : 0));
            RegisterOp(new GenericIntFunc("Equal", 8, (i, j) => i == j ? 1 : 0));
            RegisterOp(new Terminate());
            RegisterOp(new jumpIfFalse());
            RegisterOp(new jumpIfTrue());
            RegisterOp(new ReadInput());
            RegisterOp(new WriteOutput());
            Memory = program.Split(',').Select(int.Parse).ToArray();
            Pointer = 0;
            if (noun != -1)
                Memory[1] = noun;
            if (verb != -1)
                Memory[2] = verb;
            Terminated = false;
        }

        private void RegisterOp(IOperation op)
        {
            _ops[op.OpCode] = op;
        }

        public void ConnectInpOutp(IntCodeComputer target)
        {
            _target = target;
        }

        public void RunProgram()
        {
            // Console.WriteLine($"======================= {noun} {verb}");
       

            Execute();
        }

        public void AddInput(int value)
        {
            _input.Add(value);
            //Execute();
        }

        public void Execute()
        {
            _interrupt = false;
            while (!Terminated && !_interrupt)
            {
                if (Pointer >= Memory.Length)
                    throw new Exception($"Pointer outside of memory: {Pointer}");
                var opCode = Memory[Pointer];
                var op2Code = opCode % 100;

                if (!_ops.ContainsKey(op2Code))
                    throw new Exception($"Unknown operation: {Memory[Pointer]}");
                var op = _ops[op2Code];

                var modesString = opCode.ToString();
                var modes = new ParameterMode[op.ArgCount];
                var mPos = 0;
                for (var i = modesString.Length - 3; i >= 0; i--)
                {
                    if (modesString[i] == '1')
                        modes[mPos] = ParameterMode.immediate;
                    else if (modesString[i] == '0')
                        modes[mPos] = ParameterMode.position;
                    else
                    {
                        throw new Exception("unknown parameter mode");
                    }

                    mPos++;
                }

                op.Execute(this, modes);
            }
        }

        public int? GetNextInput()
        {
            if (_input.Any())
            {
                var res = _input.First();
                _input.RemoveAt(0);
                return res;
            }

            _interrupt = true;
            return null;
//            throw new Exception("no input available");
        }

        public void WriteOutput(int value)
        {
            _output.Add(value);
            //Console.WriteLine("Current output: " + _output);
            LastOutput = value;
            _target?.AddInput(value);
        }

        public int LastOutput { get; private set; }
    }

    internal class ReadInput : BaseOp, IOperation
    {
        public int OpCode => 3;
        public int ArgCount => 1;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var value = comp.GetNextInput();
            if (!value.HasValue)
                return;
            var args = GetArgs(comp, ArgCount);

            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.Memory[args[0]] = value.Value;
            comp.Pointer += ArgCount + 1;
        }
    }

    internal class WriteOutput : BaseOp, IOperation
    {
        public int OpCode => 4;
        public int ArgCount => 1;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var parameters = GetParams(comp, ArgCount, modes);

            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.WriteOutput(parameters[0]);
            comp.Pointer += ArgCount + 1;
        }
    }

    internal class jumpIfTrue : BaseOp, IOperation
    {
        public int OpCode => 5;
        public int ArgCount => 2;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var parameters = GetParams(comp, ArgCount, modes);

            if (parameters[0] != 0)
                comp.Pointer = parameters[1];
            else
                comp.Pointer += ArgCount + 1;
        }
    }



    internal class jumpIfFalse : BaseOp, IOperation
    {
        public int OpCode => 6;
        public int ArgCount => 2;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var parameters = GetParams(comp, ArgCount, modes);

            if (parameters[0] == 0)
                comp.Pointer = parameters[1];
            else
                comp.Pointer += ArgCount + 1;
        }
    }


    public enum ParameterMode
    {
        position,
        immediate
    }


    internal abstract class IntFunc : BaseOp, IOperation
    {
        protected IntFunc(int opCode)
        {
            OpCode = opCode;
        }

        public int OpCode { get; }
        public int ArgCount => 3;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var args = GetArgs(comp, ArgCount);
            var parameters = GetParams(comp, ArgCount, modes);

            var a1 = parameters[0];
            var a2 = parameters[1];
            var res = calc(a1, a2);
            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.Memory[args[2]] = res;
            comp.Pointer += ArgCount + 1;
        }

        protected abstract int calc(int arg1, int arg2);
    }

    internal class GenericIntFunc : IntFunc
    {
        private readonly Func<int, int, int> _func;

        public override string Name { get; }

        public GenericIntFunc(string name, int opCode, Func<int, int, int> func) : base(opCode)
        {
            Name = name;
            _func = func;
        }

        protected override int calc(int arg1, int arg2)
        {
            return _func(arg1, arg2);
        }
    };

    public class BaseOp
    {
        public virtual string Name => this.GetType().Name;

        protected int[] GetArgs(IntCodeComputer comp, int argCount)
        {
            var args = new int[argCount];
            for (int i = 0; i < argCount; i++)
            {
                args[i] = comp.Memory[comp.Pointer + i + 1];
            }

            return args;
        }

        protected int[] GetParams(IntCodeComputer comp, int argCount, ParameterMode[] modes)
        {
            var args = GetArgs(comp, argCount);
            var res = new int[argCount];
            for (var i = 0; i < argCount; i++)
            {
                if (modes.Length - 1 < i)
                    res[i] = comp.Memory[args[i]]; // implicit position mode
                else if (modes[i] == ParameterMode.immediate)
                    res[i] = args[i];
                else if (modes[i] == ParameterMode.position)
                    res[i] = comp.Memory[args[i]];
            }

            return res;
        }


    }

    internal class Terminate : BaseOp, IOperation
    {
        public int OpCode => 99;
        public int ArgCount => 0;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            comp.Terminated = true;
        }
    }

    interface IOperation
    {
        string Name { get; }
        int OpCode { get; }
        int ArgCount { get; }
        void Execute(IntCodeComputer comp, ParameterMode[] modes);
    }
}