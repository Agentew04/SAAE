﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Srl : TypeRInstruction {

    public Srl() {
        Function = 0b000010;
        Rs = 0;
    }

    [GeneratedRegex(@"srl\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*(?<shamt>\d+)\s*$")]
    public override partial Regex GetRegularExpression();
}
