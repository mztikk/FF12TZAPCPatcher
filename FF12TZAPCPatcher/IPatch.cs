using System;
using System.Collections.Generic;
using System.IO;

namespace FF12TZAPCPatcher
{
    public interface IPatch : IEquatable<IPatch>, IEqualityComparer<IPatch>
    {
        string Name { get; }
        string Description { get; }

        void Apply(FileStream stream);
        void Remove(FileStream stream);
        PatchStatus GetStatus(FileStream stream);
    }
}