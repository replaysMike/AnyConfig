using AnyConfig.Collections;
using System;

namespace AnyConfig
{
    public static class GenericNameValueCollectionExtensions
    {
        /// <summary>
        /// Get value as a specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T As<T>(this StringValue value)
        {
            var type = typeof(T);
            switch (type.Name)
            {
                case "Boolean":
                    return (T)(object)value.AsBool();
                case "Byte":
                    return (T)(object)value.AsByte();
                case "SByte":
                    return (T)(object)value.AsSByte();
                case "Char":
                    return (T)(object)value.AsChar();
                case "Int16":
                    return (T)(object)value.AsInt16();
                case "UInt16":
                    return (T)(object)value.AsUInt16();
                case "Int32":
                    return (T)(object)value.AsInt32();
                case "UInt32":
                    return (T)(object)value.AsUInt32();
                case "Int64":
                    return (T)(object)value.AsInt64();
                case "UInt64":
                    return (T)(object)value.AsUInt64();
                case "Single":
                    return (T)(object)value.AsFloat();
                case "Double":
                    return (T)(object)value.AsDouble();
                case "Decimal":
                    return (T)(object)value.AsDecimal();
                case "String":
                    return (T)(object)value.Value;
                default:
                    if (type.IsEnum)
                    {
                        return (T)Enum.Parse(type, value.Value);
                    }

                    // try converting the type
                    return (T)Convert.ChangeType(value.Value, type);
            }
        }

        /// <summary>
        /// Get value as boolean
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AsBool(this StringValue value) => bool.Parse(value.Value);

        /// <summary>
        /// Get value as a byte
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte AsByte(this StringValue value) => byte.Parse(value.Value);

        /// <summary>
        /// Get value as a signed byte
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static sbyte AsSByte(this StringValue value) => sbyte.Parse(value.Value);

        /// <summary>
        /// Get value as a character
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static char AsChar(this StringValue value) => char.Parse(value.Value);

        /// <summary>
        /// Get value as a short
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static short AsInt16(this StringValue value) => short.Parse(value.Value);

        /// <summary>
        /// Get value as an unsigned short
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ushort AsUInt16(this StringValue value) => ushort.Parse(value.Value);

        /// <summary>
        /// Get value as an integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int AsInt32(this StringValue value) => int.Parse(value.Value);

        /// <summary>
        /// Get value as an unsigned integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint AsUInt32(this StringValue value) => uint.Parse(value.Value);

        /// <summary>
        /// Get value as a long
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long AsInt64(this StringValue value) => long.Parse(value.Value);

        /// <summary>
        /// Get value as an unsigned long
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ulong AsUInt64(this StringValue value) => ulong.Parse(value.Value);

        /// <summary>
        /// Get value as a float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsFloat(this StringValue value) => float.Parse(value.Value);

        /// <summary>
        /// Get value as a double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double AsDouble(this StringValue value) => double.Parse(value.Value);

        /// <summary>
        /// Get value as a decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal AsDecimal(this StringValue value) => decimal.Parse(value.Value);
    }
}
