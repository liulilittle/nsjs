namespace nsjsdotnet.Core.Utilits
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class MSIL
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly static IDictionary<int, OpCode> opcodes = 
            typeof(OpCodes).GetFields().Select(fi => (OpCode)fi.GetValue(null)).ToDictionary((op) => (int)op.Value);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly static IList<OpCode> k__BackingField_Get1 = new List<OpCode>()
        {
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Ret,
        };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly static IList<OpCode> k__BackingField_Get2 = new List<OpCode>()
        {
            OpCodes.Ldarg_0,
            OpCodes.Ldfld,
            OpCodes.Stloc_0,
            OpCodes.Br_S,
            OpCodes.Ldloc_0,
            OpCodes.Ret,
        };
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly static IList<OpCode> k__BackingField_Set1 = new List<OpCode>()
        {
            OpCodes.Ldarg_0,
            OpCodes.Ldarg_1,
            OpCodes.Stfld,
            OpCodes.Ret,
        };

        public class Instruction
        {
            public long? Value
            {
                get;
                set;
            }

            public long Alignment
            {
                get;
                set;
            }

            public OpCode OpCode
            {
                get;
                set;
            }

            public override string ToString()
            {
                return OpCode.ToString();
            }
        }

        public static bool IsAnaemiaGetAttribute(PropertyInfo info)
        {
            FieldInfo backingfield;
            return IsAnaemiaGetAttribute(info, out backingfield);
        }

        public static bool IsAnaemiaGetAttribute(PropertyInfo info, out FieldInfo backingfield)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            backingfield = null;
            MethodInfo get = info.GetGetMethod();
            if (get == null)
            {
                return true;
            }
            IList<Instruction> instructions = GetInstructions(get);
            if (GetApproximateDegree(instructions, k__BackingField_Get1) == k__BackingField_Get1.Count ||
                GetApproximateDegree(instructions, k__BackingField_Get2) == k__BackingField_Get2.Count)
            {
                Instruction instruction = instructions.FirstOrDefault(i => i.OpCode == OpCodes.Ldfld);
                if (instruction != null)
                {
                    Module module = info.Module;
                    backingfield = module.ResolveField((int)instruction.Value);
                }
                return true;
            }
            return false;
        }

        public static bool IsAnaemiaSetAttribute(PropertyInfo info)
        {
            FieldInfo backingfield;
            return IsAnaemiaSetAttribute(info, out backingfield);
        }

        public static bool IsAnaemiaSetAttribute(PropertyInfo info, out FieldInfo backingfield)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            backingfield = null;
            MethodInfo set = info.GetSetMethod();
            if (set == null)
            {
                return true;
            }
            IList<Instruction> instructions = GetInstructions(set);
            if (GetApproximateDegree(instructions, k__BackingField_Set1) == k__BackingField_Set1.Count)
            {
                Instruction instruction = instructions.FirstOrDefault(i => i.OpCode == OpCodes.Stfld);
                if (instruction != null)
                {
                    Module module = info.Module;
                    backingfield = module.ResolveField((int)instruction.Value);
                }
                return true;
            }
            return false;
        }

        public static bool IsAnaemiaAttribute(PropertyInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            return IsAnaemiaGetAttribute(info) || IsAnaemiaSetAttribute(info);
        }

        private static int GetApproximateDegree(IList<Instruction> instructions, IList<OpCode> compares)
        {
            int count = 0;
            for (int i = 0, j = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode == OpCodes.Nop)
                {
                    continue;
                }
                if (j >= compares.Count)
                {
                    break;
                }
                if (compares[j++] == instructions[i].OpCode)
                {
                    count++;
                }
            }
            return count;
        }

        public static IList<Instruction> GetInstructions(MethodInfo m)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            IList<Instruction> instructions = new List<Instruction>();
            MethodBody body = m.GetMethodBody();
            if (body == null)
            {
                return instructions;
            }
            byte[] il = body.GetILAsByteArray();
            int ofs = 0;
            while (ofs < il.Length)
            {
                int start = ofs;
                short op = il[ofs];
                ofs++;
                if (op == 0xFE || opcodes[op].OpCodeType == OpCodeType.Prefix)
                {
                    op = (short)((op << 8) + il[ofs]);
                    ofs++;
                }
                OpCode code = opcodes[op];
                long? argument = null;
                int align = 4;
                if (code.OperandType == OperandType.InlineNone)
                {
                    align = 0;
                }
                else if (code.OperandType == OperandType.ShortInlineBrTarget || code.OperandType == OperandType.ShortInlineI || code.OperandType == OperandType.ShortInlineVar)
                {
                    align = 1;
                }
                else if (code.OperandType == OperandType.InlineVar)
                {
                    align = 2;
                }
                else if (code.OperandType == OperandType.InlineI8 || code.OperandType == OperandType.InlineR)
                {
                    align = 8;
                }
                else if (code.OperandType == OperandType.InlineSwitch)
                {
                    long n = il[ofs] + (il[ofs + 1] << 8) + (il[ofs + 2] << 16) + (il[ofs + 3] << 24);
                    align = (int)(4 * n + 4);
                }
                if (align > 0)
                {
                    long n = 0;
                    for (int i = 0; i < align; ++i)
                    {
                        long v = il[ofs + i];
                        n += v << (i * 8);
                    }
                    argument = n;
                    ofs += align;
                }
                instructions.Add(new Instruction() { OpCode = code, Value = argument, Alignment = align });
            }
            return instructions;
        }
    }
}
