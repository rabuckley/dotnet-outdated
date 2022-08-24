// Copyright (c) Down Syndrome Education Enterprises CIC. All Rights Reserved.
// Information contained herein is PROPRIETARY AND CONFIDENTIAL.

namespace DotNetOutdated.Core;

public static class EnumerableExtensions
{
    public static async Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> enumeration, Func<T, Task<IEnumerable<TResult>>> func)
    {
        return (await Task.WhenAll(enumeration.Select(func)).ConfigureAwait(false)).SelectMany(s => s);
    }
}
