﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Xori : TypeIInstruction {

    public Xori() {
        OpCode = 0b001110;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*xori\s+\$(?<rt>\S+),\s*\$(?<rs>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, ${TranslateRegisterName(Rs)}, {Immediate}" + FormatTrivia();
}
