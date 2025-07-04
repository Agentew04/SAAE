﻿using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public partial class Rotrv : TypeRInstruction {

    public Rotrv() {
        Function = 0b000110;
        ShiftAmount = 0b00001;
        ParseOptions = PopulationOptions.Rd | PopulationOptions.Rs | PopulationOptions.Rt;
    }

    [GeneratedRegex(@"^\s*rotrv\s+\$(?<rd>\S+)\s*,\s*\$(?<rt>\S+)\s*,\s*\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rd)}, ${TranslateRegisterName(Rt)}, ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
