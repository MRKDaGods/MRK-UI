using System;

namespace MRK.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LayoutDescriptor : Attribute
    {
        public LayoutType LayoutType
        {
            get; private set;
        }

        public LayoutIdentifier Identifier
        {
            get; private set;
        }

        public LayoutDescriptor(LayoutType type, string identifier)
        {
            LayoutType = type;
            Identifier = new LayoutIdentifier(type, identifier);
        }
    }
}
