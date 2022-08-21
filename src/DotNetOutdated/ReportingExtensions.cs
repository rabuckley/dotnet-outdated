﻿using System.Collections.Generic;
using System.Linq;
using DotNetOutdated.Models;

namespace DotNetOutdated;

public static class ReportingExtensions
{
    public static int[] DetermineColumnWidths(this List<AnalyzedDependency> packages)
    {
        var columnWidths = new List<int>();
        columnWidths.Add(packages.Select(p => p.Description).Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length);
        columnWidths.Add(packages.Select(p => p.ResolvedVersion?.ToString() ?? "").Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length);
        columnWidths.Add(packages.Select(p => p.LatestVersion?.ToString() ?? "").Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length);

        return columnWidths.ToArray();
    }
}