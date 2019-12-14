using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.Text;

using DataType = System.Int64;

namespace AdventofCode.AoC_2019
{

    public class IntCodeComputer
    {
        private readonly List<DataType> _input;
        private readonly Dictionary<int, DataType> _memory = new Dictionary<int, DataType>();
        private int endOfMemory;
        public int Pointer;
        public int RelativeBase;
        public int OpCounter;
        public int MemWriteCounter;

        public bool Terminated { get; set; }
        private readonly HashSet<int> _opAdresses = new HashSet<int>();
        private readonly HashSet<int> _paramAdresses = new HashSet<int>();
        private readonly HashSet<int> _dataAdresses = new HashSet<int>();

        private readonly Dictionary<int, IOperation> _ops = new Dictionary<int, IOperation>();
        private readonly List<DataType> _output = new List<DataType>();
        private readonly Queue<DataType> _outputQ = new Queue<DataType>();
        private bool _interrupt;
        private IntCodeComputer _outputTarget;
        public List<DataType> Output => _output;
        public Queue<DataType> OutputQ => _outputQ;
        public List<string> Log = new List<string>();
        private string _name;
        private bool _paramModifierWarningWritten;
        private bool _opModifierWarningWritten;

        public IntCodeComputer(string program): this(new List<long>(), program)
        {
        }

        public IntCodeComputer(List<DataType> input, string program, int noun = -1, int verb = -1)
        {
            _input = input;
            RegisterOp(new GenericIntFunc("ADD", 1, (i, j) => i + j));
            RegisterOp(new GenericIntFunc("MUL", 2, (i, j) => i * j));
            RegisterOp(new GenericIntFunc("LT", 7, (i, j) => i < j ? 1 : 0));
            RegisterOp(new GenericIntFunc("EQ", 8, (i, j) => i == j ? 1 : 0));
            RegisterOp(new Terminate());
            RegisterOp(new JumpIfFalse());
            RegisterOp(new JumpIfTrue());
            RegisterOp(new ReadInput());
            RegisterOp(new WriteOutput());
            RegisterOp(new AdjustRelBase());
            var prog = program.Split(',').Select(long.Parse).ToArray();
            for (int i = 0; i < prog.Length; i++)
                _memory[i] = prog[i];
            endOfMemory = prog.Length - 1;
            Pointer = 0;
            if (noun != -1)
                _memory[1] = noun;
            if (verb != -1)
                _memory[2] = verb;
            Terminated = false;
            Name = "IntCodeComputer";
            Log.Add("============= Starting IntCodeComputer ==============");
        }

        private void RegisterOp(IOperation op)
        {
            _ops[op.OpCode] = op;
        }

        public void ConnectInpOutp(IntCodeComputer target)
        {
            _outputTarget = target;
        }

        public void AddInput(DataType value)
        {
            _input.Add(value);
        }

        public void Execute()
        {
            _interrupt = false;
            while (!Terminated && !_interrupt)
            {
                if (Pointer < 0)
                    throw new Exception($"Pointer outside of memory: {Pointer}");
                if (Pointer > endOfMemory)
                    throw new Exception($"Pointer outside of memory: {Pointer}");
                var opCode = (int)_memory[Pointer];
                _opAdresses.Add(Pointer);
                if (Debug && _dataAdresses.Contains(Pointer))
                {
                    Log.Add("Reading op from a data address: "+Pointer);
                }
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
                    else if (modesString[i] == '2')
                        modes[mPos] = ParameterMode.relative;
                    else if (modesString[i] == '0')
                        modes[mPos] = ParameterMode.position;
                    else
                    {
                        throw new Exception("unknown parameter mode");
                    }

                    mPos++;
                }

                var args = GetArgs(op.ArgCount);
                var parameters = GetParams(args, modes);
                if (Debug)
                {
                    var sb = new StringBuilder();

                    sb.Append($"{Pointer} {op.Name}: ");
                    for (int i = 0; i < op.ArgCount; i++)
                    {
                        if (modes[i] == ParameterMode.immediate)
                            sb.Append($"Arg{i} I: {args[i]} ");
                        else if (modes[i] == ParameterMode.relative)
                            sb.Append($"Arg{i} R: {args[i]} [{parameters[i]}]");
                        else
                        {
                            sb.Append($"Arg{i} P: {args[i]} [{parameters[i]}] ");
                        }
                    }
                }

                op.Execute(this, modes, args, parameters);
                if (!_interrupt)
                {
                 //   Log.Add(sb.ToString());
                    OpCounter++;
                }

            }
        }


        protected DataType[] GetArgs(int argCount)
        {
            var args = new DataType[argCount];
            for (int i = 0; i < argCount; i++)
            {
                var addr = Pointer + i + 1;
                args[i] = ReadMemory(addr, ReadOp.arg);
                if (Debug && _dataAdresses.Contains(addr))
                {
                    Log.Add("Reading arg from a data address: " + addr);
                }

            }

            return args;
        }

        //        protected int[] GetParams(IntCodeComputer comp, ParameterMode[] modes)
        //        {
        //            return GetParams(comp, GetArgs(comp, ArgCount), modes);
        //        }

        protected DataType[] GetParams(DataType[] args, ParameterMode[] modes)
        {
            var res = new DataType[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                if (modes.Length - 1 < i)
                    res[i] = ReadMemory((int)args[i], ReadOp.data); // implicit position mode
                else if (modes[i] == ParameterMode.immediate)
                    res[i] = args[i];
                else if (modes[i] == ParameterMode.relative)
                    res[i] = ReadMemory((int)(RelativeBase + args[i]), ReadOp.data);
                else if (modes[i] == ParameterMode.position)
                    res[i] = ReadMemory((int)args[i], ReadOp.data);
            }

            return res;
        }

