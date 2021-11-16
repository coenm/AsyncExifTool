namespace CoenM.ExifToolLib.Internals.Guards
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides methods to protect against invalid parameters.
    /// </summary>
    [DebuggerStepThrough]
    [ExcludeFromCodeCoverage]
    internal static class Guard
    {
        /// <summary>
        /// Ensures that the value is not null.
        /// </summary>
        /// <typeparam name="T">Type of the target <paramref name="value"/>.</typeparam>
        /// <param name="value">The target object, which cannot be null.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value is null)
            {
                ThrowArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Ensures that the target value is not null, empty, or whitespace.
        /// </summary>
        /// <param name="value">The target string, which should be checked against being null, empty or whitespace.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or contains only blanks.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            if (value is null)
            {
                ThrowArgumentNullException(parameterName);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                ThrowArgumentException("Must not be empty or whitespace.", parameterName);
            }
        }

        /// <summary>
        /// Ensures that the target value is not null, or empty.
        /// </summary>
        /// <param name="value">The target string, which should be checked against being null or empty.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            if (value is null)
            {
                ThrowArgumentNullException(parameterName);
            }

            // ReSharper disable once PossibleNullReferenceException
            if (string.IsNullOrEmpty(value))
            {
                ThrowArgumentException("Must not be empty .", parameterName);
            }
        }

        /// <summary>
        /// Ensures that the enumeration is not null or empty.
        /// </summary>
        /// <typeparam name="T">The type of objects in the <paramref name="value"/>.</typeparam>
        /// <param name="value">The target enumeration, which should be checked against being null or empty.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("=> value:notnull")]
        public static void NotNullOrEmpty<T>(ICollection<T> value, string parameterName)
        {
            if (value is null)
            {
                ThrowArgumentNullException(parameterName);
            }

            // ReSharper disable once PossibleNullReferenceException
            if (value.Count == 0)
            {
                ThrowArgumentException("Must not be empty.", parameterName);
            }
        }

        /// <summary>
        /// Ensures that the nullable <paramref name="value"/> has a value.
        /// </summary>
        /// <typeparam name="T">Type of the target <paramref name="value"/>.</typeparam>
        /// <param name="value">The target object, which cannot be null.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HasValue<T>(T? value, string parameterName)
            where T : struct
        {
            if (!value.HasValue)
            {
                ThrowArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Ensures that the specified value is less than a maximum value.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is greater than the maximum value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustBeLessThan<TValue>(TValue value, TValue max, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value.CompareTo(max) >= 0)
            {
                ThrowArgumentOutOfRangeException(parameterName, $"Value {value} must be less than {max}.");
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustBeLessThanOrEqualTo<TValue>(TValue value, TValue max, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value.CompareTo(max) > 0)
            {
                ThrowArgumentOutOfRangeException(parameterName, $"Value {value} must be less than or equal to {max}.");
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustBeGreaterThan<TValue>(TValue value, TValue min, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value.CompareTo(min) <= 0)
            {
                ThrowArgumentOutOfRangeException(
                                                 parameterName,
                                                 $"Value {value} must be greater than {min}.");
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustBeGreaterThanOrEqualTo<TValue>(TValue value, TValue min, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value.CompareTo(min) < 0)
            {
                ThrowArgumentOutOfRangeException(
                                                 parameterName,
                                                 $"Value {value} must be greater than or equal to {min}.");
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustBeBetweenOrEqualTo<TValue>(TValue value, TValue min, TValue max, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                ThrowArgumentOutOfRangeException(
                                                 parameterName,
                                                 $"Value {value} must be greater than or equal to {min} and less than or equal to {max}.");
            }
        }

        /// <summary>
        /// Verifies that the specified <paramref name="value1"/> is equal to <paramref name="value2"/> and throws an exception if it is not.
        /// </summary>
        /// <param name="value1">Fist value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value1"/> is not equal to <paramref name="value2"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustBeEqualTo<TValue>(TValue value1, TValue value2, string parameterName)
            where TValue : IComparable<TValue>
        {
            if (value1.CompareTo(value2) != 0)
            {
                ThrowArgumentOutOfRangeException(parameterName, $"Value {value1} must be equal to {value2}.");
            }
        }

        /// <summary>
        /// Verifies, that the method parameter with specified target value is true
        /// and throws an exception if it is found to be so.
        /// </summary>
        /// <param name="target">The target value, which cannot be false.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <param name="message">The error message, if any to add to the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is false.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsTrue(bool target, string parameterName, string message)
        {
            if (!target)
            {
                ThrowArgumentException(message, parameterName);
            }
        }

        /// <summary>
        /// Verifies, that the method parameter with specified target value is false
        /// and throws an exception if it is found to be so.
        /// </summary>
        /// <param name="target">The target value, which cannot be true.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <param name="message">The error message, if any to add to the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="target"/> is true.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsFalse(bool target, string parameterName, string message)
        {
            if (target)
            {
                ThrowArgumentException(message, parameterName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentException(string message, string parameterName)
        {
            throw new ArgumentException(message, parameterName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentOutOfRangeException(string parameterName, string message)
        {
            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
