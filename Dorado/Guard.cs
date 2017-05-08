using Dorado.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Dorado
{
    public class Guard
    {
        private const string AgainstMessage = "argument evaluation failed with 'false'.";
        private const string ImplementsMessage = "Type '{0}' must implement type '{1}'.";
        private const string InheritsFromMessage = "Type '{0}' must inherit from type '{1}'.";
        private const string IsTypeOfMessage = "Type '{0}' must be of type '{1}'.";
        private const string IsEqualMessage = "Compared objects must be equal.";
        private const string IsPositiveMessage = "Argument '{0}' must be a positive value. Value: '{1}'.";
        private const string IsTrueMessage = "True expected for '{0}' but the condition was False.";
        private const string NotNegativeMessage = "Argument '{0}' cannot be a negative value. Value: '{1}'.";

        private Guard()
        {
        }

        /// <summary>
        /// Throws proper exception if the class reference is null.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value">Class reference to check.</param>
        /// <exception cref="InvalidOperationException">If class reference is null.</exception>
        [DebuggerStepThrough]
        public static void NotNull<TValue>(Func<TValue> value)
        {
            if (value() == null)
                throw new InvalidOperationException("'{0}' cannot be null.".FormatInvariant(value));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        [DebuggerStepThrough]
        public static void ArgumentNotNull<T>(Func<T> arg)
        {
            if (arg() == null)
                throw new ArgumentNullException(GetParamName(arg));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2>(Func<T1> arg1, Func<T2> arg2)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2, T3>(Func<T1> arg1, Func<T2> arg2, Func<T3> arg3)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));

            if (arg3() == null)
                throw new ArgumentNullException(GetParamName(arg3));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2, T3, T4>(Func<T1> arg1, Func<T2> arg2, Func<T3> arg3, Func<T4> arg4)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));

            if (arg3() == null)
                throw new ArgumentNullException(GetParamName(arg3));

            if (arg4() == null)
                throw new ArgumentNullException(GetParamName(arg4));
        }

        [DebuggerStepThrough]
        public static void Arguments<T1, T2, T3, T4, T5>(Func<T1> arg1, Func<T2> arg2, Func<T3> arg3, Func<T4> arg4, Func<T5> arg5)
        {
            if (arg1() == null)
                throw new ArgumentNullException(GetParamName(arg1));

            if (arg2() == null)
                throw new ArgumentNullException(GetParamName(arg2));

            if (arg3() == null)
                throw new ArgumentNullException(GetParamName(arg3));

            if (arg4() == null)
                throw new ArgumentNullException(GetParamName(arg4));

            if (arg5() == null)
                throw new ArgumentNullException(GetParamName(arg5));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Func<string> arg)
        {
            if (arg().IsNullOrWhiteSpace())
                throw Error.ArgumentNullOrEmpty(arg);
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Func<Guid> arg)
        {
            if (arg() == Guid.Empty)
            {
                string argName = GetParamName(arg);
                throw Error.Argument(argName, "Argument '{0}' cannot be an empty guid.", argName);
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Func<IEnumerable> arg)
        {
            if (!arg().HasItems())
            {
                string argName = GetParamName(arg);
                throw Error.Argument(argName, "List cannot be null and must have at least one item.");
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(string arg, string argName)
        {
            if (arg.IsNullOrWhiteSpace())
                throw Error.Argument(argName, "String parameter '{0}' cannot be null or all whitespace.", argName);
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(IEnumerable arg, string argName)
        {
            if (!arg.HasItems())
                throw Error.Argument(argName, "List cannot be null and must have at least one item.");
        }

        [DebuggerStepThrough]
        public static void ArgumentNotEmpty(Guid arg, string argName)
        {
            if (arg == Guid.Empty)
                throw Error.Argument(argName, "Argument '{0}' cannot be an empty guid.", argName);
        }

        [DebuggerStepThrough]
        public static void ArgumentInRange<T>(T arg, T min, T max, string argName) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(min) < 0 || arg.CompareTo(max) > 0)
                throw Error.ArgumentOutOfRange(argName, "The argument '{0}' must be between '{1}' and '{2}'.", argName, min, max);
        }

        [DebuggerStepThrough]
        public static void ArgumentNotOutOfLength(string arg, int maxLength, string argName)
        {
            if (arg.Trim().Length > maxLength)
            {
                throw Error.Argument(argName, "Argument '{0}' cannot be more than {1} characters long.", argName, maxLength);
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentNotNegative<T>(T arg, string argName, string message = NotNegativeMessage) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) < 0)
                throw Error.ArgumentOutOfRange(argName, message.FormatInvariant(argName, arg));
        }

        [DebuggerStepThrough]
        public static void ArgumentNotZero<T>(T arg, string argName) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) == 0)
                throw Error.ArgumentOutOfRange(argName, "Argument '{0}' must be greater or less than zero. Value: '{1}'.", argName, arg);
        }

        [DebuggerStepThrough]
        public static void Against<TException>(bool argument, string message = AgainstMessage) where TException : Exception
        {
            if (argument)
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        public static void Against<TException>(Func<bool> argument, string message = AgainstMessage) where TException : Exception
        {
            //Execute the lambda and if it evaluates to true then throw the exception.
            if (argument())
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        public static void InheritsFrom<TBase>(Type type)
        {
            InheritsFrom<TBase>(type, InheritsFromMessage.FormatInvariant(type.FullName, typeof(TBase).FullName));
        }

        [DebuggerStepThrough]
        public static void InheritsFrom<TBase>(Type type, string message)
        {
            if (type.BaseType != typeof(TBase))
                throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
        public static void Implements<TInterface>(Type type, string message = ImplementsMessage)
        {
            if (!typeof(TInterface).IsAssignableFrom(type))
                throw new InvalidOperationException(message.FormatInvariant(type.FullName, typeof(TInterface).FullName));
        }

        [DebuggerStepThrough]
        public static void IsTypeOf<TType>(object instance)
        {
            IsTypeOf<TType>(instance, IsTypeOfMessage.FormatInvariant(instance.GetType().Name, typeof(TType).FullName));
        }

        [DebuggerStepThrough]
        public static void IsTypeOf<TType>(object instance, string message)
        {
            if (!(instance is TType))
                throw new InvalidOperationException(message);
        }

        [DebuggerStepThrough]
        public static void IsEqual<TException>(object compare, object instance, string message = IsEqualMessage) where TException : Exception
        {
            if (!compare.Equals(instance))
                throw (TException)Activator.CreateInstance(typeof(TException), message);
        }

        [DebuggerStepThrough]
        public static void ArgumentIsPositive<T>(T arg, string argName, string message = IsPositiveMessage) where T : struct, IComparable<T>
        {
            if (arg.CompareTo(default(T)) < 1)
                throw Error.ArgumentOutOfRange(argName, message.FormatInvariant(argName));
        }

        [DebuggerStepThrough]
        public static void ArgumentIsTrue(bool arg, string argName, string message = IsTrueMessage)
        {
            if (!arg)
                throw Error.Argument(argName, message.FormatInvariant(argName));
        }

        [DebuggerStepThrough]
        public static void ArgumentIsEnumType(Type arg, string argName)
        {
            ArgumentNotNull(arg, argName);
            if (!arg.IsEnum)
                throw Error.Argument(argName, "Type '{0}' must be a valid Enum type.", arg.FullName);
        }

        [DebuggerStepThrough]
        public static void ArgumentIsEnumType(Type enumType, object arg, string argName)
        {
            ArgumentNotNull(arg, argName);
            if (!Enum.IsDefined(enumType, arg))
            {
                throw Error.ArgumentOutOfRange(argName, "The value of the argument '{0}' provided for the enumeration '{1}' is invalid.", argName, enumType.FullName);
            }
        }

        [DebuggerStepThrough]
        public static void PagingArgsValid(int indexArg, int sizeArg, string indexArgName, string sizeArgName)
        {
            ArgumentNotNegative<int>(indexArg, indexArgName, "PageIndex cannot be below 0");
            if (indexArg > 0)
            {
                // if pageIndex is specified (> 0), PageSize CANNOT be 0
                ArgumentIsPositive<int>(sizeArg, sizeArgName, "PageSize cannot be below 1 if a PageIndex greater 0 was provided.");
            }
            else
            {
                // pageIndex 0 actually means: take all!
                ArgumentNotNegative(sizeArg, sizeArgName);
            }
        }

        [DebuggerStepThrough]
        private static string GetParamName<T>(Expression<Func<T>> expression)
        {
            string name = string.Empty;
            MemberExpression body = expression.Body as MemberExpression;

            if (body != null)
            {
                name = body.Member.Name;
            }

            return name;
        }

        [DebuggerStepThrough]
        private static string GetParamName<T>(Func<T> expression)
        {
            return expression.Method.Name;
        }

        public static void ArgumentValuesNotNull<T>(params T[] values) where T : class
        {
            if (values.Any(t => t == null))
            {
                throw new ArgumentNullException();
            }
        }

        public static void ArgumentValuesNotNull<T>(string paramName, params T[] values) where T : class
        {
            if (values.Any(t => t == null))
            {
                throw new ArgumentNullException("Argument name: " + paramName);
            }
        }

        public static void ArgumentNotNull<T>(T value, string paramName) where T : class
        {
            ArgumentValuesNotNull<T>(paramName, new T[]
            {
                value
            });
        }

        public static void ArgumentValuesNotNull<T>(params T?[] values) where T : struct
        {
            if (values.Any(t => !t.HasValue))
            {
                throw new ArgumentNullException();
            }
        }

        public static void ArgumentNotNull<T>(T value) where T : class
        {
            ArgumentValuesNotNull<T>(new T[]
            {
                value
            });
        }

        public static void ArgumentValuesNotNull<T>(string paramName, params T?[] values) where T : struct
        {
            if (values.Any(t => !t.HasValue))
            {
                throw new ArgumentNullException("Argument name: " + paramName);
            }
        }

        public static void ArgumentNotNull<T>(T? value, string paramName) where T : struct
        {
            ArgumentValuesNotNull<T>(paramName, new T?[]
            {
                value
            });
        }

        public static void ArgumentNotNull<T>(T? value) where T : struct
        {
            ArgumentValuesNotNull<T>(new T?[]
            {
                value
            });
        }

        public static void ArgumentValuesNotEmpty(params string[] values)
        {
            foreach (string text in values)
            {
                if (text == null)
                {
                    throw new ArgumentNullException();
                }
                if (text.Length == 0)
                {
                    throw new ArgumentOutOfRangeException(string.Empty, "String is empty.");
                }
            }
        }

        public static void ArgumentNotEmpty(string value)
        {
            ArgumentValuesNotEmpty(new string[]
            {
                value
            });
        }

        public static void ArgumentNotEmpty(IEnumerable arg)
        {
            if (!arg.HasItems())
            {
                throw new ArgumentException("List cannot be null and must have at least one item.");
            }
        }

        public static void ArgumentValuesPositive(params int[] values)
        {
            foreach (int num in values)
            {
                if (num <= 0)
                {
                    throw new ArgumentOutOfRangeException("Argument not positive. Value: (" + num + ").");
                }
            }
        }

        public static void ArgumentValuesPositive(string paramName, params int[] values)
        {
            foreach (int num in values.Where(num => num <= 0))
            {
                throw new ArgumentOutOfRangeException(string.Concat(new object[]
                {
                    "Argument not positive. Argument name: ",
                    paramName,
                    "; Value: ",
                    num,
                    "."
                }));
            }
        }

        public static void ArgumentPositive(int value, string paramName)
        {
            ArgumentValuesPositive(paramName, new int[]
            {
                value
            });
        }

        public static void ArgumentPositive(int value)
        {
            ArgumentValuesPositive(new int[]
            {
                value
            });
        }

        public static void ArgumentInRange(string paramName, int value, int rangeBegin, int rangeEnd)
        {
            if (value < rangeBegin || value > rangeEnd)
            {
                throw new ArgumentOutOfRangeException(paramName, string.Format("{0} must be within the range {1} - {2}.  The value given was {3}.", new object[]
                {
                    paramName,
                    rangeBegin,
                    rangeEnd,
                    value
                }));
            }
        }

        public static void ArgumentIsTrue(bool argument, string message)
        {
            if (!argument)
            {
                throw new ArgumentException(message);
            }
        }

        public static void ArgumentIsTrue(bool argument, string messageTemplate, params object[] parameters)
        {
            if (!argument)
            {
                throw new ArgumentException(string.Format(messageTemplate, parameters));
            }
        }

        public static void ArgumentIsFalse(bool argument, string message)
        {
            ArgumentIsTrue(!argument, message);
        }

        public static void ArgumentIsFalse(bool argument, string messageTemplate, params object[] parameters)
        {
            ArgumentIsTrue(!argument, messageTemplate, parameters);
        }

        public static void ArgumentIsRegex(string regex, params string[] values)
        {
            Regex regex2 = new Regex(regex);
            foreach (string text in values)
            {
                if (!regex2.IsMatch(text))
                {
                    throw new ArgumentOutOfRangeException(text, string.Format("{0} must be accorded with the regex。The value given was {0}.", text));
                }
            }
        }

        public static void ArgumentIsGuid(params string[] values)
        {
            ArgumentIsRegex("[A-Fa-f0-9]{8}(-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}", values);
        }

        public static void ArgumentIsGuidList(IEnumerable<string> values)
        {
            foreach (string current in values)
            {
                ArgumentIsGuid(current);
            }
        }

        public static void ArgumentAssistType<T>(params object[] objs)
        {
            if (objs.Any(obj => !(obj is T)))
            {
                throw new ArgumentOutOfRangeException(string.Format("the value isn't {0} Type", typeof(T).Name));
            }
        }

        public static void ArgumentIsFile(string filePath)
        {
            ArgumentNotNullOrEmpty(filePath);

            if (!File.Exists(filePath))
            {
                throw new ArgumentException(string.Format("{0}文件不存在", filePath));
            }
        }

        public static void ArgumentNotNullOrEmpty(params string[] para)
        {
            foreach (string text in para)
            {
                if (string.IsNullOrEmpty(text))
                {
                    throw new ArgumentNullException(text, "the string value is null or empty");
                }
            }
        }
    }

    public static class Error
    {
        [DebuggerStepThrough]
        public static Exception Application(string message, params object[] args)
        {
            return new ApplicationException(message.FormatCurrent(args));
        }

        [DebuggerStepThrough]
        public static Exception Application(Exception innerException, string message, params object[] args)
        {
            return new ApplicationException(message.FormatCurrent(args), innerException);
        }

        [DebuggerStepThrough]
        public static Exception ArgumentNullOrEmpty(Func<string> arg)
        {
            var argName = arg.Method.Name;
            return new ArgumentException("String parameter '{0}' cannot be null or all whitespace.", argName);
        }

        [DebuggerStepThrough]
        public static Exception ArgumentNull(string argName)
        {
            return new ArgumentNullException(argName);
        }

        [DebuggerStepThrough]
        public static Exception ArgumentNull<T>(Func<T> arg)
        {
            var message = "Argument of type '{0}' cannot be null".FormatInvariant(typeof(T));
            var argName = arg.Method.Name;
            return new ArgumentNullException(argName, message);
        }

        [DebuggerStepThrough]
        public static Exception ArgumentOutOfRange<T>(Func<T> arg)
        {
            var argName = arg.Method.Name;
            return new ArgumentOutOfRangeException(argName);
        }

        [DebuggerStepThrough]
        public static Exception ArgumentOutOfRange(string argName)
        {
            return new ArgumentOutOfRangeException(argName);
        }

        [DebuggerStepThrough]
        public static Exception ArgumentOutOfRange(string argName, string message, params object[] args)
        {
            return new ArgumentOutOfRangeException(argName, String.Format(CultureInfo.CurrentCulture, message, args));
        }

        [DebuggerStepThrough]
        public static Exception Argument(string argName, string message, params object[] args)
        {
            return new ArgumentException(String.Format(CultureInfo.CurrentCulture, message, args), argName);
        }

        [DebuggerStepThrough]
        public static Exception Argument<T>(Func<T> arg, string message, params object[] args)
        {
            var argName = arg.Method.Name;
            return new ArgumentException(message.FormatCurrent(args), argName);
        }

        [DebuggerStepThrough]
        public static Exception InvalidOperation(string message, params object[] args)
        {
            return Error.InvalidOperation(message, null, args);
        }

        [DebuggerStepThrough]
        public static Exception InvalidOperation(string message, Exception innerException, params object[] args)
        {
            return new InvalidOperationException(message.FormatCurrent(args), innerException);
        }

        [DebuggerStepThrough]
        public static Exception InvalidOperation<T>(string message, Func<T> member)
        {
            return InvalidOperation<T>(message, null, member);
        }

        [DebuggerStepThrough]
        public static Exception InvalidOperation<T>(string message, Exception innerException, Func<T> member)
        {
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(member, "member");

            return new InvalidOperationException(message.FormatCurrent(member.Method.Name), innerException);
        }

        [DebuggerStepThrough]
        public static Exception InvalidCast(Type fromType, Type toType)
        {
            return InvalidCast(fromType, toType, null);
        }

        [DebuggerStepThrough]
        public static Exception InvalidCast(Type fromType, Type toType, Exception innerException)
        {
            return new InvalidCastException("Cannot convert from type '{0}' to '{1}'.".FormatCurrent(fromType.FullName, toType.FullName), innerException);
        }

        [DebuggerStepThrough]
        public static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        [DebuggerStepThrough]
        public static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        [DebuggerStepThrough]
        public static Exception ObjectDisposed(string objectName)
        {
            return new ObjectDisposedException(objectName);
        }

        [DebuggerStepThrough]
        public static Exception ObjectDisposed(string objectName, string message, params object[] args)
        {
            return new ObjectDisposedException(objectName, String.Format(CultureInfo.CurrentCulture, message, args));
        }

        [DebuggerStepThrough]
        public static Exception NoElements()
        {
            return new InvalidOperationException("Sequence contains no elements.");
        }

        [DebuggerStepThrough]
        public static Exception MoreThanOneElement()
        {
            return new InvalidOperationException("Sequence contains more than one element.");
        }
    }
}