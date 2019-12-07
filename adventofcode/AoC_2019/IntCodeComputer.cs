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
        private readonly List<int> _input;
        private readonly int[] _memory;
        public int Pointer;
        public bool Terminated { get; set; }
        private readonly HashSet<int> _opAdresses = new HashSet<int>();
        private readonly HashSet<int> _paramAdresses = new HashSet<int>();
        private readonly HashSet<int> _dataAdresses = new HashSet<int>();

        private readonly Dictionary<int, IOperation> _ops = new Dictionary<int, IOperation>();
        private readonly List<int> _output = new List<int>();
        private bool _interrupt;
        private IntCodeComputer _outputTarget;
        public List<int> Output => _output;

        public IntCodeComputer(List<int> input, string program, int noun = -1, int verb = -1)
        {
            _input = input;
            RegisterOp(new GenericIntFunc("+", 1, (i, j) => i + j));
            RegisterOp(new GenericIntFunc("*", 2, (i, j) => i * j));
            RegisterOp(new GenericIntFunc("<", 7, (i, j) => i < j ? 1 : 0));
            RegisterOp(new GenericIntFunc("=", 8, (i, j) => i == j ? 1 : 0));
            RegisterOp(new Terminate());
            RegisterOp(new JumpIfFalse());
            RegisterOp(new JumpIfTrue());
            RegisterOp(new ReadInput());
            RegisterOp(new WriteOutput());
            _memory = program.Split(',').Select(int.Parse).ToArray();
            Pointer = 0;
            if (noun != -1)
                _memory[1] = noun;
            if (verb != -1)
                _memory[2] = verb;
            Terminated = false;
            Name = "IntCodeComputer";
        }

        private void RegisterOp(IOperation op)
        {
            _ops[op.OpCode] = op;
        }

        public void ConnectInpOutp(IntCodeComputer target)
        {
            _outputTarget = target;
        }

        public void AddInput(int value)
        {
            _input.Add(value);
        }

        public void Execute()
        {
            _interrupt = false;
            while (!Terminated && !_interrupt)
            {
                if (Pointer >= _memory.Length)
                    throw new Exception($"Pointer outside of memory: {Pointer}");
                var opCode = _memory[Pointer];
                _opAdresses.Add(Pointer);
                var op2Code = opCode % 100;

                if (!_ops.ContainsKey(op2Code))
                    throw new Exception($"Unknown operation: {_memory[Pointer]}");
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
            _outputTarget?.AddInput(value);
        }

        public int LastOutput { get; private set; }
        public string Name { get; set; }

        public int ReadMemory(int pos, ReadOp readOp)
        {
            switch (readOp)
            {
                case ReadOp.arg:
                    _paramAdresses.Add(pos);
                    break;
                case ReadOp.op:
                    _opAdresses.Add(pos);
                    break;
                case ReadOp.data:
                    _dataAdresses.Add(pos);
                    break;
            }
            return _memory[pos];
        }

        public void WriteMemory(int addr, int value)
        {
            _dataAdresses.Add(addr);
            if (_opAdresses.Contains(addr))
                Console.WriteLine("Warning, writing to an op-address");
            if (_paramAdresses.Contains(addr))
                Console.WriteLine("Warning, writing to a param-address");
            _memory[addr] = value;
        }
    }

    internal class ReadInput : BaseOp, IOperation
    {
        public override string Name => "INP";

        public int OpCode => 3;
        public override int ArgCount => 1;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var value = comp.GetNextInput();
            if (!value.HasValue)
                return;
            var args = GetArgs(comp, ArgCount);

            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.WriteMemory(args[0], value.Value);
            comp.Pointer += ArgCount + 1;
        }
    }

    internal class WriteOutput : BaseOp, IOperation
    {
        public override string Name => "OUT";
        public int OpCode => 4;
        public override int ArgCount => 1;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var parameters = GetParams(comp, modes);

            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.WriteOutput(parameters[0]);
            comp.Pointer += ArgCount + 1;
        }
    }

    internal class JumpIfTrue : BaseOp, IOperation
    {
        public override string Name => "JMP";
        public int OpCode => 5;
        public override int ArgCount => 2;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var parameters = GetParams(comp, modes);

            if (parameters[0] != 0)
                comp.Pointer = parameters[1];
            else
                comp.Pointer += ArgCount + 1;
        }
    }



    internal class JumpIfFalse : BaseOp, IOperation
    {
        public override string Name => "JM!";

        public int OpCode => 6;
        public override int ArgCount => 2;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var parameters = GetParams(comp, modes);

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
        public override int ArgCount => 3;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes)
        {
            var args = GetArgs(comp, ArgCount);
            var parameters = GetParams(comp, args, modes);

            var a1 = parameters[0];
            var a2 = parameters[1];
            var res = calc(a1, a2);
            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            comp.WriteMemory(args[2], res);
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

    public abstract class BaseOp
    {
        public abstract int ArgCount { get; }
        public virtual string Name => this.GetType().Name;

        protected int[] GetArgs(IntCodeComputer comp, int argCount)
        {
            var args = new int[argCount];
            for (int i = 0; i < argCount; i++)
            {
                args[i] = comp.ReadMemory(comp.Pointer + i + 1, ReadOp.arg);
            }

            return args;
        }

        protected int[] GetParams(IntCodeComputer comp, ParameterMode[] modes)
        {
            return GetParams(comp, GetArgs(comp, ArgCount), modes);
        }

        protected int[] GetParams(IntCodeComputer comp, int[] args, ParameterMode[] modes)
        {
            var res = new int[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                if (modes.Length - 1 < i)
                    res[i] = comp.ReadMemory(args[i], ReadOp.data); // implicit position mode
                else if (modes[i] == ParameterMode.immediate)
                    res[i] = args[i];
                else if (modes[i] == ParameterMode.position)
                    res[i] = comp.ReadMemory(args[i], ReadOp.data);
            }

            return res;
        }


    }

    public enum ReadOp
    {
        op, arg, data
    }

    internal class Terminate : BaseOp, IOperation
    {
        public override string Name => "X";
        public int OpCode => 99;
        public override int ArgCount => 0;
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