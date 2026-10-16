using System;
using PromeRotation.Data;

namespace LittleDart.Gunbreaker.Planners;

public sealed record GnbPlanStep(string Name, Func<bool> Precondition, Func<PAction?> Intent, bool Optional = false, int IntentCount = 1);
