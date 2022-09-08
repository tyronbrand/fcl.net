using System;
using System.Runtime.Serialization;

namespace Fcl.Net.Core.Exceptions
{
    [Serializable]
    public class FclException : Exception
    {
        public FclException(string message) : base(message) { }
        public FclException(string message, Exception inner) : base(message, inner) { }

        protected FclException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
