﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Runtime; 
public class RegisterFile {

    private List<int> _registers;

    private List<Register> _changedRegisters = [];
    public List<Register> GetChangedRegisters() {
        List<Register> newList = [.._changedRegisters];
        _changedRegisters.Clear();
        return newList;
    }

    public RegisterFile() {
        _registers = new List<int>((int)Register.COUNT);
        _registers.AddRange(Enumerable.Repeat(0, (int)Register.COUNT));
    }

    public int Get(Register register) => Get((int)register);

    public int Get(int index) {
        if (index < 0 || index >= (int)Register.COUNT) {
            throw new ArgumentOutOfRangeException(nameof(index), "Register index out of bounds");
        }
        return _registers[index];
    }

    public void Set(Register register, int value) => Set((int)register, value);

    public void Set(int index, int value) {
        if (index is < 0 or >= (int)Register.COUNT) {
            throw new ArgumentOutOfRangeException(nameof(index), "Register index out of bounds");
        }
        if (_registers[index] != value && !_changedRegisters.Contains((Register)index)) {
            _changedRegisters.Add((Register)index);
        }
        _registers[index] = value;
    }

    public void Reset() {
        _registers = new List<int>((int)Register.COUNT);
    }

    public int this[int index] {
        get => Get(index);
        set => Set(index, value);
    }

    public int this[Register register] {
        get => Get(register);
        set => Set(register, value);
    }

    public enum Register {
        Zero = 0,
        At,
        V0,
        V1,
        A0,
        A1,
        A2,
        A3,
        T0,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        S0,
        S1,
        S2,
        S3,
        S4,
        S5,
        S6,
        S7,
        T8,
        T9,
        K0,
        K1,
        Gp,
        Sp,
        Fp,
        Ra,
        Pc,
        Hi,
        Lo,
        COUNT
    }
}
