﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Sw : TypeIInstruction {

    public Sw() {
        OpCode = 0b101011;
        ParseOptions = PopulationOptions.Rt | PopulationOptions.Rs | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*sw\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, {Immediate}(${TranslateRegisterName(Rs)})" + FormatTrivia();
}
