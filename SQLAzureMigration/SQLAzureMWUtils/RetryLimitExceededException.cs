#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// The special type of exception that provides managed exit from a retry loop. The user code can use this
    /// exception to notify the retry policy that no further retry attempts are required.
    /// </summary>
    [Serializable]
    public sealed class RetryLimitExceededException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryLimitExceededException"/> class with a default error message.
        /// </summary>
        public RetryLimitExceededException() : this(Properties.Resources.RetryLimitExceeded)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryLimitExceededException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RetryLimitExceededException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryLimitExceededException"/> class with a reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RetryLimitExceededException(Exception innerException) : base(innerException != null ? innerException.Message : Properties.Resources.RetryLimitExceeded, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryLimitExceededException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RetryLimitExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryLimitExceededException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).</exception>
        private RetryLimitExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
