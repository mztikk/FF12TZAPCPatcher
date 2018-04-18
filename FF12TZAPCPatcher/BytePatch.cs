using System;
using System.IO;
using System.Runtime.Serialization;

namespace FF12TZAPCPatcher
{
    [DataContract]
    public class BytePatch : IEquatable<BytePatch>, IComparable<BytePatch>
    {
        [DataMember(Order = 2)]
        protected readonly byte[] BytesToPatch;

        [DataMember(Order = 0)]
        public readonly string Name;

        [DataMember(Order = 1)]
        protected readonly long Offset;

        [DataMember(Order = 3)]
        protected readonly byte[] OriginalBytes;

        #region Constructor

        public BytePatch(string name, long offset, byte[] originalBytes, byte[] bytesToPatch)
        {
            if (originalBytes.Length != bytesToPatch.Length)
            {
                throw new ArgumentException($"Length of bytes has to be the same",
                    $"{nameof(originalBytes)} and {nameof(bytesToPatch)}");
            }

            this.Name = name;
            this.Offset = offset;
            this.OriginalBytes = originalBytes;
            this.BytesToPatch = bytesToPatch;
        }

        #endregion

        #region IComparable<BytePatch>

        public int CompareTo(BytePatch other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            return this.Offset.CompareTo(other.Offset);
        }

        #endregion

        #region Serialization

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (this.BytesToPatch.Length != this.OriginalBytes.Length)
            {
                throw new ArgumentException($"Length of bytes has to be the same in {this.Name}",
                    $"{nameof(this.BytesToPatch)} and {nameof(this.OriginalBytes)}");
            }
        }

        #endregion

        public override string ToString()
        {
            return
                $"{{Name: {this.Name}, Offset: {this.Offset}, BytesToPatch: [{string.Join(", ", this.BytesToPatch)}], OriginalBytes: [{string.Join(", ", this.OriginalBytes)}]}}";
        }

        #region Patching

        public void Apply(FileStream stream)
        {
            stream.Position = this.Offset;
            stream.Write(this.BytesToPatch, 0, this.BytesToPatch.Length);
        }

        public void Remove(FileStream stream)
        {
            stream.Position = this.Offset;
            stream.Write(this.OriginalBytes, 0, this.OriginalBytes.Length);
        }

        public PatchStatus GetStatus(FileStream stream)
        {
            stream.Position = this.Offset;
            var buffer = new byte[this.BytesToPatch.Length];
            stream.Read(buffer, 0, buffer.Length);

            if (FastCompare.Equal(buffer, this.BytesToPatch))
            {
                return PatchStatus.Applied;
            }

            if (FastCompare.Equal(buffer, this.OriginalBytes))
            {
                return PatchStatus.Normal;
            }

            return PatchStatus.Error;
        }

        #endregion

        #region IEquatable<BytePatch>

        public bool Equals(BytePatch other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FastCompare.Equal(this.OriginalBytes, other.OriginalBytes) &&
                   FastCompare.Equal(this.BytesToPatch, other.BytesToPatch) && this.Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((BytePatch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.BytesToPatch.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Offset.GetHashCode();
                hashCode = (hashCode * 397) ^ this.OriginalBytes.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}