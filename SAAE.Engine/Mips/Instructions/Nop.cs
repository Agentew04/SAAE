﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Instructions; 
internal partial class Nop : Instruction {
    public override int ConvertToInt() {
        return 0;
    }

    [GeneratedRegex(@"nop\s*$")]
    public override partial Regex GetRegularExpression();
}
