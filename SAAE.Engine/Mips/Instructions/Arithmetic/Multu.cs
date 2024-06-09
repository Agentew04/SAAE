﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;
public partial class Multu : TypeRInstruction {

    public Multu() {
        Rd = 0;
        Function = 0b011001;
    }

    [GeneratedRegex(@"^\s*multu\s+\$(?<rs>\S+?)\s*,\s*\$(?<rt>\S+?)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rs = byte.Parse(m.Groups["rs"].Value);
        Rt = byte.Parse(m.Groups["rt"].Value);
    }
}
