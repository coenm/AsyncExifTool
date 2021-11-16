namespace CoenM.ExifToolLib.Internals.Guards
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides methods to protect against invalid parameters for a DEBUG build.
    /// </summary>
    [DebuggerStepThrough]
    [ExcludeFromCodeCoverage]
    internal static class DebugGuard
    {
        /// <summary>
        /// Verifies, that the method parameter with specified object value is not null
        /// and throws an exception if it is found to be so.
        /// </summary>
        /// <typeparam name="T">Type of the target <paramref name="value"/>.</typeparam>
        /// <param name="value">The target object, which cannot be null.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        [Conditional("DEBUG")]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNull<T>(T value, string parameterName)
            where T : class
        {
            Guard.NotNull(value, parameterName);
        }

        /// <summary>
        /// Ensures that the target value is not null, empty, or whitespace.
        /// </summary>
        /// <param name="value">The target string, which should be checked against being null, empty or whitespace.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or contains only blanks.</exception>
        [Conditional("DEBUG")]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            Guard.NotNullOrWhiteSpace(value, parameterName);
        }

        /// <summary>
        /// Ensures that the target value is not null, or empty.
        /// </summary>
        /// <param name="value">The target string, which should be checked against being null or empty.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
        [Conditional("DEBUG")]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            Guard.NotNullOrEmpty(value, parameterName);
        }

        /// <summary>
        /// Ensures that the enumeration is not null or empty.
        /// </summary>
        /// <typeparam name="T">The type of objects in the <paramref name="value"/>.</typeparam>
        /// <param name="value">The target enumeration, which should be checked against being null or empty.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
        [Conditional("DEBUG")]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNullOrEmpty<T>(ICollection<T> value, string parameterName)
        {
            Guard.NotNullOrEmpty(value, parameterName);
        }

        /// <summary>
        /// Ensures that the nullable <paramref name="value"/> has a value.
        /// </summary>
        /// <typeparam name="T">Type of the target <paramref name="value"/>.</typeparam>
        /// <param name="value">The target object, which cannot be null.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        [Conditional("DEBUG")]
        public static void HasValue<T>(T? value, string parameterName)
            where T : struct
        {
            Guard.HasValue(value, parameterName);
        }

        /// <summary>
        /// Verifies that the specified value is less than a maximum value
        /// and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is greater than the maximum value.</exception>
        [Conditional("DEBUG")]
        public static void MustBeLessThan<TValue>(TValue value, TValue max, string parameterName)
            where TValue : IComparable<TValue>
        {
            Guard.MustBeLessThan(value, max, parameterName);
        }

        /// <summary>
        /// Verifies that the specified value is less than or equal to a maximum value
        /// and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is greater than the maximum value.</exception>
        [Conditional("DEBUG")]
        public static void MustBeLessThanOrEqualTo<TValue>(TValue value, TValue max, string parameterName)
            where TValue : IComparable<TValue>
        {
            Guard.MustBeLessThanOrEqualTo(value, max, parameterName);
        }

        /// <summary>
        /// Verifies that the specified value is greater than a minimum value
        /// and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than the minimum value.</exception>
        [Conditional("DEBUG")]
        public static void MustBeGreaterThan<TValue>(TValue value, TValue min, string parameterName)
            where TValue : IComparable<TValue>
        {
            Guard.MustBeGreaterThan(value, min, parameterName);
        }

        /// <summary>
        /// Verifies that the specified value is greater than or equal to a minimum value
        /// and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than the minimum value.</exception>
        [Conditional("DEBUG")]
        public static void MustBeGreaterThanOrEqualTo<TValue>(TValue value, TValue min, string parameterName)
            where TValue : IComparable<TValue>
        {
            Guard.MustBeGreaterThanOrEqualTo(value, min, parameterName);
        }

        /// <summary>
        /// Verifies that the specified value is greater than or equal to a minimum value and less than
        /// or equal to a maximum value and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than the minimum value of greater than the maximum value.</exception>
        [Conditional("DEBUG")]
        public static void MustBeBetweenOrEqualTo<TValue>(TValue value, TValue min, TValue max, string parameterName)
           where TValue : IComparable<TValue>
        {
            Guard.MustBeBetweenOrEqualTo(value, min, max, parameterName);
        }

        /// <summary>
        /// Verifies that the specified <paramref name="value1"/> is equal to <paramref name="value2"/> and throws an exception if it is not.
        /// </summary>
        /// <param name="value1">Fist value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value1"/> is not equal to <paramref name="value2"/>.</exception>
        [Conditional("DEBUG")]
        public static void MustBeEqualTo<TValue>(TValue value1, TValue value2, string parameterName)
            where TValue : IComparable<TValue>
        {
            Guard.MustBeEqualTo(value1, value2, parameterName);
        }

        /// <summary>
        /// Verifies, that the method parameter with specified target value is <c>true</c> and throws an exception if it is found to be so.
        /// </summary>
        /// <param name="target">The target value, which cannot be false.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <param name="message">The error message, if any to add to the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is false.</exception>
        [Conditional("DEBUG")]
        public static void IsTrue(bool target, string parameterName, string message)
        {
            Guard.IsTrue(target, parameterName, message);
        }

        /// <summary>
        /// Verifies, that the method parameter with specified target value is <c>false</c>
        /// and throws an exception if it is found to be so.
        /// </summary>
        /// <param name="target">The target value, which cannot be true.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <param name="message">The error message, if any to add to the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is true.</exception>
        [Conditional("DEBUG")]
        public static void IsFalse(bool target, string parameterName, string message)
        {
            Guard.IsFalse(target, parameterName, message);
        }
    }
}
