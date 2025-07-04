﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Lhu : TypeIInstruction {

    public Lhu() {
        OpCode = 0b100101;
        ParseOptions = PopulationOptions.Rs | PopulationOptions.Rt | PopulationOptions.Immediate;
    }

    [GeneratedRegex(@"^\s*lhu\s+\$(?<rt>\S+),\s*(?<immediate>([-+]?\d+)|((0x|0X)?[0-9A-Fa-f]+))\(\$(?<rs>\S+)\)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rt)}, {Immediate}(${TranslateRegisterName(Rs)})" + FormatTrivia();
}
