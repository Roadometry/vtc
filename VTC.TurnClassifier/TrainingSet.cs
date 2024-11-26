using System;
using System.Collections.Generic;
using System.Text;
using VTC.Common;

namespace VTC.TurnClassifier
{
    class TrainingSet
    {
        Dictionary<Input, Turn> TrainingExamples;
        Dictionary<Input, Turn> TestExamples;
    }
}
