using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Synergy.Common.Exceptions
{
#pragma warning disable CA1058 // Types should not extend certain base types
    public class ModelStateException : ApplicationException
#pragma warning restore CA1058 // Types should not extend certain base types
    {
        public ModelStateException()
            : base("Model state is invalid")
        {
        }

        public ModelStateException(IEnumerable<KeyValuePair<string, string>> state)
        {
            this.State = state;
        }

        public ModelStateException(string propertyName, string message)
        {
            this.State = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(propertyName, message),
            };
        }

        public ModelStateException(string message)
            : base(message)
        {
        }

        public ModelStateException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public IEnumerable<KeyValuePair<string, string>> State { get; }

        public override string Message => this.State != null ? JsonConvert.SerializeObject(this.State) : base.Message;
    }
}