        public DataType? GetNextInput()
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

        public void WriteOutput(DataType value)
        {
            _output.Add(value);
            _outputQ.Enqueue(value);
            //Console.WriteLine("Current output: " + _output);
            LastOutput = value;
            _outputTarget?.AddInput(value);
        }

        public DataType LastOutput { get; private set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Log.Add("Computer name: " + value);
            }
        }

        public bool Debug { get; set; }

        public DataType ReadMemory(int addr, ReadOp readOp)
        {
            switch (readOp)
            {
                case ReadOp.arg:
                    _paramAdresses.Add(addr);
                    break;
                case ReadOp.op:
                    _opAdresses.Add(addr);
                    break;
                case ReadOp.data:
                    _dataAdresses.Add(addr);
                    break;
            }
            if (!_memory.ContainsKey(addr))
                return 0;
            return _memory[addr];
        }

        public void WriteMemory(int addr, DataType value)
        {
            _dataAdresses.Add(addr);
            if (_opAdresses.Contains(addr) && !_paramModifierWarningWritten)
            {
                Console.WriteLine("Warning, writing to an op-address");
                _paramModifierWarningWritten = true;
            }

            if (_paramAdresses.Contains(addr) && !_opModifierWarningWritten)
            {
                Console.WriteLine("Warning, writing to a param-address: "+addr);
                _opModifierWarningWritten = true;
            }
            _memory[addr] = value;
            if (Debug)
                Log.Add($"Write {value} to addr {addr}");
            endOfMemory = Math.Max(endOfMemory, addr);
            MemWriteCounter++;
        }
    }

    internal class ReadInput : BaseOp, IOperation
    {
        public override string Name => "INP";

        public int OpCode => 3;
        public override int ArgCount => 1;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            var value = comp.GetNextInput();
            if (!value.HasValue)
                return;

            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            if (modes[0] == ParameterMode.relative)
                comp.WriteMemory((int)args[0] + comp.RelativeBase, value.Value);
            else
                comp.WriteMemory((int)args[0], value.Value);
            comp.Pointer += ArgCount + 1;
        }
    }

    internal class WriteOutput : BaseOp, IOperation
    {
        public override string Name => "OUT";
        public int OpCode => 4;
        public override int ArgCount => 1;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            // Console.wLine($"{a1} ({Name}) {a2} => {res}");
            comp.WriteOutput(parameters[0]);
            comp.Pointer += ArgCount + 1;
        }
    }

    internal class JumpIfTrue : BaseOp, IOperation
    {
        public override string Name => "JMP1";
        public int OpCode => 5;
        public override int ArgCount => 2;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            if (parameters[0] != 0)
                comp.Pointer = (int)parameters[1];
            else
                comp.Pointer += ArgCount + 1;
        }
    }

    internal class AdjustRelBase : BaseOp, IOperation
    {
        public override string Name => "AdjR";
        public int OpCode => 9;
        public override int ArgCount => 1;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            comp.RelativeBase += (int)parameters[0];
            comp.Pointer += ArgCount + 1;
        }
    }


    internal class JumpIfFalse : BaseOp, IOperation
    {
        public override string Name => "JMP0";

        public int OpCode => 6;
        public override int ArgCount => 2;

        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            if (parameters[0] == 0)
                comp.Pointer = (int)parameters[1];
            else
                comp.Pointer += ArgCount + 1;
        }
    }


    public enum ParameterMode
    {
        position,
        immediate,
        relative
    }

    internal abstract class IntFunc : BaseOp, IOperation
    {
        protected IntFunc(int opCode)
        {
            OpCode = opCode;
        }

        public int OpCode { get; }
        public override int ArgCount => 3;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            var a1 = parameters[0];
            var a2 = parameters[1];
            var res = calc(a1, a2);
            // Console.WriteLine($"{a1} ({Name}) {a2} => {res}");
            if (modes[2] == ParameterMode.relative)
            {
                comp.WriteMemory((int)args[2] + comp.RelativeBase, res);
            }
            else

                comp.WriteMemory((int)args[2], res);
            comp.Pointer += ArgCount + 1;
        }

        protected abstract DataType calc(DataType arg1, DataType arg2);
    }

    internal class GenericIntFunc : IntFunc
    {
        private readonly Func<DataType, DataType, DataType> _func;

        public override string Name { get; }

        public GenericIntFunc(string name, int opCode, Func<DataType, DataType, DataType> func) : base(opCode)
        {
            Name = name;
            _func = func;
        }

        protected override DataType calc(DataType arg1, DataType arg2)
        {
            return _func(arg1, arg2);
        }
    };

    public abstract class BaseOp
    {
        public abstract int ArgCount { get; }
        public virtual string Name => this.GetType().Name;
    }

    public enum ReadOp
    {
        op, arg, data
    }

    internal class Terminate : BaseOp, IOperation
    {
        public override string Name => "XIT";
        public int OpCode => 99;
        public override int ArgCount => 0;
        public void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters)
        {
            comp.Terminated = true;
        }
    }

    interface IOperation
    {
        string Name { get; }
        int OpCode { get; }
        int ArgCount { get; }
        void Execute(IntCodeComputer comp, ParameterMode[] modes, DataType[] args, DataType[] parameters);
    }
}