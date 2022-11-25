using System;

namespace Fab.Dialog
{
    public sealed class StateNodeAttribute : Attribute
    {
        public string Name { get; private set; }
        public Guid Identifier { get; private set; }

        public StateNodeAttribute(string name, string identifier)
        {
            Name = name;
            if(!Guid.TryParse(identifier, out Guid id))
            {
                throw new ArgumentException($"The identifier string of {name} is not a valid GUID.");
            }
            Identifier = id;
        }
    }
}
