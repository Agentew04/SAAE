﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions; 

public partial class Addu : TypeRInstruction {
    public Addu() {
        Function = 0x21;
        ShiftAmount = 0;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*addu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
}
