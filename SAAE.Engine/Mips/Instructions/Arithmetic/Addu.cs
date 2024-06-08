﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions; 

public partial class Addu : TypeRInstruction {
    public Addu() {
        Function = 0x21;
        ShiftAmount = 0;
    }

    [GeneratedRegex(@"^\s*addu\s+\$(?<rd>\S+)\s*,\s*\$(?<rs>\S+)\s*,\s*\$(?<rt>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rd = byte.Parse(m.Groups["rd"].Value);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
