using System;
using System.IO;
using Orient.Client.Protocol;

// ReSharper disable UnusedMember.Global

namespace Orient.Client
{
    public sealed class Orid : IEquatable<Orid>
    {
        #region Automatic Properties

        internal static int ClusterIdSize { get; } = OClient.ProtocolVersion < 31 ? sizeof(short) : sizeof(int);

        public int ClusterId { get; }

        public long ClusterPosition { get; }

        public static Orid Null { get; } = new Orid();

        #endregion

        #region Constructors

        /// <summary>
        /// Default Orid constructor.
        /// </summary>
        public Orid() 
            :this(-1, -1)
        {
        }

        /// <summary>
        /// Create a new Orid from another Orid
        /// </summary>
        /// <param name="other">Orid</param>
        public Orid(Orid other)
        {
            ClusterId = other.ClusterId;
            ClusterPosition = other.ClusterPosition;
        }

        /// <summary>
        /// Construct Orid from cluster id and cluster position.
        /// </summary>
        /// <param name="clusterId">cluster id</param>
        /// <param name="clusterPosition">cluster position</param>
        public Orid(int clusterId, long clusterPosition)
        {
            ClusterId = clusterId;
            ClusterPosition = clusterPosition;
        }

        /// <summary>
        /// Construct Orid from string.
        /// </summary>
        /// <param name="source">source string</param>
        public Orid(string source)
            : this(source, 0)
        {
        }

        /// <summary>
        /// Create Orid from string
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="offset">offset within source string</param>
        private Orid(string source, int offset)
        {
            long[] parseResults = ParseInternal(source, offset);
            ClusterId = (int)parseResults[0];
            ClusterPosition = parseResults[1];
        }

        /// <summary>
        /// Construct Orid from bytes.
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        public Orid(byte[] bytes)
        {
            var idBytes = SubArray(bytes, 0, ClusterIdSize);
            var posBytes = SubArray(bytes, ClusterIdSize, sizeof(long));

            ClusterId = (ClusterIdSize == sizeof(short))
                ? BitConverter.ToInt16(idBytes, 0)
                : BitConverter.ToInt32(idBytes, 0);

            ClusterPosition = BitConverter.ToInt64(posBytes, 0);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Equality check
        /// </summary>
        /// <param name="other">orid</param>
        /// <returns>true or false</returns>
        public bool Equals(Orid other)
        {
            if (other == null)
                return false;

            return ClusterId == other.ClusterId && ClusterPosition == other.ClusterPosition;
        }

        /// <summary>
        /// Create Orid from string
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="offset">source offset</param>
        public static Orid Parse(string source, int offset = 0)
        {
            return new Orid(source, offset);
        }

        /// <summary>
        /// Parse Orid from binary reader.
        /// </summary>
        /// <param name="reader">binary reader</param>
        /// <returns>orid</returns>
        internal static Orid Parse(BinaryReader reader)
        {
            return new Orid(ClusterIdSize == sizeof(short)
                ? reader.ReadInt16EndianAware()
                : reader.ReadInt32EndianAware()
                , reader.ReadInt64EndianAware());
        }

        public static bool TryParse(string source, out Orid orid)
        {
            return TryParse(source, 0, out orid);
        }

        public static bool TryParse(string source, int offset, out Orid orid)
        {
            try
            {
                orid = Parse(source, offset);
                return true;
            }
            catch (Exception)
            {
                orid = new Orid();
                return false;
            }
        }

        /// <summary>
        /// Convert Orid to byte array
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] ToByteArray(bool returnBigEndian = false)
        {
            var positionSize = sizeof(long);
            var idBytes = new byte[ClusterIdSize];
            var posBytes = new byte[sizeof(long)];
            var bytes = new byte[ClusterIdSize + positionSize];

            Array.Copy(BitConverter.GetBytes(ClusterId), idBytes, ClusterIdSize);
            Array.Copy(BitConverter.GetBytes(ClusterPosition), posBytes, positionSize);

            if (returnBigEndian && BitConverter.IsLittleEndian)
            {
                Array.Reverse(idBytes);
                Array.Reverse(posBytes);
            }

            Array.Copy(idBytes, 0, bytes, 0, ClusterIdSize);
            Array.Copy(posBytes, 0, bytes, ClusterIdSize, positionSize);

            // Return the resultant byte array.
            return bytes;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // if parameter cannot be cast to Orid return false.
            Orid orid = obj as Orid;
            return (orid != null) && Equals(orid);
        }

        /// <summary>
        /// Gets the Orid hash code
        /// </summary>
        /// <returns>Orid hash code</returns>
        public override int GetHashCode()
        {
            return (ClusterId * 17) ^ ClusterPosition.GetHashCode();
        }

        /// <summary>
        /// Convert Orid to a string.
        /// </summary>
        /// <returns>Orid string representation</returns>
        public override string ToString()
        {
            return $"#{ClusterId}:{ClusterPosition}";
        }

        #endregion

        #region Operators
        public static bool operator ==(Orid left, Orid right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Orid left, Orid right)
        {
            return !(left == right);
        }

        public static explicit operator Guid(Orid orid)
        {
            if (orid == Null) return Guid.Empty;
            var bytes = orid.ToByteArray();
            var guidBytes = new byte[16];
            Array.Copy(bytes, guidBytes, bytes.Length);
            var guid = new Guid(guidBytes);
            return guid;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Return sub array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns>sub array</returns>
        public static byte[] SubArray(byte[] data, int index, int length)
        {
            var result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        /// <summary>
        /// Parse string to integer.
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="index">index position in string to parse</param>
        /// <returns>array of long integer resultsue</returns>
        private static long[] ParseInternal(string source, int index)
        {
            int state = 0;
            const int exit = -1;
            long[] results = { 0L, 0L };
            int resultsIndex = 0;
            int sign = 0;

            while (state != exit)
            {
                switch (state)
                {
                    case 0: // Initial
                        if (source[index] == '#')
                        {
                            state = 1;
                            index += 1;
                        }
                        else
                        {
                            throw new FormatException("RID missing leading # sign.");
                        }
                        break;

                    case 1: // Sign
                        if (source[index] == '-')
                        {
                            sign = -1;
                            state = 2;
                            index += 1;
                        }
                        else if (char.IsDigit(source[index]))
                        {
                            sign = 1;
                            state = 2;
                        }
                        else
                        {
                            throw new FormatException("Incorrect RID format.");
                        }
                        break;

                    case 2: // Digit
                        if (index < source.Length && char.IsDigit(source[index]))
                        {
                            int digit = source[index] - '0';
                            results[resultsIndex] = results[resultsIndex] * 10 + digit;
                            index += 1;
                        }
                        else
                        {
                            state = 3;
                        }

                        break;

                    case 3: // End of Number
                        if (resultsIndex == 0)
                        {
                            if (source[index] == ':')
                            {
                                // Completed parsing cluster id; goto the sign state to parse position.
                                state = 1;
                            }
                            else
                            {
                                throw new FormatException($"Illegal cluster:offset separator '{source[index]}' found.");
                            }
                        }
                        else
                        {
                            state = exit;
                        }

                        results[resultsIndex++] *= sign;
                        sign = 1;
                        index += 1;
                        break;

                    default: // Invalid state
                        state = exit;
                        break;
                }
            }

            return results;
        }

        #endregion
    }
}
