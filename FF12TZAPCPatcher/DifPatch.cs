using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace FF12TZAPCPatcher
{
    public class DifPatch : IPatch
    {
        public static readonly Regex DifMatch =
            new Regex("([0-9a-fA-F]+): ([0-9a-fA-F]+) ([0-9a-fA-F]+)", RegexOptions.Compiled);

        private readonly ByteDif[] _difs;

        public DifPatch(string name, ByteDif[] difs, string description = "")
        {
            this.Name = name;
            this._difs = difs;
            this.Description = description;
        }

        public string Name { get; }

        public string Description { get; }

        public static (ByteDif[] difs, string desc) LoadFromFile(string path)
        {
            var dif = File.ReadAllText(path);
            var matches = DifMatch.Matches(dif);
            var rtn = new ByteDif[matches.Count];
            Match lastMatch = null;
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var vals = match.Value.Replace(":", string.Empty).Split(null);
                var offset = long.Parse(vals[0], NumberStyles.HexNumber);
                var og = byte.Parse(vals[1], NumberStyles.HexNumber);
                var n = byte.Parse(vals[2], NumberStyles.HexNumber);
                rtn[i] = new ByteDif(offset, og, n);
                lastMatch = match;
            }

            var desc = string.Empty;
            if (lastMatch != null)
            {
                desc = dif.Substring(lastMatch.Index + lastMatch.Value.Length + 2);
            }

            return (rtn, desc);
        }

        #region Patching

        public void Apply(FileStream stream)
        {
            for (var i = 0; i < this._difs.Length; i++)
            {
                var val = this._difs[i];
                if (stream.Length < val.Offset)
                {
                    throw new Exception(
                        $"Offset({val.Offset}) greater than file length({stream.Length}) in {this.Name}");
                }

                stream.Position = val.Offset;
                stream.WriteByte(val.NewByte);
            }
        }

        public void Remove(FileStream stream)
        {
            for (var i = 0; i < this._difs.Length; i++)
            {
                var val = this._difs[i];
                if (stream.Length < val.Offset)
                {
                    throw new Exception(
                        $"Offset({val.Offset}) greater than file length({stream.Length}) in {this.Name}");
                }

                stream.Position = val.Offset;
                stream.WriteByte(val.OriginalByte);
            }
        }

        public PatchStatus GetStatus(FileStream stream)
        {
            var oC = 0;
            var nC = 0;
            for (var i = 0; i < this._difs.Length; i++)
            {
                var val = this._difs[i];
                if (stream.Length < val.Offset)
                {
                    throw new Exception(
                        $"Offset({val.Offset}) greater than file length({stream.Length}) in {this.Name}");
                }

                stream.Position = val.Offset;
                var b = stream.ReadByte();
                if (b == val.OriginalByte)
                {
                    oC++;
                }
                else if (b == val.NewByte)
                {
                    nC++;
                }
                else
                {
                    return PatchStatus.Error;
                }
            }

            if (oC == this._difs.Length)
            {
                return PatchStatus.Normal;
            }

            if (nC == this._difs.Length)
            {
                return PatchStatus.Applied;
            }

            return PatchStatus.Error;
        }

        #endregion

        #region Equality

        public bool Equals(IPatch other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Name == other.Name;
        }

        public bool Equals(IPatch x, IPatch y)
        {
            return x != null && x.Equals(y);
        }

        public int GetHashCode(IPatch obj)
        {
            return obj.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return !(obj is null) && this.Equals(obj as IPatch);
        }

        public override int GetHashCode()
        {
            return this.Name != null ? this.Name.GetHashCode() : 0;
        }

        #endregion
    }
}