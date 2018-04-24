using System;
using System.IO;
using System.Runtime.Serialization;

namespace FF12TZAPCPatcher
{
    [DataContract]
    public class BytePatch : IPatch
    {
        [DataMember(Order = 2)]
        protected readonly byte[] BytesToPatch;

        [DataMember(Order = 1)]
        protected readonly long Offset;

        [DataMember(Order = 3)]
        protected readonly byte[] OriginalBytes;

        #region Constructor

        public BytePatch(string name, long offset, byte[] originalBytes, byte[] bytesToPatch, string description = "")
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
            this.Description = description;
        }

        #endregion

        [DataMember(Order = 0)]
        public string Name { get; private set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = true)]
        public string Description { get; private set; }

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

        #region Equality

        public bool Equals(IPatch x, IPatch y)
        {
            if (x is null || y is null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            return x.Name == y.Name;
        }

        public bool Equals(IPatch other)
        {
            return this.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return !(obj is null) && this.Equals(obj as IPatch);
        }

        public int GetHashCode(IPatch obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion

        #region Patching

        public void Apply(FileStream stream)
        {
            if (stream.Length < this.Offset)
            {
                throw new Exception($"Offset({this.Offset}) greater than file length({stream.Length}) in {this.Name}");
            }

            stream.Position = this.Offset;
            stream.Write(this.BytesToPatch, 0, this.BytesToPatch.Length);
        }

        public void Remove(FileStream stream)
        {
            if (stream.Length < this.Offset)
            {
                throw new Exception($"Offset({this.Offset}) greater than file length({stream.Length}) in {this.Name}");
            }

            stream.Position = this.Offset;
            stream.Write(this.OriginalBytes, 0, this.OriginalBytes.Length);
        }

        public PatchStatus GetStatus(FileStream stream)
        {
            if (stream.Length < this.Offset)
            {
                throw new Exception($"Offset({this.Offset}) greater than file length({stream.Length}) in {this.Name}");
            }

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
    }
}