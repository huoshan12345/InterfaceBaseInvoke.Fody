﻿using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace InterfaceBaseInvoke.Tests.Support
{
    internal static class AssertionExtensions
    {
        public static void ShouldEqual<T>(this T? actual, T? expected)
            => Assert.Equal(expected, actual);

        public static void ShouldNotEqual<T>(this T? actual, T? expected)
            => Assert.NotEqual(expected, actual);

        public static void ShouldBeTrue(this bool actual)
            => Assert.True(actual);

        public static void ShouldBeFalse(this bool actual)
            => Assert.False(actual);
        
        public static void ShouldBeNull(this object? actual) => Assert.Null(actual);
        
        public static T ShouldNotBeNull<T>(this T? actual) where T : class
        {
            Assert.NotNull(actual);
            return actual ?? throw new NotNullException();
        }

        public static void ShouldAll<T>(this IEnumerable<T> items, Func<T, bool> test)
            => Assert.All(items, item => Assert.True(test(item)));

        public static void ShouldAny<T>(this IEnumerable<T> items, Func<T, bool> test)
            => Assert.Contains(items, item => test(item));

        public static void ShouldContain<T>(this IEnumerable<T> items, Func<T, bool> predicate)
            => Assert.Contains(items, item => predicate(item));

        public static void ShouldNotContain<T>(this IEnumerable<T> items, Func<T, bool> predicate)
            => Assert.DoesNotContain(items, item => predicate(item));

        public static T ShouldContainSingle<T>(this IEnumerable<T> items, Func<T, bool> predicate)
            => Assert.Single(items, item => predicate(item));
        
        public static T ShouldBe<T>(this object? item) => Assert.IsType<T>(item);

        public static void ShouldBeEmpty<T>(this IEnumerable<T> items) => Assert.Empty(items);

        public static void ShouldContain(this string str, string expectedSubstring)
            => Assert.Contains(expectedSubstring, str);

        public static void ShouldNotContain(this string str, string expectedSubstring)
            => Assert.DoesNotContain(expectedSubstring, str);
    }
}
