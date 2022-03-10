using System;
using System.Collections.Generic;

namespace MRK.UI
{
    public struct LayoutIdentifier
    {
        private readonly static HashSet<LayoutIdentifier> _screenIdentifiers;

        public string ContextualIdentifier
        {
            get; private set;
        }

        public string FullIdentifier
        {
            get; private set;
        }

        public static HashSet<LayoutIdentifier> ScreenIdentifiers
        {
            get { return _screenIdentifiers; }
        }

        static LayoutIdentifier()
        {
            _screenIdentifiers = new HashSet<LayoutIdentifier>();
        }

        public LayoutIdentifier(LayoutType layoutType, string ctx)
        {
            ContextualIdentifier = ctx;
            FullIdentifier = $"{layoutType}-{ctx}";

            if (layoutType == LayoutType.Screen)
            {
                _screenIdentifiers.Add(this);
            }
        }

        public static bool operator ==(LayoutIdentifier lhs, LayoutIdentifier rhs)
        {
            return lhs.FullIdentifier == rhs.FullIdentifier;
        }

        public static bool operator !=(LayoutIdentifier lhs, LayoutIdentifier rhs)
        {
            return lhs.FullIdentifier != rhs.FullIdentifier;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutIdentifier identifier &&
                   FullIdentifier == identifier.FullIdentifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FullIdentifier);
        }
    }
}
