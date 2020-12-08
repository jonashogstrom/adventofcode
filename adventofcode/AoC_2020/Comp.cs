using System;
using System.Collections.Generic;

namespace AdventofCode.AoC_2020
{
    public class Comp
    {
        public List<Instruction> Program { get; } = new List<Instruction>();

        public Comp(IEnumerable<string> input)
        {
            foreach (var s in input)
            {
                var parts = s.Split(' ');
                Program.Add(new Instruction((OpCodex)Enum.Parse(typeof(OpCodex), parts[0]), int.Parse(parts[1])));
            }
        }

        public ExecutionState Execute(bool stopAtReExecute)
        {
            var accumulator = 0L;
            var instructions = new int[Program.Count];
            var ptr = 0;
            var lastJump = -1;
            var execCounter = 0;
            while (ptr < Program.Count)
            {
                if (!Overrides.TryGetValue(ptr, out var instr))
                    instr = Program[ptr];

                if (stopAtReExecute && instructions[ptr] != 0)
                {
                    return new ExecutionState(accumulator, true, ptr, lastJump);
                }

                execCounter++;
                instructions[ptr] = execCounter;

                switch (instr.OpCode)
                {
                    case OpCodex.nop:
                        ptr += 1;
                        break;
                    case OpCodex.jmp:
                        ptr += instr.Arg;
                        lastJump = ptr;
                        break;
                    case OpCodex.acc:
                        ptr += 1;
                        accumulator += instr.Arg;
                        break;
                }

            }

            return new ExecutionState(accumulator, false, ptr, lastJump);
        }

        public Dictionary<int, Instruction> Overrides = new Dictionary<int, Instruction>();
        public class Instruction
        {
            public OpCodex OpCode { get; }
            public int Arg { get; }

            public Instruction(OpCodex opCode, int arg)
            {
                OpCode = opCode;
                Arg = arg;
            }

            public Instruction Clone(OpCodex opCode)
            {
                return new Instruction(opCode, Arg);
            }
        }
    }

    public class ExecutionState
    {
        public long Accumulator { get; }
        public bool ReExecuted { get; }
        public int Ptr { get; }
        public int LastJump { get; }

        public ExecutionState(long accumulator, bool reExecuted, int ptr, int lastJump)
        {
            Accumulator = accumulator;
            ReExecuted = reExecuted;
            Ptr = ptr;
            LastJump = lastJump;
        }
    }

    public enum OpCodex
    {
        jmp, acc, nop
    }
}