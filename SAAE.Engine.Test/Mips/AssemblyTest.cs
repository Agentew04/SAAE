﻿using SAAE.Engine.Mips.Assembler;


namespace SAAE.Engine.Test.Mips;

[TestClass]
public class AssemblyTest {

    [TestMethod]
    public void TestTypeR() {
        string code = """
            add $t1, $zero, $s0
            sub $t1, $0, $s1
            or $s7, $k0, $1
            xor $27, $k1, $ra
            srav $t1, $t2, $a3
            sll $fp, $v0, 5
            """;
        byte[] expected = [
            0x20, 0x48, 0x10, 0x00,
            0x22, 0x48, 0x11, 0x00,
            0x25, 0xb8, 0x41, 0x03,
            0x26, 0xd8, 0x7f, 0x03,
            0x07, 0x48, 0xea, 0x00,
            0x40, 0xf1, 0x02, 0x00
        ];
        byte[] actual = new MipsAssembler().Assemble(code);

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestTypeRIJ() {
        string code = """
            addi $t0, $zero, 5
            addi $t1, $zero, 10
            sub $t2, $t1, $t0
            beq $t2, $t1, 0x2
            addi $s0, $zero, 1
            j 0x40001C
            addi $s0, $zero, 2
            """;
        byte[] expected = [
            0x05, 0x00, 0x08, 0x20,
            0x0A, 0x00, 0x09, 0x20,
            0x22, 0x50, 0x28, 0x01,
            0x02, 0x00, 0x48, 0x11,
            0x01, 0x00, 0x10, 0x20,
            0x01, 0x00, 0x10, 0x08,
            0x02, 0x00, 0x10, 0x20
        ];
        byte[] actual = new MipsAssembler().Assemble(code);
        string expectedHex = string.Join(" ", expected.Select(x => x.ToString("X2")));
        string actualHex = string.Join(" ", actual.Select(x => x.ToString("X2")));
        CollectionAssert.AreEqual(expected, actual, $"Expected: {expectedHex}; Actual: {actualHex}");
    }
}
